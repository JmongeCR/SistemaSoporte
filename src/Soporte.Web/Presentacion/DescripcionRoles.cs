using Soporte.Abstracciones.Enumeraciones;

namespace Soporte.Web.Presentacion;

/// <summary>Textos del diseño "06 - Roles y Permisos" para cada rol del sistema.</summary>
public static class DescripcionRoles
{
    public record Descripcion(string Titulo, string Resumen, string Detalle, string AvisoAlAsignar);

    public static Descripcion De(int rolId) => (RolTipo)rolId switch
    {
        RolTipo.Administrador => new("Administrador",
            "Gestiona la plataforma y sus accesos",
            "Gestiona la plataforma y sus accesos. Administra todos los tickets, asigna agentes y controla los roles de los usuarios.",
            "Este usuario podrá consultar todos los tickets y administrar los accesos de la plataforma."),
        RolTipo.Agente => new("Agente de soporte",
            "Atiende y da seguimiento a solicitudes",
            "Atiende los tickets asignados y los que están sin asignar, conversa con los clientes y registra notas internas.",
            "Este usuario podrá tomar tickets sin asignar, responder a clientes y escribir notas internas."),
        _ => new("Cliente",
            "Crea y consulta sus propias solicitudes",
            "Reporta problemas, da seguimiento a sus propios tickets y responde las consultas del equipo.",
            "Este usuario solo podrá crear y consultar sus propios tickets.")
    };

    public static string ClaseRol(int rolId) => (RolTipo)rolId switch
    {
        RolTipo.Administrador => "rol-administrador",
        RolTipo.Agente => "rol-agente",
        _ => "rol-cliente"
    };
}
