using Microsoft.EntityFrameworkCore;
using Recetas.Api.Aplicacion.Comun;

namespace Recetas.Api.Infraestructura.Persistencia;

public static class ExtensionesDeConsulta
{
    public static async Task<Paginado<T>> PagineLaConsulta<T>(this IQueryable<T> laConsulta, SolicitudDePagina laPagina,
        CancellationToken cancelacion)
    {
        var elTotal = await laConsulta.CountAsync(cancelacion);
        var losItems = await laConsulta.Skip(laPagina.CuantosSaltar).Take(laPagina.Tamano).ToListAsync(cancelacion);
        return new Paginado<T>(losItems, laPagina.Pagina, laPagina.Tamano, elTotal);
    }

    public static string ConstruyaElPatronDeContiene(string elTexto) =>
        "%" + elTexto.Replace("\\", "\\\\").Replace("%", "\\%").Replace("_", "\\_") + "%";
}
