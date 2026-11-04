using Recetas.Api.Aplicacion.Comun;
using Recetas.Api.Aplicacion.Dtos;
using Recetas.Api.Aplicacion.Solicitudes;

namespace Recetas.Api.Aplicacion.Puertos;

public record RecetaBasica(int RecetaId, string Titulo, int AutorId, string Estado, short CategoriaId, short PorcionesBase);

public record NuevaReceta(int AutorId, DatosDeLaReceta Datos, string Origen);

public record DatosDeLaReceta(short CategoriaId, short DificultadId, string Titulo, string? Descripcion,
    short TiempoPrepMin, short PorcionesBase);

public record IngredienteDeLaReceta(int IngredienteId, short UnidadId, decimal Cantidad, string? Nota);

public record MultimediaNueva(string Tipo, string Ruta, bool EsPortada);

public record DetalleDeLaReceta(IReadOnlyList<IngredienteDeLaReceta> Ingredientes, IReadOnlyList<string> Pasos,
    IReadOnlyList<MultimediaNueva> Multimedia);

public interface IRepositorioDeRecetas
{
    Task<RecetaBasica?> BusqueLaRecetaBasica(int laRecetaId, CancellationToken cancelacion);

    Task<int> AgregueLaReceta(NuevaReceta laReceta, CancellationToken cancelacion);

    Task ActualiceLosDatosDeLaReceta(int laRecetaId, DatosDeLaReceta losDatos, CancellationToken cancelacion);

    Task BorreElDetalleDeLaReceta(int laRecetaId, CancellationToken cancelacion);

    Task AgregueElDetalleDeLaReceta(int laRecetaId, DetalleDeLaReceta elDetalle, CancellationToken cancelacion);

    Task EstablezcaElEstadoDeLaReceta(int laRecetaId, string elEstado, CancellationToken cancelacion);

    Task<bool> CambieElEstadoSiEs(int laRecetaId, string elEstadoActual, string elEstadoNuevo, CancellationToken cancelacion);
}

public interface IConsultasDeRecetas
{
    Task<Paginado<RecetaResumenDto>> BusqueLasRecetasPublicadas(FiltroDeRecetas elFiltro, SolicitudDePagina laPagina,
        CancellationToken cancelacion);

    Task<Paginado<RecetaResumenDto>> ListeLasRecetasDelAutor(int elAutorId, string? elEstado, SolicitudDePagina laPagina,
        CancellationToken cancelacion);

    Task<RecetaDetalleDto?> BusqueElDetalleDeLaReceta(int laRecetaId, int? elUsuarioId, CancellationToken cancelacion);

    Task<IReadOnlyList<IngredienteRecetaDto>> CalculeLosIngredientesParaLasPorciones(int laRecetaId, int lasPorciones,
        CancellationToken cancelacion);

    Task<long> AgregueLaBusqueda(int? elUsuarioId, string elTermino, short? laCategoriaId, int laCantidadDeResultados,
        CancellationToken cancelacion);

    Task<bool> LaBusquedaEsValidaParaElUsuario(long laBusquedaId, int? elUsuarioId, CancellationToken cancelacion);

    Task AgregueLaVisita(int laRecetaId, int? elUsuarioId, long? laBusquedaId, CancellationToken cancelacion);
}
