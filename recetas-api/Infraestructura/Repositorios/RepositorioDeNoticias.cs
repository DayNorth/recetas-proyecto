using Microsoft.EntityFrameworkCore;
using Recetas.Api.Aplicacion.Comun;
using Recetas.Api.Aplicacion.Dtos;
using Recetas.Api.Aplicacion.Puertos;
using Recetas.Api.Infraestructura.Persistencia;
using Recetas.Api.Infraestructura.Persistencia.Entidades;

namespace Recetas.Api.Infraestructura.Repositorios;

public class RepositorioDeNoticias(RecetasDbContext laBaseDeDatos) : IRepositorioDeNoticias
{
    private static NoticiaDto ConviertaAlDto(Noticia laNoticia) =>
        new(laNoticia.NoticiaId, laNoticia.Titulo, laNoticia.Contenido, laNoticia.Imagen, laNoticia.Publicada,
            laNoticia.FechaPublicacion, laNoticia.Usuario.Nombre + " " + laNoticia.Usuario.Apellidos);

    public Task<Paginado<NoticiaDto>> ListeLasNoticias(bool incluirNoPublicadas, SolicitudDePagina laPagina, CancellationToken cancelacion)
    {
        var laConsulta = laBaseDeDatos.Noticias.AsNoTracking().AsQueryable();
        if (!incluirNoPublicadas) laConsulta = laConsulta.Where(noticia => noticia.Publicada);
        return laConsulta.OrderByDescending(noticia => noticia.FechaPublicacion).ThenByDescending(noticia => noticia.NoticiaId)
            .Select(noticia => new NoticiaDto(noticia.NoticiaId, noticia.Titulo, noticia.Contenido, noticia.Imagen, noticia.Publicada,
                noticia.FechaPublicacion, noticia.Usuario.Nombre + " " + noticia.Usuario.Apellidos))
            .PagineLaConsulta(laPagina, cancelacion);
    }

    public async Task<NoticiaDto?> BusqueLaNoticia(int laNoticiaId, CancellationToken cancelacion)
    {
        var laNoticia = await laBaseDeDatos.Noticias.AsNoTracking().Include(noticia => noticia.Usuario)
            .FirstOrDefaultAsync(noticia => noticia.NoticiaId == laNoticiaId, cancelacion);
        return laNoticia is null ? null : ConviertaAlDto(laNoticia);
    }

    public Task<bool> ExisteLaNoticia(int laNoticiaId, CancellationToken cancelacion) =>
        laBaseDeDatos.Noticias.AnyAsync(noticia => noticia.NoticiaId == laNoticiaId, cancelacion);

    public async Task<NoticiaDto> AgregueLaNoticia(int elAutorId, string elTitulo, string elContenido, string? laImagen, bool estaPublicada,
        CancellationToken cancelacion)
    {
        var laNoticia = new Noticia
        {
            UsuarioId = elAutorId,
            Titulo = elTitulo,
            Contenido = elContenido,
            Imagen = laImagen,
            Publicada = estaPublicada
        };
        laBaseDeDatos.Noticias.Add(laNoticia);
        await laBaseDeDatos.SaveChangesAsync(cancelacion);
        return (await BusqueLaNoticia(laNoticia.NoticiaId, cancelacion))!;
    }

    public async Task<(NoticiaDto Noticia, string? ImagenAnterior)> ActualiceLaNoticia(int laNoticiaId, string elTitulo, string elContenido,
        string? laImagenNueva, bool quitarLaImagen, bool? estaPublicada, CancellationToken cancelacion)
    {
        var laNoticia = await laBaseDeDatos.Noticias.Include(noticia => noticia.Usuario)
            .FirstAsync(noticia => noticia.NoticiaId == laNoticiaId, cancelacion);

        var laImagenAnterior = laNoticia.Imagen;
        if (laImagenNueva is not null) laNoticia.Imagen = laImagenNueva;
        else if (quitarLaImagen) laNoticia.Imagen = null;

        laNoticia.Titulo = elTitulo;
        laNoticia.Contenido = elContenido;
        if (estaPublicada is not null) laNoticia.Publicada = estaPublicada.Value;
        await laBaseDeDatos.SaveChangesAsync(cancelacion);
        return (ConviertaAlDto(laNoticia), laImagenAnterior);
    }

    public async Task<bool> DespubliqueLaNoticia(int laNoticiaId, CancellationToken cancelacion) =>
        await laBaseDeDatos.Noticias.Where(noticia => noticia.NoticiaId == laNoticiaId)
            .ExecuteUpdateAsync(cambios => cambios.SetProperty(noticia => noticia.Publicada, false), cancelacion) > 0;
}
