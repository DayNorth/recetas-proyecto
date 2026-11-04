using System;
using System.Collections.Generic;

namespace Recetas.Api.Infraestructura.Persistencia.Entidades;

public partial class MeGusta
{
    public int UsuarioId { get; set; }

    public int RecetaId { get; set; }

    public DateTime Fecha { get; set; }

    public virtual Receta Receta { get; set; } = null!;

    public virtual Usuario Usuario { get; set; } = null!;
}
