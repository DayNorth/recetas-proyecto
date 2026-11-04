namespace Recetas.Api.Aplicacion.Dtos;

public record CategoriaDto(short CategoriaId, string Nombre, string? Descripcion);
public record DificultadDto(short DificultadId, string Nombre);
public record UnidadDto(short UnidadId, string Nombre, string Abreviatura, bool Escalable);
public record RedSocialDto(short RedId, string Nombre);
public record IngredienteDto(int IngredienteId, string Nombre);
public record CategoriaAdminDto(short CategoriaId, string Nombre, string? Descripcion, bool Activo, int TotalRecetas);
