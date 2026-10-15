using Soporte.Abstracciones.DTOs;

namespace Soporte.Web.Servicios;

/// <summary>Crea y elimina la cookie de sesión (HU-01, HU-03).</summary>
public interface IGestorSesion
{
    Task IniciarAsync(SesionDto sesion);
    Task CerrarAsync();
}
