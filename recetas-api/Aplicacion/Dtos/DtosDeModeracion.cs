namespace Recetas.Api.Aplicacion.Dtos;

public record ReporteCreadoDto(int ReporteContenidoId, string Estado);
public record ResolucionDto(int ReporteContenidoId, string Estado, int DenunciasCerradasAdicionales);
public record EstadoContenidoDto(int Id, string Estado);

public record ReporteListadoDto(int ReporteContenidoId, DateTime Fecha, string Motivo, string? Descripcion,
    string TipoContenido, string? Contenido, string ReportadoPor, string Estado);
public record PersonaDto(int UsuarioId, string Nombre);
public record RecetaModDto(int RecetaId, string Titulo, string Estado, string? Descripcion, PersonaDto Autor);
public record ComentarioModDto(int ComentarioId, int RecetaId, string RecetaTitulo, short? Calificacion, string? Texto, bool Visible, PersonaDto Autor);
public record OtraDenunciaDto(int ReporteContenidoId, string Estado, string Motivo, DateTime Fecha, string ReportadoPor);
public record HistorialDto(int ModeracionId, DateTime Fecha, string Admin, string Accion, string Motivo,
    int? RecetaId, string? RecetaTitulo, int? ComentarioId, int? ReporteContenidoId);
public record ReporteDetalleDto(int ReporteContenidoId, string Estado, string Motivo, string? Descripcion, DateTime Fecha,
    PersonaDto ReportadoPor, string? Resolucion, string? AtendidoPor, DateTime? FechaAtencion, string TipoContenido,
    RecetaModDto? Receta, ComentarioModDto? Comentario,
    IReadOnlyList<OtraDenunciaDto> OtrasDenuncias, IReadOnlyList<HistorialDto> Historial);
