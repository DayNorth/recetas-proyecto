using System;
using System.Collections.Generic;

namespace Recetas.Api.Infraestructura.Persistencia.Entidades;

public partial class ListaCompra
{
    public int ListaId { get; set; }

    public int UsuarioId { get; set; }

    public string Nombre { get; set; } = null!;

    public string Estado { get; set; } = null!;

    public DateTime FechaCreacion { get; set; }

    public virtual ICollection<ListaCompraItem> ListaCompraItems { get; set; } = new List<ListaCompraItem>();

    public virtual Usuario Usuario { get; set; } = null!;
}
