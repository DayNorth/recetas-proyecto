using System;
using System.Collections.Generic;

namespace Recetas.Api.Infraestructura.Persistencia.Entidades;

public partial class ReporteGenerado
{
    public int ReporteId { get; set; }

    public int UsuarioId { get; set; }

    public string TipoReporte { get; set; } = null!;

    public DateOnly FechaInicio { get; set; }

    public DateOnly FechaFin { get; set; }

    public string Formato { get; set; } = null!;

    public DateTime FechaGeneracion { get; set; }

    public virtual Usuario Usuario { get; set; } = null!;
}
