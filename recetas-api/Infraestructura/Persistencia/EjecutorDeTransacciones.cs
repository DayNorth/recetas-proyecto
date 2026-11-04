using Microsoft.EntityFrameworkCore;
using Recetas.Api.Aplicacion.Puertos;

namespace Recetas.Api.Infraestructura.Persistencia;

public class EjecutorDeTransacciones(RecetasDbContext laBaseDeDatos) : IEjecutorDeTransacciones
{
    public async Task<T> EjecuteEnTransaccion<T>(Func<CancellationToken, Task<T>> laAccion, CancellationToken cancelacion = default)
    {
        var laEstrategia = laBaseDeDatos.Database.CreateExecutionStrategy();
        return await laEstrategia.ExecuteAsync(async () =>
        {
            laBaseDeDatos.ChangeTracker.Clear();
            await using var laTransaccion = await laBaseDeDatos.Database.BeginTransactionAsync(cancelacion);
            try
            {
                var elResultado = await laAccion(cancelacion);
                await laTransaccion.CommitAsync(cancelacion);
                return elResultado;
            }
            catch
            {
                await laTransaccion.RollbackAsync(CancellationToken.None);
                throw;
            }
        });
    }

    public async Task EjecuteEnTransaccion(Func<CancellationToken, Task> laAccion, CancellationToken cancelacion = default) =>
        await EjecuteEnTransaccion<int>(async laCancelacion =>
        {
            await laAccion(laCancelacion);
            return 0;
        }, cancelacion);
}
