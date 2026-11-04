using System;
using System.Collections.Generic;

namespace Recetas.Api.Infraestructura.Persistencia.Entidades;

public partial class UnidadMedida
{
    public short UnidadId { get; set; }

    public string Nombre { get; set; } = null!;

    public string Abreviatura { get; set; } = null!;

    public bool Escalable { get; set; }

    public virtual ICollection<ListaCompraItem> ListaCompraItems { get; set; } = new List<ListaCompraItem>();

    public virtual ICollection<RecetaIngrediente> RecetaIngredientes { get; set; } = new List<RecetaIngrediente>();
}
