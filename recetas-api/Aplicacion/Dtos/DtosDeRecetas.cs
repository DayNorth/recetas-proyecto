namespace Recetas.Api.Aplicacion.Dtos;

public record RecetaResumenDto(int RecetaId, string Titulo, string? Descripcion, string? Portada,
    string Categoria, string Dificultad, short TiempoPrepMin, int MeGusta,
    decimal? CalificacionPromedio, int TotalCalificaciones, string Estado);

public record ResultadoBusqueda(IReadOnlyList<RecetaResumenDto> Items, int Pagina, int Tamano, int Total, long? BusquedaId);

public record IngredienteRecetaDto(int IngredienteId, string Ingrediente, decimal Cantidad, short UnidadId,
    string Unidad, string? Nota);

public record PasoDto(short Numero, string Descripcion);

public record MultimediaDto(int MultimediaId, string Tipo, string Ruta, string? Descripcion, bool EsPortada);

public record AutorDto(int UsuarioId, string Nombre);

public record RecetaDetalleDto(int RecetaId, string Titulo, string? Descripcion, short CategoriaId, string Categoria,
    short DificultadId, string Dificultad, short TiempoPrepMin, short PorcionesBase, string Origen, string Estado,
    DateTime FechaCreacion, AutorDto Autor,
    IReadOnlyList<IngredienteRecetaDto> Ingredientes, IReadOnlyList<PasoDto> Pasos, IReadOnlyList<MultimediaDto> Multimedia,
    int TotalMeGusta, int TotalFavoritos, decimal? CalificacionPromedio, int TotalCalificaciones,
    bool? MeGusta, bool? Favorito, int? MiCalificacion);

public record IngredientesPorPorcionesDto(int RecetaId, int Porciones, int PorcionesBase,
    IReadOnlyList<IngredienteRecetaDto> Ingredientes);

public record RecetaCreadaDto(int RecetaId);
