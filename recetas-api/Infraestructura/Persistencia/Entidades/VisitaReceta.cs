using System;
using System.Collections.Generic;

namespace Recetas.Api.Infraestructura.Persistencia.Entidades;

public partial class VisitaReceta
{
    public long VisitaId { get; set; }

    public int RecetaId { get; set; }

    public int? UsuarioId { get; set; }

    public long? BusquedaId { get; set; }

    public DateTime Fecha { get; set; }

    public virtual Busqueda? Busqueda { get; set; }

    public virtual Receta Receta { get; set; } = null!;

    public virtual Usuario? Usuario { get; set; }
}
