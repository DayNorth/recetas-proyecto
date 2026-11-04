using Microsoft.EntityFrameworkCore;
using Recetas.Api.Aplicacion.Puertos;
using Recetas.Api.Dominio;
using Recetas.Api.Infraestructura.Persistencia;
using Recetas.Api.Infraestructura.Persistencia.Entidades;

namespace Recetas.Api.Infraestructura.Repositorios;

public class RepositorioDeRecetas(RecetasDbContext laBaseDeDatos) : IRepositorioDeRecetas
{
    public Task<RecetaBasica?> BusqueLaRecetaBasica(int laRecetaId, CancellationToken cancelacion) =>
        laBaseDeDatos.Recetas.AsNoTracking().Where(receta => receta.RecetaId == laRecetaId)
            .Select(receta => new RecetaBasica(receta.RecetaId, receta.Titulo, receta.UsuarioId, receta.Estado,
                receta.CategoriaId, receta.PorcionesBase))
            .FirstOrDefaultAsync(cancelacion);

    public async Task<int> AgregueLaReceta(NuevaReceta laReceta, CancellationToken cancelacion)
    {
        var laEntidad = new Receta
        {
            UsuarioId = laReceta.AutorId,
            CategoriaId = laReceta.Datos.CategoriaId,
            DificultadId = laReceta.Datos.DificultadId,
            Titulo = laReceta.Datos.Titulo,
            Descripcion = laReceta.Datos.Descripcion,
            TiempoPrepMin = laReceta.Datos.TiempoPrepMin,
            PorcionesBase = laReceta.Datos.PorcionesBase,
            Origen = laReceta.Origen,
            Estado = EstadosDeLaReceta.PUBLICADA
        };
        laBaseDeDatos.Recetas.Add(laEntidad);
        await laBaseDeDatos.SaveChangesAsync(cancelacion);
        return laEntidad.RecetaId;
    }

    public async Task ActualiceLosDatosDeLaReceta(int laRecetaId, DatosDeLaReceta losDatos, CancellationToken cancelacion)
    {
        var laReceta = await laBaseDeDatos.Recetas.FirstAsync(receta => receta.RecetaId == laRecetaId, cancelacion);
        laReceta.CategoriaId = losDatos.CategoriaId;
        laReceta.DificultadId = losDatos.DificultadId;
        laReceta.Titulo = losDatos.Titulo;
        laReceta.Descripcion = losDatos.Descripcion;
        laReceta.TiempoPrepMin = losDatos.TiempoPrepMin;
        laReceta.PorcionesBase = losDatos.PorcionesBase;
        await laBaseDeDatos.SaveChangesAsync(cancelacion);
    }

    public async Task BorreElDetalleDeLaReceta(int laRecetaId, CancellationToken cancelacion)
    {
        await laBaseDeDatos.RecetaIngredientes.Where(detalle => detalle.RecetaId == laRecetaId).ExecuteDeleteAsync(cancelacion);
        await laBaseDeDatos.RecetaPasos.Where(paso => paso.RecetaId == laRecetaId).ExecuteDeleteAsync(cancelacion);
    }

    public async Task AgregueElDetalleDeLaReceta(int laRecetaId, DetalleDeLaReceta elDetalle, CancellationToken cancelacion)
    {
        foreach (var elIngrediente in elDetalle.Ingredientes)
            laBaseDeDatos.RecetaIngredientes.Add(new RecetaIngrediente
            {
                RecetaId = laRecetaId,
                IngredienteId = elIngrediente.IngredienteId,
                UnidadId = elIngrediente.UnidadId,
                Cantidad = elIngrediente.Cantidad,
                Nota = elIngrediente.Nota
            });

        short elNumeroDePaso = 1;
        foreach (var laDescripcion in elDetalle.Pasos)
            laBaseDeDatos.RecetaPasos.Add(new RecetaPaso { RecetaId = laRecetaId, NumeroPaso = elNumeroDePaso++, Descripcion = laDescripcion });

        foreach (var elArchivo in elDetalle.Multimedia)
            laBaseDeDatos.RecetaMultimedia.Add(new RecetaMultimedia
            {
                RecetaId = laRecetaId,
                Tipo = elArchivo.Tipo,
                Ruta = elArchivo.Ruta,
                EsPortada = elArchivo.EsPortada
            });

        await laBaseDeDatos.SaveChangesAsync(cancelacion);
    }

    public Task EstablezcaElEstadoDeLaReceta(int laRecetaId, string elEstado, CancellationToken cancelacion) =>
        laBaseDeDatos.Recetas.Where(receta => receta.RecetaId == laRecetaId)
            .ExecuteUpdateAsync(cambios => cambios.SetProperty(receta => receta.Estado, elEstado), cancelacion);

    public async Task<bool> CambieElEstadoSiEs(int laRecetaId, string elEstadoActual, string elEstadoNuevo, CancellationToken cancelacion) =>
        await laBaseDeDatos.Recetas.Where(receta => receta.RecetaId == laRecetaId && receta.Estado == elEstadoActual)
            .ExecuteUpdateAsync(cambios => cambios.SetProperty(receta => receta.Estado, elEstadoNuevo), cancelacion) > 0;
}
