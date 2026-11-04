using System;
using System.Collections.Generic;

namespace Recetas.Api.Infraestructura.Persistencia.Entidades;

public partial class RecetaIngrediente
{
    public int RecetaId { get; set; }

    public int IngredienteId { get; set; }

    public short UnidadId { get; set; }

    public decimal Cantidad { get; set; }

    public string? Nota { get; set; }

    public virtual Ingrediente Ingrediente { get; set; } = null!;

    public virtual Receta Receta { get; set; } = null!;

    public virtual UnidadMedida Unidad { get; set; } = null!;
}
