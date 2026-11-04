namespace Recetas.Api.Aplicacion.Dtos;

public record UsuarioDto(int UsuarioId, string Nombre, string Apellidos, string Correo, string Rol,
    string? FotoPerfil, string? Descripcion, DateTime FechaRegistro);

public record LoginResponse(string Token, DateTimeOffset Expira, UsuarioDto Usuario);

public record UsuarioAdminDto(int UsuarioId, string Nombre, string Apellidos, string Correo, string Rol, bool Activo,
    DateTime FechaRegistro, DateTime? UltimoAcceso);
