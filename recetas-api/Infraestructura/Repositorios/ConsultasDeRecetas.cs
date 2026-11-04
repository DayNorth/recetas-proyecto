using Microsoft.EntityFrameworkCore;
using Recetas.Api.Aplicacion.Comun;
using Recetas.Api.Aplicacion.Dtos;
using Recetas.Api.Aplicacion.Puertos;
using Recetas.Api.Aplicacion.Solicitudes;
using Recetas.Api.Dominio;
using Recetas.Api.Infraestructura.Persistencia;
using Recetas.Api.Infraestructura.Persistencia.Entidades;

namespace Recetas.Api.Infraestructura.Repositorios;

public class ConsultasDeRecetas(RecetasDbContext laBaseDeDatos) : IConsultasDeRecetas
{
    private static IQueryable<RecetaResumenDto> ProyecteElResumen(IQueryable<Receta> lasRecetas) =>
        lasRecetas.Select(receta => new RecetaResumenDto(
            receta.RecetaId, receta.Titulo, receta.Descripcion,
            receta.Multimedia.Where(archivo => archivo.EsPortada).Select(archivo => archivo.Ruta).FirstOrDefault(),
            receta.Categoria.Nombre, receta.Dificultad.Nombre, receta.TiempoPrepMin,
            receta.MeGusta.Count,
            receta.Comentarios.Where(comentario => comentario.Calificacion != null).Average(comentario => (decimal?)comentario.Calificacion),
            receta.Comentarios.Count(comentario => comentario.Calificacion != null),
            receta.Estado));

    public async Task<Paginado<RecetaResumenDto>> BusqueLasRecetasPublicadas(FiltroDeRecetas elFiltro, SolicitudDePagina laPagina,
        CancellationToken cancelacion)
    {
        var laConsulta = laBaseDeDatos.Recetas.AsNoTracking().Where(receta => receta.Estado == EstadosDeLaReceta.PUBLICADA);
        if (!string.IsNullOrEmpty(elFiltro.Texto))
        {
            var elPatron = ExtensionesDeConsulta.ConstruyaElPatronDeContiene(elFiltro.Texto);
            laConsulta = laConsulta.Where(receta => EF.Functions.ILike(receta.Titulo, elPatron, "\\")
                                                    || (receta.Descripcion != null && EF.Functions.ILike(receta.Descripcion, elPatron, "\\")));
        }
        if (elFiltro.CategoriaId is not null) laConsulta = laConsulta.Where(receta => receta.CategoriaId == elFiltro.CategoriaId);
        if (elFiltro.DificultadId is not null) laConsulta = laConsulta.Where(receta => receta.DificultadId == elFiltro.DificultadId);
        if (elFiltro.TiempoMaximo is not null) laConsulta = laConsulta.Where(receta => receta.TiempoPrepMin <= elFiltro.TiempoMaximo);
        foreach (var elIngredienteId in elFiltro.Ingredientes)
        {
            var elIdBuscado = elIngredienteId;
            laConsulta = laConsulta.Where(receta => receta.RecetaIngredientes.Any(detalle => detalle.IngredienteId == elIdBuscado));
        }

        laConsulta = elFiltro.Orden switch
        {
            "populares" => laConsulta.OrderByDescending(receta => receta.MeGusta.Count).ThenByDescending(receta => receta.FechaCreacion),
            "tiempo" => laConsulta.OrderBy(receta => receta.TiempoPrepMin).ThenBy(receta => receta.Titulo),
            _ => laConsulta.OrderByDescending(receta => receta.FechaCreacion).ThenByDescending(receta => receta.RecetaId)
        };

        var elTotal = await laConsulta.CountAsync(cancelacion);
        var losItems = await ProyecteElResumen(laConsulta).Skip(laPagina.CuantosSaltar).Take(laPagina.Tamano).ToListAsync(cancelacion);
        return new Paginado<RecetaResumenDto>(losItems, laPagina.Pagina, laPagina.Tamano, elTotal);
    }

    public Task<Paginado<RecetaResumenDto>> ListeLasRecetasDelAutor(int elAutorId, string? elEstado, SolicitudDePagina laPagina,
        CancellationToken cancelacion)
    {
        var laConsulta = laBaseDeDatos.Recetas.AsNoTracking().Where(receta => receta.UsuarioId == elAutorId);
        if (elEstado is not null) laConsulta = laConsulta.Where(receta => receta.Estado == elEstado);
        return ProyecteElResumen(laConsulta.OrderByDescending(receta => receta.FechaCreacion).ThenByDescending(receta => receta.RecetaId))
            .PagineLaConsulta(laPagina, cancelacion);
    }

    public async Task<RecetaDetalleDto?> BusqueElDetalleDeLaReceta(int laRecetaId, int? elUsuarioId, CancellationToken cancelacion)
    {
        var laReceta = await laBaseDeDatos.Recetas.AsNoTracking().AsSplitQuery()
            .Include(receta => receta.Categoria).Include(receta => receta.Dificultad).Include(receta => receta.Usuario)
            .Include(receta => receta.RecetaIngredientes).ThenInclude(detalle => detalle.Ingrediente)
            .Include(receta => receta.RecetaIngredientes).ThenInclude(detalle => detalle.Unidad)
            .Include(receta => receta.RecetaPasos).Include(receta => receta.Multimedia)
            .FirstOrDefaultAsync(receta => receta.RecetaId == laRecetaId, cancelacion);
        if (laReceta is null) return null;

        var lasCalificaciones = await laBaseDeDatos.Comentarios.Where(comentario => comentario.RecetaId == laRecetaId && comentario.Calificacion != null)
            .GroupBy(_ => 1)
            .Select(grupo => new { Promedio = grupo.Average(comentario => (decimal)comentario.Calificacion!), Total = grupo.Count() })
            .FirstOrDefaultAsync(cancelacion);
        var elTotalDeMeGusta = await laBaseDeDatos.MeGusta.CountAsync(meGusta => meGusta.RecetaId == laRecetaId, cancelacion);
        var elTotalDeFavoritos = await laBaseDeDatos.Favoritos.CountAsync(favorito => favorito.RecetaId == laRecetaId, cancelacion);

        bool? leDioMeGusta = null, loTieneEnFavoritos = null;
        int? suCalificacion = null;
        if (elUsuarioId is int elId)
        {
            leDioMeGusta = await laBaseDeDatos.MeGusta.AnyAsync(meGusta => meGusta.RecetaId == laRecetaId && meGusta.UsuarioId == elId, cancelacion);
            loTieneEnFavoritos = await laBaseDeDatos.Favoritos.AnyAsync(favorito => favorito.RecetaId == laRecetaId && favorito.UsuarioId == elId, cancelacion);
            suCalificacion = await laBaseDeDatos.Comentarios.Where(comentario => comentario.RecetaId == laRecetaId && comentario.UsuarioId == elId)
                .Select(comentario => (int?)comentario.Calificacion).FirstOrDefaultAsync(cancelacion);
        }

        return new RecetaDetalleDto(laReceta.RecetaId, laReceta.Titulo, laReceta.Descripcion, laReceta.CategoriaId, laReceta.Categoria.Nombre,
            laReceta.DificultadId, laReceta.Dificultad.Nombre, laReceta.TiempoPrepMin, laReceta.PorcionesBase, laReceta.Origen, laReceta.Estado,
            laReceta.FechaCreacion, new AutorDto(laReceta.UsuarioId, $"{laReceta.Usuario.Nombre} {laReceta.Usuario.Apellidos}"),
            laReceta.RecetaIngredientes.OrderBy(detalle => detalle.Ingrediente.Nombre)
                .Select(detalle => new IngredienteRecetaDto(detalle.IngredienteId, detalle.Ingrediente.Nombre, detalle.Cantidad,
                    detalle.UnidadId, detalle.Unidad.Abreviatura, detalle.Nota)).ToList(),
            laReceta.RecetaPasos.OrderBy(paso => paso.NumeroPaso).Select(paso => new PasoDto(paso.NumeroPaso, paso.Descripcion)).ToList(),
            laReceta.Multimedia.OrderByDescending(archivo => archivo.EsPortada).ThenBy(archivo => archivo.MultimediaId)
                .Select(archivo => new MultimediaDto(archivo.MultimediaId, archivo.Tipo, archivo.Ruta, archivo.Descripcion, archivo.EsPortada)).ToList(),
            elTotalDeMeGusta, elTotalDeFavoritos, lasCalificaciones is null ? null : Math.Round(lasCalificaciones.Promedio, 2),
            lasCalificaciones?.Total ?? 0, leDioMeGusta, loTieneEnFavoritos, suCalificacion);
    }

    public async Task<IReadOnlyList<IngredienteRecetaDto>> CalculeLosIngredientesParaLasPorciones(int laRecetaId, int lasPorciones,
        CancellationToken cancelacion)
    {
        var lasFilas = await laBaseDeDatos.Database
            .SqlQuery<FilaDeIngredientePorPorcion>($"SELECT * FROM sp_ingredientes_por_porciones({laRecetaId}, {lasPorciones})")
            .ToListAsync(cancelacion);
        return lasFilas.Select(laFila => new IngredienteRecetaDto(laFila.IngredienteId, laFila.Ingrediente, laFila.Cantidad,
            laFila.UnidadId, laFila.Unidad, laFila.Nota)).ToList();
    }

    public async Task<long> AgregueLaBusqueda(int? elUsuarioId, string elTermino, short? laCategoriaId, int laCantidadDeResultados,
        CancellationToken cancelacion)
    {
        var laBusqueda = new Busqueda
        {
            UsuarioId = elUsuarioId,
            Termino = elTermino,
            CategoriaId = laCategoriaId,
            CantidadResultados = laCantidadDeResultados
        };
        laBaseDeDatos.Busquedas.Add(laBusqueda);
        await laBaseDeDatos.SaveChangesAsync(cancelacion);
        return laBusqueda.BusquedaId;
    }

    public Task<bool> LaBusquedaEsValidaParaElUsuario(long laBusquedaId, int? elUsuarioId, CancellationToken cancelacion) =>
        laBaseDeDatos.Busquedas.AnyAsync(
            busqueda => busqueda.BusquedaId == laBusquedaId && (busqueda.UsuarioId == null || busqueda.UsuarioId == elUsuarioId), cancelacion);

    public async Task AgregueLaVisita(int laRecetaId, int? elUsuarioId, long? laBusquedaId, CancellationToken cancelacion)
    {
        laBaseDeDatos.VisitasReceta.Add(new VisitaReceta { RecetaId = laRecetaId, UsuarioId = elUsuarioId, BusquedaId = laBusquedaId });
        await laBaseDeDatos.SaveChangesAsync(cancelacion);
    }
}
