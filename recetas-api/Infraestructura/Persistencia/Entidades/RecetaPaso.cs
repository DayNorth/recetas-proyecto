using System;
using System.Collections.Generic;

namespace Recetas.Api.Infraestructura.Persistencia.Entidades;

public partial class RecetaPaso
{
    public int RecetaId { get; set; }

    public short NumeroPaso { get; set; }

    public string Descripcion { get; set; } = null!;

    public virtual Receta Receta { get; set; } = null!;
}
