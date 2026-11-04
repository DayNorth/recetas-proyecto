using System;
using System.Collections.Generic;

namespace Recetas.Api.Infraestructura.Persistencia.Entidades;

public partial class VwReportesContenidoPendiente
{
    public int? ReporteContenidoId { get; set; }

    public DateTime? Fecha { get; set; }

    public string? Motivo { get; set; }

    public string? Descripcion { get; set; }

    public string? TipoContenido { get; set; }

    public string? Contenido { get; set; }

    public string? ReportadoPor { get; set; }
}
