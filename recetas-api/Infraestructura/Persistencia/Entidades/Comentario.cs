using System;
using System.Collections.Generic;

namespace Recetas.Api.Infraestructura.Persistencia.Entidades;

public partial class Comentario
{
    public int ComentarioId { get; set; }

    public int UsuarioId { get; set; }

    public int RecetaId { get; set; }

    public short? Calificacion { get; set; }

    public string? Comentario1 { get; set; }

    public bool Visible { get; set; }

    public DateTime Fecha { get; set; }

    public virtual ICollection<Moderacion> Moderaciones { get; set; } = new List<Moderacion>();

    public virtual Receta Receta { get; set; } = null!;

    public virtual ICollection<ReporteContenido> ReportesContenido { get; set; } = new List<ReporteContenido>();

    public virtual Usuario Usuario { get; set; } = null!;
}
