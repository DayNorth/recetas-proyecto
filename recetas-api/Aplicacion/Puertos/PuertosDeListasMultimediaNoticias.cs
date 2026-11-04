using Recetas.Api.Aplicacion.Comun;
using Recetas.Api.Aplicacion.Dtos;

namespace Recetas.Api.Aplicacion.Puertos;

public interface IRepositorioDeListas
{
    Task<int> AgregueLaLista(int elUsuarioId, string elNombre, CancellationToken cancelacion);

    Task<int> AgregueLosItemsDesdeLaReceta(int laListaId, int laRecetaId, int lasPorciones, CancellationToken cancelacion);

    Task<Paginado<ListaResumenDto>> ListeLasListas(int elUsuarioId, string? elEstado, SolicitudDePagina laPagina,
        CancellationToken cancelacion);

    Task<ListaDetalleDto?> BusqueElDetalleDeLaLista(int elUsuarioId, int laListaId, CancellationToken cancelacion);

    Task<bool> ExisteLaListaDelUsuario(int elUsuarioId, int laListaId, CancellationToken cancelacion);

    Task BloqueeLaLista(int laListaId, CancellationToken cancelacion);

    Task<bool> MarqueElItem(int laListaId, int elItemId, bool estaComprado, CancellationToken cancelacion);

    Task<bool> HayItemsPendientes(int laListaId, CancellationToken cancelacion);

    Task EstablezcaElEstadoDeLaLista(int laListaId, string elEstado, CancellationToken cancelacion);

    Task<ListaItemDto> BusqueElItem(int elItemId, CancellationToken cancelacion);

    Task<bool> BorreLaLista(int elUsuarioId, int laListaId, CancellationToken cancelacion);
}

public interface IRepositorioDeMultimedia
{
    Task<int> CuenteLosArchivos(int laRecetaId, CancellationToken cancelacion);

    Task<bool> LaRecetaTienePortada(int laRecetaId, CancellationToken cancelacion);

    Task<MultimediaDto> AgregueElArchivo(int laRecetaId, string elTipo, string laRuta, string? laDescripcion, bool esPortada,
        CancellationToken cancelacion);

    Task<MultimediaDto?> BusqueElArchivo(int laRecetaId, int elArchivoId, CancellationToken cancelacion);

    Task<string?> BorreElArchivoYReasigneLaPortada(int laRecetaId, int elArchivoId, CancellationToken cancelacion);

    Task QuiteLasDemasPortadas(int laRecetaId, int laPortadaId, CancellationToken cancelacion);

    Task EstablezcaLaPortada(int elArchivoId, CancellationToken cancelacion);
}

public interface IRepositorioDeNoticias
{
    Task<Paginado<NoticiaDto>> ListeLasNoticias(bool incluirNoPublicadas, SolicitudDePagina laPagina,
        CancellationToken cancelacion);

    Task<NoticiaDto?> BusqueLaNoticia(int laNoticiaId, CancellationToken cancelacion);

    Task<bool> ExisteLaNoticia(int laNoticiaId, CancellationToken cancelacion);

    Task<NoticiaDto> AgregueLaNoticia(int elAutorId, string elTitulo, string elContenido, string? laImagen, bool estaPublicada,
        CancellationToken cancelacion);

    Task<(NoticiaDto Noticia, string? ImagenAnterior)> ActualiceLaNoticia(int laNoticiaId, string elTitulo, string elContenido,
        string? laImagenNueva, bool quitarLaImagen, bool? estaPublicada, CancellationToken cancelacion);

    Task<bool> DespubliqueLaNoticia(int laNoticiaId, CancellationToken cancelacion);
}
