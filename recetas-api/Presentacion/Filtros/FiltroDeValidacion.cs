using FluentValidation;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;

namespace Recetas.Api.Presentacion.Filtros;

public class FiltroDeValidacion(IServiceProvider losServicios) : IAsyncActionFilter
{
    public async Task OnActionExecutionAsync(ActionExecutingContext elContexto, ActionExecutionDelegate elSiguiente)
    {
        foreach (var elArgumento in elContexto.ActionArguments.Values)
        {
            if (elArgumento is null) continue;
            var elTipoDelValidador = typeof(IValidator<>).MakeGenericType(elArgumento.GetType());
            if (losServicios.GetService(elTipoDelValidador) is not IValidator elValidador) continue;

            var elResultado = await elValidador.ValidateAsync(
                new ValidationContext<object>(elArgumento), elContexto.HttpContext.RequestAborted);
            foreach (var elError in elResultado.Errors)
                elContexto.ModelState.AddModelError(elError.PropertyName, elError.ErrorMessage);
        }

        if (!elContexto.ModelState.IsValid)
        {
            elContexto.Result = new BadRequestObjectResult(
                new ValidationProblemDetails(elContexto.ModelState) { Status = 400, Title = "Hay errores de validación." });
            return;
        }
        await elSiguiente();
    }
}
