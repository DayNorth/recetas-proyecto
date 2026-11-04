using System;
using System.Collections.Generic;

namespace Recetas.Api.Infraestructura.Persistencia.Entidades;

public partial class NivelDificultad
{
    public short DificultadId { get; set; }

    public string Nombre { get; set; } = null!;

    public virtual ICollection<Receta> Recetas { get; set; } = new List<Receta>();
}
