namespace Recetas.Api.Aplicacion.Dtos;

public record NoticiaDto(int NoticiaId, string Titulo, string Contenido, string? Imagen, bool Publicada,
    DateTime FechaPublicacion, string Autor);
