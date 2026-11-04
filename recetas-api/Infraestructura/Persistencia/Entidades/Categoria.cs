using System;
using System.Collections.Generic;

namespace Recetas.Api.Infraestructura.Persistencia.Entidades;

public partial class Categoria
{
    public short CategoriaId { get; set; }

    public string Nombre { get; set; } = null!;

    public string? Descripcion { get; set; }

    public bool Activo { get; set; }

    public virtual ICollection<Busqueda> Busquedas { get; set; } = new List<Busqueda>();

    public virtual ICollection<Receta> Recetas { get; set; } = new List<Receta>();
}
