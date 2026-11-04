using Microsoft.AspNetCore.Diagnostics;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Npgsql;
using Recetas.Api.Dominio;

namespace Recetas.Api.Presentacion.Middleware;

public class ManejadorGlobalDeExcepciones(IProblemDetailsService losProblemas, ILogger<ManejadorGlobalDeExcepciones> elRegistro)
    : IExceptionHandler
{
    private const string TITULO_DEL_ERROR_INESPERADO = "Ocurrió un error inesperado.";

    public async ValueTask<bool> TryHandleAsync(HttpContext elContexto, Exception laExcepcion, CancellationToken cancelacion)
    {
        if (laExcepcion is FluentValidation.ValidationException laFallaDeValidacion)
        {
            var losErrores = laFallaDeValidacion.Errors.GroupBy(error => error.PropertyName)
                .ToDictionary(grupo => grupo.Key, grupo => grupo.Select(error => error.ErrorMessage).ToArray());
            elContexto.Response.StatusCode = StatusCodes.Status400BadRequest;
            return await losProblemas.TryWriteAsync(new ProblemDetailsContext
            {
                HttpContext = elContexto,
                ProblemDetails = new ValidationProblemDetails(losErrores) { Status = 400, Title = "Hay errores de validación." }
            });
        }

        var (elEstado, elTitulo, elDetalle) = TraduzcaLaExcepcion(laExcepcion);
        if (elEstado >= 500) elRegistro.LogError(laExcepcion, "Error no controlado en {Path}", elContexto.Request.Path);
        else elRegistro.LogInformation("{Status} en {Path}: {Mensaje}", elEstado, elContexto.Request.Path, laExcepcion.Message);

        elContexto.Response.StatusCode = elEstado;
        return await losProblemas.TryWriteAsync(new ProblemDetailsContext
        {
            HttpContext = elContexto,
            ProblemDetails = new ProblemDetails { Status = elEstado, Title = elTitulo, Detail = elDetalle }
        });
    }

    private static (int Estado, string Titulo, string? Detalle) TraduzcaLaExcepcion(Exception laExcepcion)
    {
        if (laExcepcion is ExcepcionDeNegocio laExcepcionDeNegocio)
            return (ObtengaElEstadoHttp(laExcepcionDeNegocio.Tipo), laExcepcionDeNegocio.Titulo, laExcepcionDeNegocio.Detalle);
        if (laExcepcion is BadHttpRequestException) return (400, "Petición mal formada.", null);

        var laFallaDePostgres = laExcepcion as PostgresException
                                ?? (laExcepcion as DbUpdateException)?.InnerException as PostgresException
                                ?? laExcepcion.InnerException as PostgresException;
        if (laFallaDePostgres is null) return (500, TITULO_DEL_ERROR_INESPERADO, null);

        return laFallaDePostgres.SqlState switch
        {
            PostgresErrorCodes.UniqueViolation => (409, "Ya existe un registro con esos datos.", null),
            PostgresErrorCodes.ForeignKeyViolation => (409, "La operación referencia o afecta datos relacionados.", null),
            PostgresErrorCodes.CheckViolation => (422, "Un dato no cumple las reglas de la base de datos.", null),
            PostgresErrorCodes.SerializationFailure or PostgresErrorCodes.DeadlockDetected =>
                (409, "Conflicto de concurrencia; intente de nuevo.", null),
            _ => (500, TITULO_DEL_ERROR_INESPERADO, null)
        };
    }

    private static int ObtengaElEstadoHttp(TipoDeFalla elTipo) => elTipo switch
    {
        TipoDeFalla.SolicitudInvalida => StatusCodes.Status400BadRequest,
        TipoDeFalla.RecursoInexistente => StatusCodes.Status404NotFound,
        TipoDeFalla.Conflicto => StatusCodes.Status409Conflict,
        TipoDeFalla.AccesoProhibido => StatusCodes.Status403Forbidden,
        TipoDeFalla.FaltaDeAutenticacion => StatusCodes.Status401Unauthorized,
        TipoDeFalla.ServicioNoDisponible => StatusCodes.Status503ServiceUnavailable,
        _ => StatusCodes.Status500InternalServerError
    };
}
