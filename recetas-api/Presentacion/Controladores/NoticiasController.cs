using FluentValidation;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Recetas.Api.Aplicacion.Comun;
using Recetas.Api.Aplicacion.Dtos;
using Recetas.Api.Aplicacion.Servicios;
using Recetas.Api.Aplicacion.Solicitudes;
using Recetas.Api.Presentacion.Seguridad;

namespace Recetas.Api.Presentacion.Controladores;

public class NoticiaForm
{
    public string? Titulo { get; set; }
    public string? Contenido { get; set; }
    public bool? Publicada { get; set; }
    public IFormFile? Imagen { get; set; }
    public bool QuitarImagen { get; set; }

    public DatosDeLaNoticia ConviertaALosDatos() =>
        new(Titulo!, Contenido!, Publicada, Imagen.ConviertaAlArchivoSubido(), QuitarImagen);
}

public class NoticiaFormValidator : AbstractValidator<NoticiaForm>
{
    public NoticiaFormValidator()
    {
        RuleFor(x => x.Titulo).NotEmpty().WithMessage("El título es obligatorio.")
            .MaximumLength(150).WithMessage("El título no puede superar 150 caracteres.");
        RuleFor(x => x.Contenido).NotEmpty().WithMessage("El contenido es obligatorio.")
            .MaximumLength(10000).WithMessage("El contenido no puede superar 10000 caracteres.");
    }
}

[ApiController]
[Route("api/noticias")]
public class NoticiasController(ServicioDeNoticias lasNoticias) : ControllerBase
{
    [HttpGet]
    [AllowAnonymous]
    public Task<Paginado<NoticiaDto>> ListeLasNoticias([FromQuery(Name = "incluirNoPublicadas")] bool incluirNoPublicadas,
        [FromQuery(Name = "pagina")] int? laPagina, [FromQuery(Name = "tamano")] int? elTamano, CancellationToken cancelacion) =>
        lasNoticias.ListeLasNoticias(incluirNoPublicadas, laPagina, elTamano, cancelacion);

    [HttpGet("{id:int}")]
    [AllowAnonymous]
    public Task<NoticiaDto> ObtengaLaNoticia([FromRoute(Name = "id")] int laNoticiaId, CancellationToken cancelacion) =>
        lasNoticias.ObtengaLaNoticia(laNoticiaId, cancelacion);

    [HttpPost]
    [Authorize(Policy = Politicas.SOLO_ADMIN)]
    [Consumes("multipart/form-data")]
    [ProducesResponseType<NoticiaDto>(StatusCodes.Status201Created)]
    public async Task<IActionResult> CreeLaNoticia([FromForm] NoticiaForm elFormulario, CancellationToken cancelacion)
    {
        var laNoticia = await lasNoticias.CreeLaNoticia(elFormulario.ConviertaALosDatos(), cancelacion);
        return CreatedAtAction(nameof(ObtengaLaNoticia), new { id = laNoticia.NoticiaId }, laNoticia);
    }

    [HttpPut("{id:int}")]
    [Authorize(Policy = Politicas.SOLO_ADMIN)]
    [Consumes("multipart/form-data")]
    public Task<NoticiaDto> EditeLaNoticia([FromRoute(Name = "id")] int laNoticiaId, [FromForm] NoticiaForm elFormulario,
        CancellationToken cancelacion) =>
        lasNoticias.EditeLaNoticia(laNoticiaId, elFormulario.ConviertaALosDatos(), cancelacion);

    [HttpDelete("{id:int}")]
    [Authorize(Policy = Politicas.SOLO_ADMIN)]
    public async Task<IActionResult> DespubliqueLaNoticia([FromRoute(Name = "id")] int laNoticiaId, CancellationToken cancelacion)
    {
        await lasNoticias.DespubliqueLaNoticia(laNoticiaId, cancelacion);
        return NoContent();
    }
}
