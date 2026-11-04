using System;
using System.Collections.Generic;

namespace Recetas.Api.Infraestructura.Persistencia.Entidades;

public partial class Busqueda
{
    public long BusquedaId { get; set; }

    public int? UsuarioId { get; set; }

    public string Termino { get; set; } = null!;

    public short? CategoriaId { get; set; }

    public int CantidadResultados { get; set; }

    public DateTime Fecha { get; set; }

    public virtual Categoria? Categoria { get; set; }

    public virtual Usuario? Usuario { get; set; }

    public virtual ICollection<VisitaReceta> VisitasReceta { get; set; } = new List<VisitaReceta>();
}
