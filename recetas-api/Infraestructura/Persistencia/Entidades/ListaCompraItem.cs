using System;
using System.Collections.Generic;

namespace Recetas.Api.Infraestructura.Persistencia.Entidades;

public partial class ListaCompraItem
{
    public int ItemId { get; set; }

    public int ListaId { get; set; }

    public int IngredienteId { get; set; }

    public short UnidadId { get; set; }

    public decimal Cantidad { get; set; }

    public int? RecetaOrigenId { get; set; }

    public bool Comprado { get; set; }

    public virtual Ingrediente Ingrediente { get; set; } = null!;

    public virtual ListaCompra Lista { get; set; } = null!;

    public virtual Receta? RecetaOrigen { get; set; }

    public virtual UnidadMedida Unidad { get; set; } = null!;
}
