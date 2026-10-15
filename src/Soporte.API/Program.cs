using System.Reflection;
using System.Security.Claims;
using System.Threading.RateLimiting;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Infrastructure;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi.Models;
using Soporte.Abstracciones.DTOs;
using Soporte.Abstracciones.Interfaces.DA;
using Soporte.Abstracciones.Interfaces.Flujo;
using Soporte.Abstracciones.Interfaces.Infraestructura;
using Soporte.Abstracciones.Interfaces.Reglas;
using Soporte.API.Servicios;
using Soporte.DA.Contexto;
using Soporte.DA.Repositorios;
using Soporte.Flujo.Servicios;
using Soporte.Reglas.Reglas;

var builder = WebApplication.CreateBuilder(args);

// ---------- Acceso a datos (Soporte.DA) ----------
var cadenaConexion = builder.Configuration.GetConnectionString("SoporteDb")
    ?? throw new InvalidOperationException("Falta la cadena de conexión 'SoporteDb' en appsettings.json.");

builder.Services.AddDbContext<SoporteDbContext>(opciones =>
    opciones.UseSqlServer(cadenaConexion, sql => sql.EnableRetryOnFailure()));

builder.Services.AddScoped<ITicketRepository, TicketRepository>();
builder.Services.AddScoped<ICatalogoRepository, CatalogoRepository>();
builder.Services.AddScoped<IUsuarioRepository, UsuarioRepository>();
builder.Services.AddScoped<IAdjuntoRepository, AdjuntoRepository>();
builder.Services.AddScoped<ITokenRecuperacionRepository, TokenRecuperacionRepository>();
builder.Services.AddScoped<IComentarioRepository, ComentarioRepository>();
builder.Services.AddScoped<INotificacionRepository, NotificacionRepository>();
builder.Services.AddScoped<IDashboardRepository, DashboardRepository>();
builder.Services.AddScoped<IUnidadDeTrabajo, UnidadDeTrabajo>();

var rutaAdjuntos = builder.Configuration["Adjuntos:RutaBase"]
    ?? throw new InvalidOperationException("Falta 'Adjuntos:RutaBase' en appsettings.json.");
builder.Services.AddSingleton<IAlmacenamientoArchivos>(
    new AlmacenamientoArchivosLocal(Path.Combine(builder.Environment.ContentRootPath, rutaAdjuntos)));

// ---------- Reglas de negocio (Soporte.Reglas) ----------
builder.Services.AddScoped<ITicketReglas, TicketReglas>();
builder.Services.AddScoped<IComentarioReglas, ComentarioReglas>();
builder.Services.AddScoped<IAdjuntoReglas, AdjuntoReglas>();
builder.Services.AddScoped<IUsuarioReglas, UsuarioReglas>();
builder.Services.AddScoped<INotificacionReglas, NotificacionReglas>();
builder.Services.AddSingleton<ISeguridadContrasenas, SeguridadContrasenas>();

// ---------- Flujo (Soporte.Flujo) ----------
builder.Services.AddScoped<ITicketFlujo, TicketFlujo>();
builder.Services.AddScoped<ICatalogoFlujo, CatalogoFlujo>();
builder.Services.AddScoped<IAdjuntoFlujo, AdjuntoFlujo>();
builder.Services.AddScoped<IUsuarioFlujo, UsuarioFlujo>();
builder.Services.AddScoped<IAutenticacionFlujo, AutenticacionFlujo>();
builder.Services.AddScoped<IComentarioFlujo, ComentarioFlujo>();
builder.Services.AddScoped<INotificacionFlujo, NotificacionFlujo>();
builder.Services.AddScoped<IDashboardFlujo, DashboardFlujo>();
builder.Services.AddSingleton(new ConfiguracionRecuperacionDto
{
    UrlRestablecer = builder.Configuration["Recuperacion:UrlRestablecer"]
        ?? throw new InvalidOperationException("Falta 'Recuperacion:UrlRestablecer' en appsettings.json.")
});

// ---------- Autenticación con JWT (HU-01) ----------
var configuracionJwt = builder.Configuration.GetSection("Jwt").Get<ConfiguracionJwt>() ?? new ConfiguracionJwt();
if (configuracionJwt.Clave.Length < ConfiguracionJwt.LongitudMinimaClave)
{
    throw new InvalidOperationException(
        $"Falta 'Jwt:Clave' (mínimo {ConfiguracionJwt.LongitudMinimaClave} caracteres). Configúrela con: " +
        "dotnet user-secrets set \"Jwt:Clave\" \"<clave aleatoria>\" --project src/Soporte.API");
}

builder.Services.AddSingleton(configuracionJwt);
builder.Services.AddSingleton<IGeneradorToken, GeneradorTokenJwt>();
builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
    .AddJwtBearer(opciones =>
    {
        opciones.MapInboundClaims = false;   // conserva los tipos de claim tal como se emitieron
        opciones.TokenValidationParameters = new TokenValidationParameters
        {
            ValidateIssuer = true,
            ValidIssuer = configuracionJwt.Emisor,
            ValidateAudience = true,
            ValidAudience = configuracionJwt.Audiencia,
            ValidateIssuerSigningKey = true,
            IssuerSigningKey = GeneradorTokenJwt.CrearClave(configuracionJwt),
            ValidateLifetime = true,
            ClockSkew = TimeSpan.FromMinutes(1),
            NameClaimType = ClaimTypes.Name,
            RoleClaimType = ClaimTypes.Role
        };

        // El token solo vale si el usuario sigue activo y con el mismo rol (HU-15).
        opciones.Events = new JwtBearerEvents
        {
            OnTokenValidated = async contexto =>
            {
                var validador = contexto.HttpContext.RequestServices.GetRequiredService<IValidadorSesion>();
                if (!await validador.EsVigenteAsync(contexto.Principal!))
                    contexto.Fail("La cuenta cambió de rol o fue desactivada. Inicie sesión de nuevo.");
            }
        };
    });
builder.Services.AddAuthorization();

// ---------- Correo (solo desarrollo: archivos en disco) ----------
if (builder.Environment.IsDevelopment())
{
    var carpetaCorreos = builder.Configuration["Correo:CarpetaSalida"]
        ?? throw new InvalidOperationException("Falta 'Correo:CarpetaSalida' en appsettings.json.");
    builder.Services.AddSingleton<IEnvioCorreo>(proveedor => new EnvioCorreoArchivo(
        Path.Combine(builder.Environment.ContentRootPath, carpetaCorreos),
        proveedor.GetRequiredService<ILogger<EnvioCorreoArchivo>>()));
}
else
{
    throw new InvalidOperationException(
        "No se configuró un proveedor de correo para producción. " +
        "Implemente IEnvioCorreo con SMTP y regístrelo en el contenedor.");
}

// ---------- Rate limiting en endpoints de autenticación ----------
builder.Services.AddRateLimiter(opciones =>
{
    opciones.RejectionStatusCode = StatusCodes.Status429TooManyRequests;
    opciones.AddPolicy(NombrePoliticaAuth, contexto =>
        RateLimitPartition.GetFixedWindowLimiter(
            contexto.Connection.RemoteIpAddress?.ToString() ?? "desconocida",
            _ => new FixedWindowRateLimiterOptions { PermitLimit = 10, Window = TimeSpan.FromMinutes(1) }));
    opciones.OnRejected = async (ctx, ct) =>
    {
        ctx.HttpContext.Response.ContentType = "application/problem+json";
        await ctx.HttpContext.Response.WriteAsync(
            "{\"status\":429,\"title\":\"Demasiados intentos. Esperá un minuto y volvé a intentarlo.\"}", ct);
    };
});

// ---------- API ----------
builder.Services.AddHttpContextAccessor();
builder.Services.AddScoped<IUsuarioActualProveedor, UsuarioActualProveedor>();
builder.Services.AddScoped<IValidadorSesion, ValidadorSesion>();
builder.Services.AddControllers()
    .ConfigureApiBehaviorOptions(opciones =>
    {
        // Errores de DataAnnotations con título en español (la Web lo muestra tal cual).
        opciones.InvalidModelStateResponseFactory = contexto =>
        {
            var fabrica = contexto.HttpContext.RequestServices.GetRequiredService<ProblemDetailsFactory>();
            var problema = fabrica.CreateValidationProblemDetails(
                contexto.HttpContext, contexto.ModelState, StatusCodes.Status400BadRequest, title: "Revise los datos enviados.");
            return new BadRequestObjectResult(problema);
        };
    });
builder.Services.AddProblemDetails();

// ---------- Swagger ----------
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(opciones =>
{
    opciones.SwaggerDoc("v1", new OpenApiInfo
    {
        Title = "Soporte API",
        Version = "v1",
        Description = "API del Sistema de Tickets de Soporte Técnico (SC-701 Programación Avanzada Web)."
    });

    // Comentarios XML de los controladores y de los DTOs de Soporte.Abstracciones.
    foreach (var ensamblado in new[] { Assembly.GetExecutingAssembly(), typeof(ITicketFlujo).Assembly })
    {
        var archivoXml = Path.Combine(AppContext.BaseDirectory, $"{ensamblado.GetName().Name}.xml");
        if (File.Exists(archivoXml))
            opciones.IncludeXmlComments(archivoXml);
    }

    // Botón "Authorize": pegar el token de POST /api/autenticacion/login.
    var esquemaBearer = new OpenApiSecurityScheme
    {
        Type = SecuritySchemeType.Http,
        Scheme = "bearer",
        BearerFormat = "JWT",
        Description = "Token obtenido en POST /api/autenticacion/login.",
        Reference = new OpenApiReference { Type = ReferenceType.SecurityScheme, Id = "Bearer" }
    };
    opciones.AddSecurityDefinition("Bearer", esquemaBearer);
    opciones.AddSecurityRequirement(new OpenApiSecurityRequirement { [esquemaBearer] = Array.Empty<string>() });
});

var app = builder.Build();

app.UseExceptionHandler();
app.UseStatusCodePages();
app.UseRateLimiter();

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI(opciones => opciones.DocumentTitle = "Soporte API");
}
else
{
    app.UseHttpsRedirection();
}

app.UseAuthentication();
app.UseAuthorization();

// Todo endpoint exige token, salvo los marcados con [AllowAnonymous] (login y recuperación).
app.MapControllers().RequireAuthorization();

app.Run();

public partial class Program
{
    public const string NombrePoliticaAuth = "auth";
}
