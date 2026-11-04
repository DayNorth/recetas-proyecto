using Recetas.Api.Aplicacion.Dtos;
using Recetas.Api.Aplicacion.Puertos;
using Recetas.Api.Dominio;

namespace Recetas.Api.Aplicacion.Servicios;

public class ServicioDeCatalogos(IRepositorioDeCatalogos losCatalogos)
{
    private const int MAXIMO_DE_SUGERENCIAS = 20;
    private const int LARGO_MAXIMO_DEL_PREFIJO = 100;

    public Task<IReadOnlyList<CategoriaDto>> ListeLasCategorias(CancellationToken cancelacion) =>
        losCatalogos.ListeLasCategoriasActivas(cancelacion);

    public Task<IReadOnlyList<DificultadDto>> ListeLasDificultades(CancellationToken cancelacion) =>
        losCatalogos.ListeLasDificultades(cancelacion);

    public Task<IReadOnlyList<UnidadDto>> ListeLasUnidades(CancellationToken cancelacion) =>
        losCatalogos.ListeLasUnidades(cancelacion);

    public Task<IReadOnlyList<RedSocialDto>> ListeLasRedesSociales(CancellationToken cancelacion) =>
        losCatalogos.ListeLasRedesSociales(cancelacion);

    public Task<IReadOnlyList<IngredienteDto>> SugieraIngredientes(string? elTexto, CancellationToken cancelacion)
    {
        var elPrefijo = Texto.LimiteElLargo((elTexto ?? "").Trim().ToLower(), LARGO_MAXIMO_DEL_PREFIJO);
        return losCatalogos.BusqueLosIngredientesPorPrefijo(elPrefijo, MAXIMO_DE_SUGERENCIAS, cancelacion);
    }
}
