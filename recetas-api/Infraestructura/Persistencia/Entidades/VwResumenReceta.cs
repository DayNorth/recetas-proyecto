using System;
using System.Collections.Generic;

namespace Recetas.Api.Infraestructura.Persistencia.Entidades;

public partial class VwResumenReceta
{
    public int? RecetaId { get; set; }

    public string? Titulo { get; set; }

    public string? Categoria { get; set; }

    public string? Autor { get; set; }

    public long? TotalVisitas { get; set; }

    public long? TotalMeGusta { get; set; }

    public long? TotalFavoritos { get; set; }

    public long? TotalCompartidos { get; set; }

    public decimal? CalificacionPromedio { get; set; }
}
