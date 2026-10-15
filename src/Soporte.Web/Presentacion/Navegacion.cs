using Soporte.Abstracciones.Enumeraciones;

namespace Soporte.Web.Presentacion;

/// <summary>Destinos de navegación que dependen del rol (HU-01 esc. 1).</summary>
public static class Navegacion
{
    /// <summary>
    /// Página de inicio tras el login: el Dashboard, que muestra a cada rol su propio contenido
    /// (métricas del equipo para Administrador y Agente; resumen de sus casos para el Cliente).
    /// </summary>
    public static string InicioSegunRol(RolTipo rol) => "/Dashboard";
}
