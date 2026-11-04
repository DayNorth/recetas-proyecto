using Recetas.Api.Aplicacion.Comun;
using Recetas.Api.Aplicacion.Dtos;
using Recetas.Api.Aplicacion.Puertos;
using Recetas.Api.Aplicacion.Solicitudes;
using Recetas.Api.Dominio;

namespace Recetas.Api.Aplicacion.Servicios;

public class ServicioDeConsultaDeRecetas(IConsultasDeRecetas lasConsultas, IRepositorioDeRecetas lasRecetas,
    IRepositorioDeCatalogos losCatalogos, ControlDeAccesoARecetas elControlDeAcceso, IUsuarioActual elUsuarioActual)
{
    private const string ORDEN_RECIENTES = "recientes";
    private static readonly string[] ORDENES_VALIDOS = [ORDEN_RECIENTES, "populares", "tiempo"];
    private const int MAXIMO_DE_INGREDIENTES_EN_EL_FILTRO = 20;
    private const int LARGO_MAXIMO_DEL_TEXTO = 100;
    private const int MINIMO_DE_PORCIONES = 1;
    private const int MAXIMO_DE_PORCIONES = 100;

    public async Task<ResultadoBusqueda> BusqueLasRecetas(ConsultaDelCatalogo laConsulta, CancellationToken cancelacion)
    {
        var elOrden = string.IsNullOrWhiteSpace(laConsulta.Orden) ? ORDEN_RECIENTES : laConsulta.Orden.Trim().ToLowerInvariant();
        if (!ORDENES_VALIDOS.Contains(elOrden))
            throw ExcepcionDeNegocio.PorSolicitudInvalida("Orden inválido.", "Use: recientes, populares o tiempo.");
        if (laConsulta.TiempoMaximo is <= 0)
            throw ExcepcionDeNegocio.PorSolicitudInvalida("tiempoMax debe ser mayor que 0.");
        var losIngredientes = (laConsulta.Ingredientes ?? []).Distinct().ToList();
        if (losIngredientes.Count > MAXIMO_DE_INGREDIENTES_EN_EL_FILTRO)
            throw ExcepcionDeNegocio.PorSolicitudInvalida("Se pueden filtrar hasta 20 ingredientes.");

        var elTexto = laConsulta.Texto?.Trim();
        if (elTexto is not null) elTexto = Texto.LimiteElLargo(elTexto, LARGO_MAXIMO_DEL_TEXTO);

        var elFiltro = new FiltroDeRecetas(elTexto, laConsulta.CategoriaId, laConsulta.DificultadId, laConsulta.TiempoMaximo,
            losIngredientes, elOrden);
        var laPagina = SolicitudDePagina.Normalice(laConsulta.Pagina, laConsulta.Tamano);
        var laPaginaDeRecetas = await lasConsultas.BusqueLasRecetasPublicadas(elFiltro, laPagina, cancelacion);

        long? laBusquedaId = null;
        var hayCriterio = !string.IsNullOrEmpty(elTexto) || losIngredientes.Count > 0 || laConsulta.CategoriaId is not null
                          || laConsulta.DificultadId is not null || laConsulta.TiempoMaximo is not null;
        if (laConsulta.Registrar && hayCriterio)
            laBusquedaId = await RegistreLaBusqueda(elFiltro, laPaginaDeRecetas.Total, cancelacion);

        return new ResultadoBusqueda(laPaginaDeRecetas.Items.Select(RedondeeLaCalificacion).ToList(),
            laPagina.Pagina, laPagina.Tamano, laPaginaDeRecetas.Total, laBusquedaId);
    }

    public async Task<RecetaDetalleDto> ObtengaElDetalle(int laRecetaId, long? laBusquedaId, CancellationToken cancelacion)
    {
        int? elUsuarioId = elUsuarioActual.EstaAutenticado ? elUsuarioActual.IdRequerido : null;
        var elDetalle = await lasConsultas.BusqueElDetalleDeLaReceta(laRecetaId, elUsuarioId, cancelacion);
        if (elDetalle is null || !elControlDeAcceso.PuedeVerLaReceta(elDetalle.Estado, elDetalle.Autor.UsuarioId))
            throw ExcepcionDeNegocio.PorRecursoInexistente("La receta no existe.");

        if (elDetalle.Estado == EstadosDeLaReceta.PUBLICADA && elUsuarioActual.Id != elDetalle.Autor.UsuarioId)
            await RegistreLaVisita(laRecetaId, laBusquedaId, cancelacion);

        return elDetalle;
    }

    public async Task<IngredientesPorPorcionesDto> CalculeLosIngredientesPorPorciones(int laRecetaId, int? lasPorciones,
        CancellationToken cancelacion)
    {
        if (lasPorciones is < MINIMO_DE_PORCIONES or > MAXIMO_DE_PORCIONES)
            throw ExcepcionDeNegocio.PorSolicitudInvalida("porciones debe estar entre 1 y 100.");

        var laReceta = await lasRecetas.BusqueLaRecetaBasica(laRecetaId, cancelacion);
        if (laReceta is null || !elControlDeAcceso.PuedeVerLaReceta(laReceta.Estado, laReceta.AutorId))
            throw ExcepcionDeNegocio.PorRecursoInexistente("La receta no existe.");

        var lasPorcionesPedidas = lasPorciones ?? laReceta.PorcionesBase;
        var losIngredientes = await lasConsultas.CalculeLosIngredientesParaLasPorciones(laRecetaId, lasPorcionesPedidas, cancelacion);
        return new IngredientesPorPorcionesDto(laRecetaId, lasPorcionesPedidas, laReceta.PorcionesBase, losIngredientes);
    }

    public async Task<Paginado<RecetaResumenDto>> ListeMisRecetas(string? elEstado, int? laPagina, int? elTamano,
        CancellationToken cancelacion)
    {
        string? elEstadoNormalizado = null;
        if (!string.IsNullOrWhiteSpace(elEstado))
        {
            elEstadoNormalizado = elEstado.Trim().ToUpperInvariant();
            if (!EstadosDeLaReceta.TODOS.Contains(elEstadoNormalizado))
                throw ExcepcionDeNegocio.PorSolicitudInvalida("Estado inválido.", "Use: BORRADOR, PUBLICADA, INACTIVA o MODERADA.");
        }

        var laPaginaDeRecetas = await lasConsultas.ListeLasRecetasDelAutor(elUsuarioActual.IdRequerido, elEstadoNormalizado,
            SolicitudDePagina.Normalice(laPagina, elTamano), cancelacion);
        return laPaginaDeRecetas with { Items = laPaginaDeRecetas.Items.Select(RedondeeLaCalificacion).ToList() };
    }

    private static RecetaResumenDto RedondeeLaCalificacion(RecetaResumenDto laReceta) =>
        laReceta with { CalificacionPromedio = laReceta.CalificacionPromedio is null ? null : Math.Round(laReceta.CalificacionPromedio.Value, 2) };

    private async Task<long> RegistreLaBusqueda(FiltroDeRecetas elFiltro, int elTotal, CancellationToken cancelacion)
    {
        var elTermino = elFiltro.Texto;
        if (string.IsNullOrEmpty(elTermino) && elFiltro.Ingredientes.Count > 0)
            elTermino = string.Join(", ", await losCatalogos.ListeLosNombresDeIngredientes(elFiltro.Ingredientes.ToList(), cancelacion));
        if (string.IsNullOrEmpty(elTermino) && elFiltro.CategoriaId is not null)
            elTermino = await losCatalogos.BusqueElNombreDeLaCategoria(elFiltro.CategoriaId.Value, cancelacion);
        if (string.IsNullOrEmpty(elTermino) && elFiltro.DificultadId is not null)
            elTermino = await losCatalogos.BusqueElNombreDeLaDificultad(elFiltro.DificultadId.Value, cancelacion);
        if (string.IsNullOrEmpty(elTermino)) elTermino = $"hasta {elFiltro.TiempoMaximo} min";
        elTermino = Texto.LimiteElLargo(elTermino, LARGO_MAXIMO_DEL_TEXTO);

        short? laCategoriaId = elFiltro.CategoriaId is not null
                               && await losCatalogos.ConsulteSiLaCategoriaEstaActiva(elFiltro.CategoriaId.Value, cancelacion) is not null
            ? elFiltro.CategoriaId
            : null;

        return await lasConsultas.AgregueLaBusqueda(elUsuarioActual.Id, elTermino, laCategoriaId, elTotal, cancelacion);
    }

    private async Task RegistreLaVisita(int laRecetaId, long? laBusquedaId, CancellationToken cancelacion)
    {
        long? laBusquedaValida = null;
        if (laBusquedaId is not null
            && await lasConsultas.LaBusquedaEsValidaParaElUsuario(laBusquedaId.Value, elUsuarioActual.Id, cancelacion))
            laBusquedaValida = laBusquedaId;

        await lasConsultas.AgregueLaVisita(laRecetaId, elUsuarioActual.Id, laBusquedaValida, cancelacion);
    }
}
