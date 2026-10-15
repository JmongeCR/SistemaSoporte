namespace Soporte.Abstracciones.Enumeraciones;

/// <summary>Evento que originó una notificación. Los valores se guardan en Notificacion.Tipo.</summary>
public enum TipoNotificacion : byte
{
    CambioEstado = 1,
    RespuestaEquipo = 2,
    SolicitudInformacion = 3,
    Resuelto = 4,
    Reabierto = 5,
    RespuestaCliente = 6,
    Cerrado = 7
}
