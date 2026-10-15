namespace Soporte.Web.Servicios;

/// <summary>El API entrega fechas en UTC; la interfaz las muestra en la hora local configurada.</summary>
public interface IZonaHoraria
{
    DateTime ALocal(DateTime fechaUtc);
}
