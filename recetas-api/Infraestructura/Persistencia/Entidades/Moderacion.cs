using System;
using System.Collections.Generic;

namespace Recetas.Api.Infraestructura.Persistencia.Entidades;

public partial class Moderacion
{
    public int ModeracionId { get; set; }

    public int AdminId { get; set; }

    public int? RecetaId { get; set; }

    public int? ComentarioId { get; set; }

    public int? ReporteContenidoId { get; set; }

    public string Accion { get; set; } = null!;

    public string Motivo { get; set; } = null!;

    public DateTime Fecha { get; set; }

    public virtual Usuario Admin { get; set; } = null!;

    public virtual Comentario? Comentario { get; set; }

    public virtual Receta? Receta { get; set; }

    public virtual ReporteContenido? ReporteContenido { get; set; }
}
