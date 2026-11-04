using System;
using System.Collections.Generic;

namespace Recetas.Api.Infraestructura.Persistencia.Entidades;

public partial class ReporteContenido
{
    public int ReporteContenidoId { get; set; }

    public int UsuarioId { get; set; }

    public int? RecetaId { get; set; }

    public int? ComentarioId { get; set; }

    public string Motivo { get; set; } = null!;

    public string? Descripcion { get; set; }

    public string Estado { get; set; } = null!;

    public string? Resolucion { get; set; }

    public int? AtendidoPor { get; set; }

    public DateTime Fecha { get; set; }

    public DateTime? FechaAtencion { get; set; }

    public virtual Usuario? AtendidoPorNavigation { get; set; }

    public virtual Comentario? Comentario { get; set; }

    public virtual ICollection<Moderacion> Moderaciones { get; set; } = new List<Moderacion>();

    public virtual Receta? Receta { get; set; }

    public virtual Usuario Usuario { get; set; } = null!;
}
