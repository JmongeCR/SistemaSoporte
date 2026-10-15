using Soporte.Abstracciones.Enumeraciones;

namespace Soporte.Web.Presentacion;

/// <summary>Ayudas de presentación compartidas por todas las páginas (clases CSS del sistema de diseño e iniciales).</summary>
public static class Formato
{
    /// <summary>"Luis Vargas" → "LV"; "Clínica San José" → "CS".</summary>
    public static string Iniciales(string? nombre)
    {
        var palabras = (nombre ?? string.Empty).Split(' ', StringSplitOptions.RemoveEmptyEntries);
        return string.Concat(palabras.Take(2).Select(p => char.ToUpperInvariant(p[0])));
    }

    /// <summary>Tiempo transcurrido legible: "hace 5 min", "hace 3 h", "ayer", "hace 4 días".</summary>
    public static string Hace(DateTime fechaUtc)
    {
        var transcurrido = DateTime.UtcNow - DateTime.SpecifyKind(fechaUtc, DateTimeKind.Utc);
        return transcurrido.TotalMinutes switch
        {
            < 1 => "hace un momento",
            < 60 => $"hace {(int)transcurrido.TotalMinutes} min",
            < 60 * 24 => $"hace {(int)transcurrido.TotalHours} h",
            < 60 * 48 => "ayer",
            < 60 * 24 * 30 => $"hace {(int)transcurrido.TotalDays} días",
            _ => $"hace {(int)(transcurrido.TotalDays / 30)} meses"
        };
    }

    public static string ClaseEstado(int estadoId) => (EstadoTicketTipo)estadoId switch
    {
        EstadoTicketTipo.Abierto => "pill-abierto",
        EstadoTicketTipo.EnProgreso => "pill-progreso",
        EstadoTicketTipo.Resuelto => "pill-resuelto",
        _ => "pill-cerrado"
    };

    public static string ClasePrioridad(int prioridadId) => (PrioridadTipo)prioridadId switch
    {
        PrioridadTipo.Urgente => "prioridad-urgente",
        PrioridadTipo.Alta => "prioridad-alta",
        PrioridadTipo.Media => "prioridad-media",
        _ => "prioridad-baja"
    };
}
