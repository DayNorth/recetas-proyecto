using System;
using System.Collections.Generic;

namespace Recetas.Api.Infraestructura.Persistencia.Entidades;

public partial class RecetaMultimedia
{
    public int MultimediaId { get; set; }

    public int RecetaId { get; set; }

    public string Tipo { get; set; } = null!;

    public string Ruta { get; set; } = null!;

    public string? Descripcion { get; set; }

    public bool EsPortada { get; set; }

    public DateTime FechaSubida { get; set; }

    public virtual Receta Receta { get; set; } = null!;
}
