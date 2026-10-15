using Soporte.Abstracciones.DTOs;
using Soporte.Abstracciones.Modelos;

namespace Soporte.Abstracciones.Interfaces.Reglas;

public interface IUsuarioReglas
{
    /// <summary>HU-14/HU-15: solo el Administrador consulta usuarios y cambia roles.</summary>
    bool PuedeAdministrarUsuarios(UsuarioContextoDto usuario);

    /// <summary>Administrador y Agente eligen el cliente solicitante al crear un ticket.</summary>
    bool PuedeBuscarClientes(UsuarioContextoDto usuario);

    /// <summary>
    /// HU-15 esc. 2: no se puede quitar el rol Administrador al único que lo tiene.
    /// Es asíncrona porque consulta cuántos administradores hay.
    /// </summary>
    Task<ResultadoOperacion> ValidarCambioRolAsync(Usuario usuario, int nuevoRolId);
}
