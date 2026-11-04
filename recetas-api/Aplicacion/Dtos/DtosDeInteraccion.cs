namespace Recetas.Api.Aplicacion.Dtos;

public record ToggleDto(bool Activo, int Total);
public record FavoritoDto(int RecetaId, string Titulo, string? Portada, string? Categoria, short? TiempoPrepMin, bool Disponible, DateTime FechaFavorito);
public record ComentarioDto(int ComentarioId, int UsuarioId, string Usuario, short? Calificacion, string? Comentario, DateTime Fecha);
public record ComentariosPage(IReadOnlyList<ComentarioDto> Items, int Pagina, int Tamano, int Total, decimal? Promedio, int TotalCalificaciones);
public record CompartirResponse(string Url, string? Enlace);
