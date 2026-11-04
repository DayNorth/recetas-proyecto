using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Recetas.Api.Aplicacion.Dtos;
using Recetas.Api.Aplicacion.Servicios;

namespace Recetas.Api.Presentacion.Controladores;

[ApiController]
[AllowAnonymous]
[Route("api")]
public class CatalogosController(ServicioDeCatalogos losCatalogos) : ControllerBase
{
    [HttpGet("categorias")]
    public Task<IReadOnlyList<CategoriaDto>> ListeLasCategorias(CancellationToken cancelacion) =>
        losCatalogos.ListeLasCategorias(cancelacion);

    [HttpGet("dificultades")]
    public Task<IReadOnlyList<DificultadDto>> ListeLasDificultades(CancellationToken cancelacion) =>
        losCatalogos.ListeLasDificultades(cancelacion);

    [HttpGet("unidades")]
    public Task<IReadOnlyList<UnidadDto>> ListeLasUnidades(CancellationToken cancelacion) =>
        losCatalogos.ListeLasUnidades(cancelacion);

    [HttpGet("redes-sociales")]
    public Task<IReadOnlyList<RedSocialDto>> ListeLasRedesSociales(CancellationToken cancelacion) =>
        losCatalogos.ListeLasRedesSociales(cancelacion);

    [HttpGet("ingredientes")]
    public Task<IReadOnlyList<IngredienteDto>> SugieraIngredientes([FromQuery(Name = "q")] string? elTexto, CancellationToken cancelacion) =>
        losCatalogos.SugieraIngredientes(elTexto, cancelacion);
}
