using System;
using System.Collections.Generic;

namespace Recetas.Api.Infraestructura.Persistencia.Entidades;

public partial class Rol
{
    public short RolId { get; set; }

    public string Nombre { get; set; } = null!;

    public string? Descripcion { get; set; }

    public virtual ICollection<Usuario> Usuarios { get; set; } = new List<Usuario>();
}
