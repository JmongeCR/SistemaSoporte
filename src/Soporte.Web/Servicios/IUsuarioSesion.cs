using Soporte.Abstracciones.Enumeraciones;

namespace Soporte.Web.Servicios;

/// <summary>Usuario que está usando la interfaz (menú, opciones por rol, formularios).</summary>
public interface IUsuarioSesion
{
    bool EstaAutenticado { get; }
    int UsuarioId { get; }
    string Nombre { get; }
    string Correo { get; }
    RolTipo Rol { get; }
}
