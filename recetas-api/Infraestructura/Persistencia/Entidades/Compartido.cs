using System;
using System.Collections.Generic;

namespace Recetas.Api.Infraestructura.Persistencia.Entidades;

public partial class Compartido
{
    public int CompartidoId { get; set; }

    public int RecetaId { get; set; }

    public int? UsuarioId { get; set; }

    public short RedId { get; set; }

    public DateTime Fecha { get; set; }

    public virtual Receta Receta { get; set; } = null!;

    public virtual RedSocial Red { get; set; } = null!;

    public virtual Usuario? Usuario { get; set; }
}
