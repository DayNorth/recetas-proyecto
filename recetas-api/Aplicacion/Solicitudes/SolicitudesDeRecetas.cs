using FluentValidation;
using Recetas.Api.Dominio;

namespace Recetas.Api.Aplicacion.Solicitudes;

public record IngredienteEntrada(int? IngredienteId, string? Nombre, short UnidadId, decimal Cantidad, string? Nota);

public record CrearRecetaRequest(string Titulo, string? Descripcion, short CategoriaId, short DificultadId,
    int TiempoPrepMin, int PorcionesBase, List<IngredienteEntrada> Ingredientes, List<string> Pasos);

public record FiltroDeRecetas(string? Texto, short? CategoriaId, short? DificultadId, int? TiempoMaximo,
    IReadOnlyList<int> Ingredientes, string Orden);

public class CrearRecetaRequestValidator : AbstractValidator<CrearRecetaRequest>
{
    public CrearRecetaRequestValidator()
    {
        RuleFor(x => x.Titulo).NotEmpty().WithMessage("El título es obligatorio.")
            .MaximumLength(120).WithMessage("El título no puede superar 120 caracteres.");
        RuleFor(x => x.Descripcion).MaximumLength(2000).WithMessage("La descripción no puede superar 2000 caracteres.");
        RuleFor(x => x.CategoriaId).GreaterThan((short)0).WithMessage("La categoría es obligatoria.");
        RuleFor(x => x.DificultadId).GreaterThan((short)0).WithMessage("La dificultad es obligatoria.");
        RuleFor(x => x.TiempoPrepMin).InclusiveBetween(1, 32767).WithMessage("El tiempo de preparación debe ser mayor que 0.");
        RuleFor(x => x.PorcionesBase).InclusiveBetween(1, 1000).WithMessage("Las porciones deben estar entre 1 y 1000.");

        RuleFor(x => x.Ingredientes).NotEmpty().WithMessage("Debe incluir al menos un ingrediente.");
        RuleForEach(x => x.Ingredientes).ChildRules(ingrediente =>
        {
            ingrediente.RuleFor(entrada => entrada)
                .Must(entrada => (entrada.IngredienteId is not null) ^ !string.IsNullOrWhiteSpace(entrada.Nombre))
                .WithMessage("Cada ingrediente debe indicar ingredienteId o nombre (uno solo).");
            ingrediente.RuleFor(entrada => entrada.Nombre).MaximumLength(80).WithMessage("El nombre del ingrediente no puede superar 80 caracteres.");
            ingrediente.RuleFor(entrada => entrada.UnidadId).GreaterThan((short)0).WithMessage("La unidad es obligatoria.");
            ingrediente.RuleFor(entrada => entrada.Cantidad).GreaterThan(0).WithMessage("La cantidad debe ser mayor que 0.")
                .LessThanOrEqualTo(999999.99m).WithMessage("La cantidad es demasiado grande.");
            ingrediente.RuleFor(entrada => entrada.Nota).MaximumLength(100).WithMessage("La nota no puede superar 100 caracteres.");
        });
        RuleFor(x => x.Ingredientes).Must(laLista =>
        {
            var lasClaves = laLista.Where(entrada => entrada is not null).Select(entrada => entrada.IngredienteId is not null
                ? "id:" + entrada.IngredienteId
                : "n:" + Texto.CalculeLaClaveDelNombre(entrada.Nombre ?? ""));
            return lasClaves.Distinct().Count() == lasClaves.Count();
        }).WithMessage("Hay ingredientes repetidos.");

        RuleFor(x => x.Pasos).NotEmpty().WithMessage("Debe incluir al menos un paso.");
        RuleForEach(x => x.Pasos).NotEmpty().WithMessage("Un paso no puede estar vacío.")
            .MaximumLength(2000).WithMessage("Un paso no puede superar 2000 caracteres.");
    }
}
