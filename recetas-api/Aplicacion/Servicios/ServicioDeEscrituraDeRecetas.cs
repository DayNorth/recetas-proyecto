using Recetas.Api.Aplicacion.Puertos;
using Recetas.Api.Aplicacion.Solicitudes;
using Recetas.Api.Dominio;

namespace Recetas.Api.Aplicacion.Servicios;

public class ServicioDeEscrituraDeRecetas(IRepositorioDeRecetas lasRecetas, IRepositorioDeCatalogos losCatalogos,
    ControlDeAccesoARecetas elControlDeAcceso, IEjecutorDeTransacciones elEjecutor, IAlmacenDeArchivos losArchivos,
    IPuntoDeFalla elPuntoDeFalla, IUsuarioActual elUsuarioActual)
{
    public const long MAXIMO_DE_BYTES_DE_LA_IMAGEN = 5 * 1024 * 1024;

    public async Task<int> RegistreLaReceta(CrearRecetaRequest laSolicitud, ArchivoSubido? laImagen, CancellationToken cancelacion)
    {
        await VerifiqueLasReferencias(laSolicitud, categoriaDebeEstarActiva: true, cancelacion);

        string? laRutaDeLaImagen = null;
        if (laImagen is { Longitud: > 0 })
        {
            await using var elFlujo = laImagen.AbraElFlujo();
            laRutaDeLaImagen = await losArchivos.GuardeElArchivo(elFlujo, "recetas", true, false, MAXIMO_DE_BYTES_DE_LA_IMAGEN, cancelacion);
        }

        try
        {
            return await elEjecutor.EjecuteEnTransaccion(async laCancelacion =>
            {
                var losIngredientesPorClave = await AgregueLosIngredientesNuevos(laSolicitud.Ingredientes, laCancelacion);

                var laRecetaId = await lasRecetas.AgregueLaReceta(
                    new NuevaReceta(elUsuarioActual.IdRequerido, ArmeLosDatos(laSolicitud), OrigenesDeLaReceta.LOCAL),
                    laCancelacion);
                elPuntoDeFalla.PaseElPuntoDeFalla("receta-insertada");

                var laPortada = laRutaDeLaImagen is null ? [] : new[] { new MultimediaNueva(TiposDeMultimedia.IMAGEN, laRutaDeLaImagen, true) };
                await lasRecetas.AgregueElDetalleDeLaReceta(laRecetaId, ArmeElDetalle(laSolicitud, losIngredientesPorClave, laPortada), laCancelacion);
                elPuntoDeFalla.PaseElPuntoDeFalla("detalle-insertado");

                return laRecetaId;
            }, cancelacion);
        }
        catch
        {
            losArchivos.BorreElArchivo(laRutaDeLaImagen);
            throw;
        }
    }

    public async Task ActualiceLaReceta(int laRecetaId, CrearRecetaRequest laSolicitud, CancellationToken cancelacion)
    {
        var laRecetaActual = await elControlDeAcceso.ExijaAlAutor(laRecetaId, cancelacion);
        if (laRecetaActual.Estado == EstadosDeLaReceta.MODERADA)
            throw ExcepcionDeNegocio.PorConflicto("La receta fue moderada y no se puede editar.");

        await VerifiqueLasReferencias(laSolicitud, categoriaDebeEstarActiva: laSolicitud.CategoriaId != laRecetaActual.CategoriaId, cancelacion);

        await elEjecutor.EjecuteEnTransaccion(async laCancelacion =>
        {
            var losIngredientesPorClave = await AgregueLosIngredientesNuevos(laSolicitud.Ingredientes, laCancelacion);

            await lasRecetas.ActualiceLosDatosDeLaReceta(laRecetaId, ArmeLosDatos(laSolicitud), laCancelacion);
            await lasRecetas.BorreElDetalleDeLaReceta(laRecetaId, laCancelacion);
            elPuntoDeFalla.PaseElPuntoDeFalla("detalle-borrado");

            await lasRecetas.AgregueElDetalleDeLaReceta(laRecetaId, ArmeElDetalle(laSolicitud, losIngredientesPorClave, []), laCancelacion);
        }, cancelacion);
    }

    public async Task DesactiveLaReceta(int laRecetaId, CancellationToken cancelacion)
    {
        var laReceta = await elControlDeAcceso.ExijaAlAutor(laRecetaId, cancelacion);
        if (laReceta.Estado == EstadosDeLaReceta.MODERADA)
            throw ExcepcionDeNegocio.PorConflicto("La receta fue moderada; solo un administrador puede restaurarla.");
        if (laReceta.Estado != EstadosDeLaReceta.INACTIVA)
            await lasRecetas.EstablezcaElEstadoDeLaReceta(laRecetaId, EstadosDeLaReceta.INACTIVA, cancelacion);
    }

    private static DatosDeLaReceta ArmeLosDatos(CrearRecetaRequest laSolicitud) =>
        new(laSolicitud.CategoriaId, laSolicitud.DificultadId, Texto.NormaliceElTexto(laSolicitud.Titulo),
            Texto.RecorteOMarqueNulo(laSolicitud.Descripcion), (short)laSolicitud.TiempoPrepMin, (short)laSolicitud.PorcionesBase);

    private static DetalleDeLaReceta ArmeElDetalle(CrearRecetaRequest laSolicitud,
        IReadOnlyDictionary<string, int> losIngredientesPorClave, IReadOnlyList<MultimediaNueva> laMultimedia)
    {
        var losIngredientesVistos = new HashSet<int>();
        var losIngredientes = new List<IngredienteDeLaReceta>();
        foreach (var laEntrada in laSolicitud.Ingredientes)
        {
            var elIngredienteId = laEntrada.IngredienteId ?? losIngredientesPorClave[Texto.CalculeLaClaveDelNombre(laEntrada.Nombre!)];
            if (!losIngredientesVistos.Add(elIngredienteId))
                throw ExcepcionDeNegocio.PorConflicto("Un ingrediente está repetido en la receta.");
            losIngredientes.Add(new IngredienteDeLaReceta(elIngredienteId, laEntrada.UnidadId, laEntrada.Cantidad,
                Texto.RecorteOMarqueNulo(laEntrada.Nota)));
        }
        return new DetalleDeLaReceta(losIngredientes, laSolicitud.Pasos.Select(elPaso => elPaso.Trim()).ToList(), laMultimedia);
    }

    private Task<IReadOnlyDictionary<string, int>> AgregueLosIngredientesNuevos(IEnumerable<IngredienteEntrada> lasEntradas,
        CancellationToken cancelacion)
    {
        var losNombres = lasEntradas.Where(laEntrada => laEntrada.IngredienteId is null)
            .Select(laEntrada => Texto.NormaliceElTexto(laEntrada.Nombre!))
            .DistinctBy(Texto.CalculeLaClaveDelNombre)
            .ToList();
        return losCatalogos.AgregueLosIngredientesFaltantes(losNombres, cancelacion);
    }

    private async Task VerifiqueLasReferencias(CrearRecetaRequest laSolicitud, bool categoriaDebeEstarActiva, CancellationToken cancelacion)
    {
        var laCategoriaEstaActiva = await losCatalogos.ConsulteSiLaCategoriaEstaActiva(laSolicitud.CategoriaId, cancelacion)
                                    ?? throw ExcepcionDeNegocio.PorSolicitudInvalida("La categoría no existe.");
        if (categoriaDebeEstarActiva && !laCategoriaEstaActiva)
            throw ExcepcionDeNegocio.PorSolicitudInvalida("La categoría está inactiva.");
        if (!await losCatalogos.ExisteLaDificultad(laSolicitud.DificultadId, cancelacion))
            throw ExcepcionDeNegocio.PorSolicitudInvalida("La dificultad no existe.");

        var lasUnidades = laSolicitud.Ingredientes.Select(laEntrada => laEntrada.UnidadId).Distinct().ToList();
        if (await losCatalogos.CuenteLasUnidadesExistentes(lasUnidades, cancelacion) != lasUnidades.Count)
            throw ExcepcionDeNegocio.PorSolicitudInvalida("Alguna unidad de medida no existe.");

        var losIngredientesPorId = laSolicitud.Ingredientes.Where(laEntrada => laEntrada.IngredienteId is not null)
            .Select(laEntrada => laEntrada.IngredienteId!.Value).Distinct().ToList();
        if (losIngredientesPorId.Count > 0
            && await losCatalogos.CuenteLosIngredientesExistentes(losIngredientesPorId, cancelacion) != losIngredientesPorId.Count)
            throw ExcepcionDeNegocio.PorSolicitudInvalida("Algún ingrediente indicado por id no existe.");
    }
}
