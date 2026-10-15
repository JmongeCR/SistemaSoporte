using System.Threading.RateLimiting;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Soporte.Abstracciones.Enumeraciones;
using Soporte.Web.Presentacion;
using Soporte.Web.Servicios;

var builder = WebApplication.CreateBuilder(args);

// ---------- Páginas: todo exige sesión salvo /Cuenta (login y recuperación) ----------
builder.Services.AddRazorPages(opciones =>
{
    opciones.Conventions.AuthorizeFolder("/");
    // HU-14 a HU-16: la sección de roles se protege en el servidor, no solo ocultando el menú.
    opciones.Conventions.AuthorizeFolder("/Roles", PoliticaSoloAdministrador);
    opciones.Conventions.AllowAnonymousToFolder("/Cuenta");
    opciones.Conventions.AllowAnonymousToPage("/Error");
});

// ---------- Sesión con cookie (HU-01, HU-03) ----------
var minutosInactividad = builder.Configuration.GetValue("Sesion:MinutosInactividad", 30);

builder.Services.AddAuthentication(CookieAuthenticationDefaults.AuthenticationScheme)
    .AddCookie(opciones =>
    {
        opciones.LoginPath = "/Cuenta/Login";
        opciones.LogoutPath = "/Cuenta/Salir";
        opciones.AccessDeniedPath = "/AccesoDenegado";
        opciones.Cookie.Name = "Soporte.Sesion";
        opciones.Cookie.HttpOnly = true;
        opciones.Cookie.SameSite = SameSiteMode.Lax;
        opciones.Cookie.SecurePolicy = CookieSecurePolicy.SameAsRequest;

        // HU-03 esc. 2: la sesión vence tras N minutos sin actividad (cada petición la renueva).
        opciones.ExpireTimeSpan = TimeSpan.FromMinutes(minutosInactividad);
        opciones.SlidingExpiration = true;

        // La consulta periódica de la campana no cuenta como actividad del usuario: no extiende
        // la sesión, así sigue venciendo por inactividad (HU-03 esc. 2).
        opciones.Events.OnCheckSlidingExpiration = contexto =>
        {
            if (contexto.Request.Headers.ContainsKey(EncabezadoConsultaAutomatica))
                contexto.ShouldRenew = false;
            return Task.CompletedTask;
        };

        // Si el token del API venció, la sesión también termina.
        opciones.Events.OnValidatePrincipal = async contexto =>
        {

            if (contexto.Principal is not null && GestorSesionCookie.TokenVencido(contexto.Principal))
            {
                contexto.RejectPrincipal();
                await contexto.HttpContext.SignOutAsync(CookieAuthenticationDefaults.AuthenticationScheme);
            }
        };
    });
builder.Services.AddAuthorization(opciones =>
    opciones.AddPolicy(PoliticaSoloAdministrador, politica => politica.RequireRole(nameof(RolTipo.Administrador))));
builder.Services.AddHttpContextAccessor();
builder.Services.AddScoped<IUsuarioSesion, UsuarioSesionCookie>();
builder.Services.AddScoped<IGestorSesion, GestorSesionCookie>();

// ---------- Límite de intentos de login y recuperación por IP ----------
builder.Services.AddRateLimiter(opciones =>
{
    opciones.RejectionStatusCode = StatusCodes.Status429TooManyRequests;
    opciones.AddPolicy(PoliticasLimite.Intentos, contexto =>
        HttpMethods.IsPost(contexto.Request.Method)
            ? RateLimitPartition.GetFixedWindowLimiter(
                contexto.Connection.RemoteIpAddress?.ToString() ?? "desconocida",
                _ => new FixedWindowRateLimiterOptions { PermitLimit = 10, Window = TimeSpan.FromMinutes(1) })
            : RateLimitPartition.GetNoLimiter("lecturas"));
    opciones.OnRejected = async (contexto, cancelacion) =>
    {
        contexto.HttpContext.Response.ContentType = "text/plain; charset=utf-8";
        await contexto.HttpContext.Response.WriteAsync(
            "Demasiados intentos. Esperá un minuto y volvé a intentarlo.", cancelacion);
    };
});

// ---------- Cliente HTTP hacia Soporte.API (única fuente de datos de la interfaz) ----------
var urlApi = builder.Configuration["ApiSoporte:UrlBase"]
    ?? throw new InvalidOperationException("Falta 'ApiSoporte:UrlBase' en appsettings.json.");

builder.Services.AddTransient<TokenSesionHandler>();
builder.Services.AddHttpClient(ApiSoporteCliente.NombreClienteHttp, cliente =>
    {
        cliente.BaseAddress = new Uri(urlApi);
        cliente.Timeout = TimeSpan.FromSeconds(15);
    })
    .AddHttpMessageHandler<TokenSesionHandler>();

builder.Services.AddScoped<IApiSoporteCliente, ApiSoporteCliente>();
builder.Services.AddScoped<IAutenticacionApiServicio, AutenticacionApiServicio>();
builder.Services.AddScoped<ITicketsApiServicio, TicketsApiServicio>();
builder.Services.AddScoped<ICatalogosApiServicio, CatalogosApiServicio>();
builder.Services.AddScoped<IUsuariosApiServicio, UsuariosApiServicio>();
builder.Services.AddScoped<INotificacionesApiServicio, NotificacionesApiServicio>();
builder.Services.AddScoped<IDashboardApiServicio, DashboardApiServicio>();
builder.Services.AddMemoryCache();
builder.Services.AddSingleton<IZonaHoraria>(new ZonaHoraria(builder.Configuration["ZonaHoraria"] ?? "America/Costa_Rica"));

var app = builder.Build();

if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Error");
    app.UseHsts();
    app.UseHttpsRedirection();
}

app.UseStaticFiles();
app.UseRouting();
app.UseRateLimiter();
app.UseAuthentication();
app.UseAuthorization();

// HU-03 esc. 3: las páginas con sesión no se guardan en caché, así el botón "atrás"
// después de cerrar sesión vuelve a pedirlas al servidor (que redirige al login).
app.Use(async (contexto, siguiente) =>
{
    contexto.Response.OnStarting(() =>
    {
        if (contexto.User.Identity?.IsAuthenticated == true)
        {
            contexto.Response.Headers.CacheControl = "no-store, no-cache, must-revalidate";
            contexto.Response.Headers.Pragma = "no-cache";
        }
        return Task.CompletedTask;
    });
    await siguiente();
});

app.MapRazorPages();

app.Run();

public partial class Program
{
    /// <summary>Lo envía el script de la campana en sus consultas periódicas (ver site.js).</summary>
    public const string EncabezadoConsultaAutomatica = "X-Consulta-Automatica";

    public const string PoliticaSoloAdministrador = "SoloAdministrador";
}
