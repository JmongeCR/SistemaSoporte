namespace Soporte.Abstracciones.Modelos;

/// <summary>Registro de que un usuario leyó un comentario. Sin fila = no leído (HU-09).</summary>
public class LecturaComentario
{
    public int ComentarioId { get; set; }
    public int UsuarioId { get; set; }
    public DateTime FechaLectura { get; set; }

    public Comentario? Comentario { get; set; }
    public Usuario? Usuario { get; set; }
}
