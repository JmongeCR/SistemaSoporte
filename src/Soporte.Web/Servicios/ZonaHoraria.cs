namespace Soporte.Web.Servicios;

public class ZonaHoraria : IZonaHoraria
{
    private readonly TimeZoneInfo _zona;

    /// <param name="idZona">Id IANA o Windows (ej. "America/Costa_Rica"), leído de appsettings.json.</param>
    public ZonaHoraria(string idZona)
    {
        _zona = TimeZoneInfo.FindSystemTimeZoneById(idZona);
    }

    public DateTime ALocal(DateTime fechaUtc)
    {
        return TimeZoneInfo.ConvertTimeFromUtc(DateTime.SpecifyKind(fechaUtc, DateTimeKind.Utc), _zona);
    }
}
