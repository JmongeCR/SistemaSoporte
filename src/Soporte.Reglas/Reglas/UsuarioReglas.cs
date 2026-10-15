using Soporte.Abstracciones.DTOs;
using Soporte.Abstracciones.Enumeraciones;
using Soporte.Abstracciones.Interfaces.DA;
using Soporte.Abstracciones.Interfaces.Reglas;
using Soporte.Abstracciones.Modelos;

namespace Soporte.Reglas.Reglas;

public class UsuarioReglas : IUsuarioReglas
{
    private readonly IUsuarioRepository _usuarios;

    public UsuarioReglas(IUsuarioRepository usuarios)
    {
        _usuarios = usuarios;
    }

    public bool PuedeAdministrarUsuarios(UsuarioContextoDto usuario) => usuario.Rol == RolTipo.Administrador;

    public bool PuedeBuscarClientes(UsuarioContextoDto usuario) => usuario.Rol != RolTipo.Cliente;

    public async Task<ResultadoOperacion> ValidarCambioRolAsync(Usuario usuario, int nuevoRolId)
    {
        if (!Enum.IsDefined(typeof(RolTipo), nuevoRolId))
            return ResultadoOperacion.Fallo("El rol debe ser Administrador, Agente o Cliente.");

        var dejaDeSerAdministrador = usuario.RolId == (int)RolTipo.Administrador && nuevoRolId != (int)RolTipo.Administrador;
        if (dejaDeSerAdministrador && usuario.Activo && await _usuarios.ContarPorRolAsync((int)RolTipo.Administrador) <= 1)
        {
            return ResultadoOperacion.Fallo(
                $"{usuario.Nombre} es el único usuario con rol Administrador. Asigná el rol a otra persona antes de continuar.");
        }

        return ResultadoOperacion.Ok();
    }
}
