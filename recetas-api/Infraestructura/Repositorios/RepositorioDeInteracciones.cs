using Microsoft.EntityFrameworkCore;
using Recetas.Api.Aplicacion.Comun;
using Recetas.Api.Aplicacion.Dtos;
using Recetas.Api.Aplicacion.Puertos;
using Recetas.Api.Dominio;
using Recetas.Api.Infraestructura.Persistencia;
using Recetas.Api.Infraestructura.Persistencia.Entidades;

namespace Recetas.Api.Infraestructura.Repositorios;

public class RepositorioDeInteracciones(RecetasDbContext laBaseDeDatos) : IRepositorioDeInteracciones
{

    public async Task AgregueElMeGusta(int elUsuarioId, int laRecetaId, CancellationToken cancelacion) =>
        await laBaseDeDatos.Database.ExecuteSqlInterpolatedAsync(
            $"INSERT INTO me_gusta (usuario_id, receta_id) VALUES ({elUsuarioId}, {laRecetaId}) ON CONFLICT DO NOTHING", cancelacion);

    public Task QuiteElMeGusta(int elUsuarioId, int laRecetaId, CancellationToken cancelacion) =>
        laBaseDeDatos.MeGusta.Where(meGusta => meGusta.RecetaId == laRecetaId && meGusta.UsuarioId == elUsuarioId)
            .ExecuteDeleteAsync(cancelacion);

    public Task<int> CuenteLosMeGusta(int laRecetaId, CancellationToken cancelacion) =>
        laBaseDeDatos.MeGusta.CountAsync(meGusta => meGusta.RecetaId == laRecetaId, cancelacion);

    public async Task AgregueElFavorito(int elUsuarioId, int laRecetaId, CancellationToken cancelacion) =>
        await laBaseDeDatos.Database.ExecuteSqlInterpolatedAsync(
            $"INSERT INTO favoritos (usuario_id, receta_id) VALUES ({elUsuarioId}, {laRecetaId}) ON CONFLICT DO NOTHING", cancelacion);

    public Task QuiteElFavorito(int elUsuarioId, int laRecetaId, CancellationToken cancelacion) =>
        laBaseDeDatos.Favoritos.Where(favorito => favorito.RecetaId == laRecetaId && favorito.UsuarioId == elUsuarioId)
            .ExecuteDeleteAsync(cancelacion);

    public Task<int> CuenteLosFavoritos(int laRecetaId, CancellationToken cancelacion) =>
        laBaseDeDatos.Favoritos.CountAsync(favorito => favorito.RecetaId == laRecetaId, cancelacion);

    public Task<Paginado<FavoritoDto>> ListeLosFavoritos(int elUsuarioId, SolicitudDePagina laPagina, CancellationToken cancelacion) =>
        laBaseDeDatos.Favoritos.AsNoTracking().Where(favorito => favorito.UsuarioId == elUsuarioId)
            .OrderByDescending(favorito => favorito.Fecha).ThenByDescending(favorito => favorito.RecetaId)
            .Select(favorito => new FavoritoDto(favorito.RecetaId, favorito.Receta.Titulo,
                favorito.Receta.Estado == EstadosDeLaReceta.PUBLICADA
                    ? favorito.Receta.Multimedia.Where(archivo => archivo.EsPortada).Select(archivo => archivo.Ruta).FirstOrDefault()
                    : null,
                favorito.Receta.Estado == EstadosDeLaReceta.PUBLICADA ? favorito.Receta.Categoria.Nombre : null,
                favorito.Receta.Estado == EstadosDeLaReceta.PUBLICADA ? favorito.Receta.TiempoPrepMin : null,
                favorito.Receta.Estado == EstadosDeLaReceta.PUBLICADA, favorito.Fecha))
            .PagineLaConsulta(laPagina, cancelacion);

    public Task<Paginado<ComentarioDto>> ListeLosComentariosVisibles(int laRecetaId, SolicitudDePagina laPagina,
        CancellationToken cancelacion) =>
        laBaseDeDatos.Comentarios.AsNoTracking().Where(comentario => comentario.RecetaId == laRecetaId && comentario.Visible)
            .OrderByDescending(comentario => comentario.Fecha).ThenByDescending(comentario => comentario.ComentarioId)
            .Select(comentario => new ComentarioDto(comentario.ComentarioId, comentario.UsuarioId,
                comentario.Usuario.Nombre + " " + comentario.Usuario.Apellidos, comentario.Calificacion, comentario.Comentario1, comentario.Fecha))
            .PagineLaConsulta(laPagina, cancelacion);

    public async Task<EstadisticaDeCalificaciones> CalculeLasCalificaciones(int laRecetaId, CancellationToken cancelacion)
    {
        var lasEstadisticas = await laBaseDeDatos.Comentarios.Where(comentario => comentario.RecetaId == laRecetaId && comentario.Calificacion != null)
            .GroupBy(_ => 1)
            .Select(grupo => new { Promedio = grupo.Average(comentario => (decimal)comentario.Calificacion!), Total = grupo.Count() })
            .FirstOrDefaultAsync(cancelacion);
        return new EstadisticaDeCalificaciones(lasEstadisticas is null ? null : Math.Round(lasEstadisticas.Promedio, 2), lasEstadisticas?.Total ?? 0);
    }

    public async Task<ComentarioDto> GuardeLaCalificacion(int elUsuarioId, int laRecetaId, short? laCalificacion, string? elTexto,
        CancellationToken cancelacion)
    {
        var losIds = await laBaseDeDatos.Database.SqlQuery<int>($@"
            INSERT INTO comentarios (usuario_id, receta_id, calificacion, comentario) VALUES ({elUsuarioId}, {laRecetaId}, {laCalificacion}::smallint, {elTexto}::varchar)
            ON CONFLICT (usuario_id, receta_id)
            DO UPDATE SET calificacion = EXCLUDED.calificacion, comentario = EXCLUDED.comentario, fecha = CURRENT_TIMESTAMP
            RETURNING comentario_id AS ""Value""").ToListAsync(cancelacion);
        var elComentarioId = losIds.Single();

        return await laBaseDeDatos.Comentarios.AsNoTracking().Where(comentario => comentario.ComentarioId == elComentarioId)
            .Select(comentario => new ComentarioDto(comentario.ComentarioId, comentario.UsuarioId,
                comentario.Usuario.Nombre + " " + comentario.Usuario.Apellidos, comentario.Calificacion, comentario.Comentario1, comentario.Fecha))
            .SingleAsync(cancelacion);
    }

    public Task BorreLaCalificacion(int elUsuarioId, int laRecetaId, CancellationToken cancelacion) =>
        laBaseDeDatos.Comentarios.Where(comentario => comentario.RecetaId == laRecetaId && comentario.UsuarioId == elUsuarioId)
            .ExecuteDeleteAsync(cancelacion);

    public async Task AgregueElCompartido(int laRecetaId, int? elUsuarioId, short laRedId, CancellationToken cancelacion)
    {
        laBaseDeDatos.Compartidos.Add(new Compartido { RecetaId = laRecetaId, UsuarioId = elUsuarioId, RedId = laRedId });
        await laBaseDeDatos.SaveChangesAsync(cancelacion);
    }
}
