using FluentValidation;

namespace Recetas.Api.Aplicacion.Solicitudes;

public record CalificarRequest(int? Calificacion, string? Comentario);
public record CompartirRequest(short RedId);

/// <summary>Señal de interés de un visitante (con o sin cuenta); alimenta la conversión del tablero.</summary>
public record InteraccionRequest(string Tipo)
{
    public static readonly string[] TIPOS_VALIDOS = ["IMPRIMIR", "PORCIONES", "DOCUMENTO", "LECTURA"];
}
public record CrearListaRequest(int RecetaId, int Porciones, string? Nombre);
public record MarcarItemRequest(bool Comprado);

public class CalificarRequestValidator : AbstractValidator<CalificarRequest>
{
    public CalificarRequestValidator()
    {
        RuleFor(x => x.Calificacion).InclusiveBetween(1, 5).When(x => x.Calificacion is not null)
            .WithMessage("La calificación debe estar entre 1 y 5.");
        RuleFor(x => x.Comentario).MaximumLength(500).WithMessage("El comentario no puede superar 500 caracteres.");
        RuleFor(x => x).Must(x => x.Calificacion is not null || !string.IsNullOrWhiteSpace(x.Comentario))
            .WithName("Calificacion").WithMessage("Debe enviar una calificación, un comentario o ambos.");
    }
}

public class CrearListaRequestValidator : AbstractValidator<CrearListaRequest>
{
    public CrearListaRequestValidator()
    {
        RuleFor(x => x.RecetaId).GreaterThan(0).WithMessage("La receta es obligatoria.");
        RuleFor(x => x.Porciones).InclusiveBetween(1, 100).WithMessage("Las porciones deben estar entre 1 y 100.");
        RuleFor(x => x.Nombre).MaximumLength(80).WithMessage("El nombre no puede superar 80 caracteres.");
    }
}
