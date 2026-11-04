using FluentValidation;

namespace Recetas.Api.Aplicacion.Solicitudes;

public record RegistroRequest(string Nombre, string Apellidos, string Correo, string Password, string ConfirmacionPassword);

public record LoginRequest(string Correo, string Password);

public record ActualizarPerfilRequest(string Nombre, string Apellidos, string? Descripcion);

public static class ReglasComunes
{
    public static IRuleBuilderOptions<T, string> ExijaUnaContrasenaSegura<T>(this IRuleBuilder<T, string> laRegla) =>
        laRegla.NotEmpty().WithMessage("La contraseña es obligatoria.")
            .MinimumLength(8).WithMessage("La contraseña debe tener al menos 8 caracteres.")
            .MaximumLength(72).WithMessage("La contraseña no puede superar 72 caracteres.")
            .Must(laClave => laClave.Any(char.IsLetter)).WithMessage("La contraseña debe incluir al menos una letra.")
            .Must(laClave => laClave.Any(char.IsDigit)).WithMessage("La contraseña debe incluir al menos un número.");
}

public class RegistroRequestValidator : AbstractValidator<RegistroRequest>
{
    public RegistroRequestValidator()
    {
        RuleFor(x => x.Nombre).NotEmpty().WithMessage("El nombre es obligatorio.")
            .MaximumLength(60).WithMessage("El nombre no puede superar 60 caracteres.");
        RuleFor(x => x.Apellidos).NotEmpty().WithMessage("Los apellidos son obligatorios.")
            .MaximumLength(80).WithMessage("Los apellidos no pueden superar 80 caracteres.");
        RuleFor(x => x.Correo).NotEmpty().WithMessage("El correo es obligatorio.")
            .MaximumLength(120).WithMessage("El correo no puede superar 120 caracteres.")
            .EmailAddress().WithMessage("El correo no es válido.");
        RuleFor(x => x.Password).ExijaUnaContrasenaSegura();
        RuleFor(x => x.ConfirmacionPassword).Equal(x => x.Password)
            .WithMessage("La confirmación no coincide con la contraseña.");
    }
}

public class LoginRequestValidator : AbstractValidator<LoginRequest>
{
    public LoginRequestValidator()
    {
        RuleFor(x => x.Correo).NotEmpty().WithMessage("El correo es obligatorio.");
        RuleFor(x => x.Password).NotEmpty().WithMessage("La contraseña es obligatoria.");
    }
}

public class ActualizarPerfilRequestValidator : AbstractValidator<ActualizarPerfilRequest>
{
    public ActualizarPerfilRequestValidator()
    {
        RuleFor(x => x.Nombre).NotEmpty().WithMessage("El nombre es obligatorio.")
            .MaximumLength(60).WithMessage("El nombre no puede superar 60 caracteres.");
        RuleFor(x => x.Apellidos).NotEmpty().WithMessage("Los apellidos son obligatorios.")
            .MaximumLength(80).WithMessage("Los apellidos no pueden superar 80 caracteres.");
        RuleFor(x => x.Descripcion).MaximumLength(300).WithMessage("La descripción no puede superar 300 caracteres.");
    }
}
