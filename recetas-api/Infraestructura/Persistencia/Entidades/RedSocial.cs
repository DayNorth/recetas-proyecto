using System;
using System.Collections.Generic;

namespace Recetas.Api.Infraestructura.Persistencia.Entidades;

public partial class RedSocial
{
    public short RedId { get; set; }

    public string Nombre { get; set; } = null!;

    public virtual ICollection<Compartido> Compartidos { get; set; } = new List<Compartido>();
}
