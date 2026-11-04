using System;
using System.Collections.Generic;

namespace Recetas.Api.Infraestructura.Persistencia.Entidades;

public partial class Ingrediente
{
    public int IngredienteId { get; set; }

    public string Nombre { get; set; } = null!;

    public virtual ICollection<ListaCompraItem> ListaCompraItems { get; set; } = new List<ListaCompraItem>();

    public virtual ICollection<RecetaIngrediente> RecetaIngredientes { get; set; } = new List<RecetaIngrediente>();
}
