using Recetas.Api.Aplicacion.Comun;
using Recetas.Api.Aplicacion.Dtos;
using Recetas.Api.Aplicacion.Reportes;

namespace Recetas.Api.Aplicacion.Puertos;

public record ContenidoDenunciado(int? RecetaId, int? ComentarioId);

public interface IRepositorioDeModeracion
{
    Task<int?> BusqueElAutorDelComentarioVisible(int elComentarioId, CancellationToken cancelacion);

    Task<bool> ExisteUnaDenunciaPendiente(int elUsuarioId, int? laRecetaId, int? elComentarioId, CancellationToken cancelacion);

    Task<ReporteCreadoDto> AgregueLaDenuncia(int elUsuarioId, int? laRecetaId, int? elComentarioId, string elMotivo,
        string? laDescripcion, CancellationToken cancelacion);

    Task<Paginado<ReporteListadoDto>> ListeLasDenunciasPendientes(SolicitudDePagina laPagina, CancellationToken cancelacion);

    Task<Paginado<ReporteListadoDto>> ListeLasDenunciasAtendidas(string elEstado, SolicitudDePagina laPagina,
        CancellationToken cancelacion);

    Task<ReporteDetalleDto?> BusqueElDetalleDeLaDenuncia(int laDenunciaId, CancellationToken cancelacion);

    Task<string?> BloqueeLaDenunciaYObtengaSuEstado(int laDenunciaId, CancellationToken cancelacion);

    Task<ContenidoDenunciado> BusqueElContenidoDenunciado(int laDenunciaId, CancellationToken cancelacion);

    Task MarqueLaDenuncia(int laDenunciaId, string elEstado, string laResolucion, int elAdminId, DateTime laFecha,
        CancellationToken cancelacion);

    Task<int> CierreLasDemasDenunciasPendientes(int laDenunciaId, ContenidoDenunciado elContenido, string laResolucion,
        int elAdminId, DateTime laFecha, CancellationToken cancelacion);

    Task<bool> LaRecetaNoEstaModerada(int laRecetaId, CancellationToken cancelacion);

    Task<bool> OculteElComentarioVisible(int elComentarioId, CancellationToken cancelacion);

    Task<bool> CambieLaVisibilidadDelComentario(int elComentarioId, bool eraVisible, CancellationToken cancelacion);

    Task<bool> ExisteElComentario(int elComentarioId, CancellationToken cancelacion);

    Task AgregueLaAccionDeModeracion(int elAdminId, int? laRecetaId, int? elComentarioId, int? laDenunciaId, string laAccion,
        string elMotivo, CancellationToken cancelacion);

    Task<Paginado<HistorialDto>> ListeElHistorial(int? laRecetaId, int? elComentarioId, DateTime? desdeUtc, DateTime? hastaUtcExclusivo,
        SolicitudDePagina laPagina, CancellationToken cancelacion);
}

public interface IRepositorioDeReportes
{
    Task<ResultadoReporte> EjecuteElReporte(DefinicionDeReporte laDefinicion, RangoReporte elRango, ParametrosReporte losParametros,
        CancellationToken cancelacion);

    Task<Paginado<DescargaDto>> ListeLasDescargas(SolicitudDePagina laPagina, CancellationToken cancelacion);

    Task RegistreLaDescarga(int elUsuarioId, string elTipo, RangoReporte elRango, string elFormato, CancellationToken cancelacion);
}

public record ContenidoExportado(byte[] Bytes, string TipoDeContenido);

public interface IExportadorDeReportes
{
    ContenidoExportado Exporte(ResultadoReporte elResultado, string elFormato, DateTimeOffset laFechaDeGeneracion);
}
