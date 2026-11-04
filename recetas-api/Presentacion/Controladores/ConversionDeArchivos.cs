using Recetas.Api.Aplicacion.Puertos;

namespace Recetas.Api.Presentacion.Controladores;

public static class ConversionDeArchivos
{
    public static ArchivoSubido? ConviertaAlArchivoSubido(this IFormFile? elArchivoDelFormulario) =>
        elArchivoDelFormulario is null
            ? null
            : new ArchivoSubido(elArchivoDelFormulario.Length, elArchivoDelFormulario.OpenReadStream);
}
