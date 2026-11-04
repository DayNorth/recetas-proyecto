using Microsoft.EntityFrameworkCore;
using Recetas.Api.Aplicacion.Dtos;
using Recetas.Api.Aplicacion.Puertos;
using Recetas.Api.Dominio;
using Recetas.Api.Infraestructura.Persistencia;
using Recetas.Api.Infraestructura.Persistencia.Entidades;

namespace Recetas.Api.Infraestructura.Repositorios;

public class RepositorioDeMultimedia(RecetasDbContext laBaseDeDatos) : IRepositorioDeMultimedia
{
    public Task<int> CuenteLosArchivos(int laRecetaId, CancellationToken cancelacion) =>
        laBaseDeDatos.RecetaMultimedia.CountAsync(archivo => archivo.RecetaId == laRecetaId, cancelacion);

    public Task<bool> LaRecetaTienePortada(int laRecetaId, CancellationToken cancelacion) =>
        laBaseDeDatos.RecetaMultimedia.AnyAsync(archivo => archivo.RecetaId == laRecetaId && archivo.EsPortada, cancelacion);

    public async Task<MultimediaDto> AgregueElArchivo(int laRecetaId, string elTipo, string laRuta, string? laDescripcion, bool esPortada,
        CancellationToken cancelacion)
    {
        var elArchivo = new RecetaMultimedia
        {
            RecetaId = laRecetaId,
            Tipo = elTipo,
            Ruta = laRuta,
            Descripcion = laDescripcion,
            EsPortada = esPortada
        };
        laBaseDeDatos.RecetaMultimedia.Add(elArchivo);
        await laBaseDeDatos.SaveChangesAsync(cancelacion);
        return new MultimediaDto(elArchivo.MultimediaId, elArchivo.Tipo, elArchivo.Ruta, elArchivo.Descripcion, elArchivo.EsPortada);
    }

    public Task<MultimediaDto?> BusqueElArchivo(int laRecetaId, int elArchivoId, CancellationToken cancelacion) =>
        laBaseDeDatos.RecetaMultimedia.AsNoTracking().Where(archivo => archivo.MultimediaId == elArchivoId && archivo.RecetaId == laRecetaId)
            .Select(archivo => new MultimediaDto(archivo.MultimediaId, archivo.Tipo, archivo.Ruta, archivo.Descripcion, archivo.EsPortada))
            .FirstOrDefaultAsync(cancelacion);

    public async Task<string?> BorreElArchivoYReasigneLaPortada(int laRecetaId, int elArchivoId, CancellationToken cancelacion)
    {
        var elArchivo = await laBaseDeDatos.RecetaMultimedia
            .FirstOrDefaultAsync(archivo => archivo.MultimediaId == elArchivoId && archivo.RecetaId == laRecetaId, cancelacion);
        if (elArchivo is null) return null;

        laBaseDeDatos.RecetaMultimedia.Remove(elArchivo);
        await laBaseDeDatos.SaveChangesAsync(cancelacion);

        if (elArchivo.EsPortada)
        {
            var laSiguienteImagen = await laBaseDeDatos.RecetaMultimedia
                .Where(archivo => archivo.RecetaId == laRecetaId && archivo.Tipo == TiposDeMultimedia.IMAGEN)
                .OrderBy(archivo => archivo.MultimediaId).FirstOrDefaultAsync(cancelacion);
            if (laSiguienteImagen is not null)
            {
                laSiguienteImagen.EsPortada = true;
                await laBaseDeDatos.SaveChangesAsync(cancelacion);
            }
        }
        return elArchivo.Ruta;
    }

    public Task QuiteLasDemasPortadas(int laRecetaId, int laPortadaId, CancellationToken cancelacion) =>
        laBaseDeDatos.RecetaMultimedia.Where(archivo => archivo.RecetaId == laRecetaId && archivo.EsPortada && archivo.MultimediaId != laPortadaId)
            .ExecuteUpdateAsync(cambios => cambios.SetProperty(archivo => archivo.EsPortada, false), cancelacion);

    public Task EstablezcaLaPortada(int elArchivoId, CancellationToken cancelacion) =>
        laBaseDeDatos.RecetaMultimedia.Where(archivo => archivo.MultimediaId == elArchivoId)
            .ExecuteUpdateAsync(cambios => cambios.SetProperty(archivo => archivo.EsPortada, true), cancelacion);
}
