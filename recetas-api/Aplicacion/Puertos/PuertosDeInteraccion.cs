using Recetas.Api.Aplicacion.Comun;
using Recetas.Api.Aplicacion.Dtos;

namespace Recetas.Api.Aplicacion.Puertos;

public record EstadisticaDeCalificaciones(decimal? Promedio, int Total);

public interface IRepositorioDeInteracciones
{
    Task AgregueElMeGusta(int elUsuarioId, int laRecetaId, CancellationToken cancelacion);

    Task QuiteElMeGusta(int elUsuarioId, int laRecetaId, CancellationToken cancelacion);

    Task<int> CuenteLosMeGusta(int laRecetaId, CancellationToken cancelacion);

    Task AgregueElFavorito(int elUsuarioId, int laRecetaId, CancellationToken cancelacion);

    Task QuiteElFavorito(int elUsuarioId, int laRecetaId, CancellationToken cancelacion);

    Task<int> CuenteLosFavoritos(int laRecetaId, CancellationToken cancelacion);

    Task<Paginado<FavoritoDto>> ListeLosFavoritos(int elUsuarioId, SolicitudDePagina laPagina, CancellationToken cancelacion);

    Task<Paginado<ComentarioDto>> ListeLosComentariosVisibles(int laRecetaId, SolicitudDePagina laPagina,
        CancellationToken cancelacion);

    Task<EstadisticaDeCalificaciones> CalculeLasCalificaciones(int laRecetaId, CancellationToken cancelacion);

    Task<ComentarioDto> GuardeLaCalificacion(int elUsuarioId, int laRecetaId, short? laCalificacion, string? elTexto,
        CancellationToken cancelacion);

    Task BorreLaCalificacion(int elUsuarioId, int laRecetaId, CancellationToken cancelacion);

    Task AgregueElCompartido(int laRecetaId, int? elUsuarioId, short laRedId, CancellationToken cancelacion);
}
