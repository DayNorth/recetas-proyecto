using System;
using System.Collections.Generic;

namespace Recetas.Api.Infraestructura.Persistencia.Entidades;

public partial class VwTerminosMasBuscado
{
    public string? Termino { get; set; }

    public long? VecesBuscado { get; set; }

    public decimal? PromedioResultados { get; set; }
}
