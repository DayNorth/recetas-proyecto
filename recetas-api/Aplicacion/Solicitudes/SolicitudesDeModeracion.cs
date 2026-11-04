using FluentValidation;
using Recetas.Api.Dominio;

namespace Recetas.Api.Aplicacion.Solicitudes;

public record CrearReporteContenidoRequest(int? RecetaId, int? ComentarioId, string Motivo, string? Descripcion);
public record ResolverReporteRequest(string Decision, string Resolucion);
public record MotivoRequest(string Motivo);

public record FiltroDelHistorial(int? RecetaId, int? ComentarioId, DateOnly? Desde, DateOnly? Hasta);

public class CrearReporteContenidoRequestValidator : AbstractValidator<CrearReporteContenidoRequest>
{
    public CrearReporteContenidoRequestValidator()
    {
        RuleFor(x => x).Must(x => (x.RecetaId is not null) ^ (x.ComentarioId is not null))
            .WithName("Objetivo").WithMessage("Indique exactamente una receta o un comentario.");
        RuleFor(x => x.Motivo).NotEmpty().WithMessage("El motivo es obligatorio.")
            .Must(elMotivo => elMotivo is not null && MotivosDeDenuncia.VALIDOS.Contains(elMotivo.Trim().ToUpperInvariant()))
            .WithMessage("Motivo inválido. Use: " + string.Join(", ", MotivosDeDenuncia.VALIDOS) + ".");
        RuleFor(x => x.Descripcion).MaximumLength(500).WithMessage("La descripción no puede superar 500 caracteres.");
    }
}

public class ResolverReporteRequestValidator : AbstractValidator<ResolverReporteRequest>
{
    public ResolverReporteRequestValidator()
    {
        RuleFor(x => x.Decision)
            .Must(laDecision => laDecision is not null
                && laDecision.ToUpperInvariant() is DecisionesDeModeracion.MANTENER or DecisionesDeModeracion.MODERAR)
            .WithMessage("La decisión debe ser MANTENER o MODERAR.");
        RuleFor(x => x.Resolucion).NotEmpty().WithMessage("La resolución es obligatoria.")
            .Must(laResolucion => laResolucion is null || laResolucion.Trim().Length >= 10).WithMessage("La resolución debe tener al menos 10 caracteres.")
            .MaximumLength(500).WithMessage("La resolución no puede superar 500 caracteres.");
    }
}

public class MotivoRequestValidator : AbstractValidator<MotivoRequest>
{
    public MotivoRequestValidator()
    {
        RuleFor(x => x.Motivo).NotEmpty().WithMessage("El motivo es obligatorio.")
            .MaximumLength(500).WithMessage("El motivo no puede superar 500 caracteres.");
    }
}
