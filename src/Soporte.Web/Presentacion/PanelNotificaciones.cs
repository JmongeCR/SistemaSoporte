using Soporte.Abstracciones.DTOs;

namespace Soporte.Web.Presentacion;

/// <summary>Modelo del panel desplegable de la campana.</summary>
public class PanelNotificaciones
{
    public IReadOnlyList<NotificacionDto> Elementos { get; init; } = Array.Empty<NotificacionDto>();
    public string? MensajeError { get; init; }

    /// <summary>Página desde la que se abrió el panel (para volver después de "Marcar todas como leídas").</summary>
    public string? VolverA { get; init; }
}
