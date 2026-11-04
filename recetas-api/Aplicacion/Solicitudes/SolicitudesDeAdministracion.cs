using FluentValidation;

namespace Recetas.Api.Aplicacion.Solicitudes;

public record CategoriaRequest(string Nombre, string? Descripcion);
public record EstadoRequest(bool Activo);

public record FiltroDeUsuarios(string? Texto, bool? Activo, string? Rol);

public class CategoriaRequestValidator : AbstractValidator<CategoriaRequest>
{
    public CategoriaRequestValidator()
    {
        RuleFor(x => x.Nombre).NotEmpty().WithMessage("El nombre es obligatorio.")
            .MaximumLength(60).WithMessage("El nombre no puede superar 60 caracteres.");
        RuleFor(x => x.Descripcion).MaximumLength(200).WithMessage("La descripción no puede superar 200 caracteres.");
    }
}
