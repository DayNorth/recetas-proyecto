using System;
using System.Collections.Generic;

namespace Recetas.Api.Infraestructura.Persistencia.Entidades;

public partial class Noticia
{
    public int NoticiaId { get; set; }

    public int UsuarioId { get; set; }

    public string Titulo { get; set; } = null!;

    public string Contenido { get; set; } = null!;

    public string? Imagen { get; set; }

    public bool Publicada { get; set; }

    public DateTime FechaPublicacion { get; set; }

    public virtual Usuario Usuario { get; set; } = null!;
}
