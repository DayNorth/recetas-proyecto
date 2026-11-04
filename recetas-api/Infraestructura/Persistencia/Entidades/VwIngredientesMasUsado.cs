using System;
using System.Collections.Generic;

namespace Recetas.Api.Infraestructura.Persistencia.Entidades;

public partial class VwIngredientesMasUsado
{
    public string? Ingrediente { get; set; }

    public long? CantidadRecetas { get; set; }
}
