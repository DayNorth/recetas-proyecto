using Microsoft.EntityFrameworkCore;
using Recetas.Api.Aplicacion.Puertos;
using Recetas.Api.Infraestructura.Persistencia;

namespace Recetas.Api.Infraestructura.Repositorios;

public class VerificadorDeSaludPostgres(RecetasDbContext laBaseDeDatos) : IVerificadorDeSalud
{
    public async Task<InformeDeSalud> Verifique(CancellationToken cancelacion)
    {
        var laVersion = await laBaseDeDatos.Database.SqlQuery<string>($"SELECT version() AS \"Value\"").SingleAsync(cancelacion);
        var lasTablas = await laBaseDeDatos.Database
            .SqlQuery<int>($"SELECT count(*)::int AS \"Value\" FROM pg_tables WHERE schemaname = 'public'")
            .SingleAsync(cancelacion);
        var lasRecetas = await laBaseDeDatos.Recetas.CountAsync(cancelacion);
        var losUsuarios = await laBaseDeDatos.Usuarios.CountAsync(cancelacion);

        var lasMasVistas = await laBaseDeDatos.Database
            .SqlQuery<int>($"SELECT receta_id AS \"Value\" FROM sp_reporte_mas_vistas({new DateOnly(2026, 5, 1)}, {new DateOnly(2026, 10, 31)}, {1})")
            .ToListAsync(cancelacion);

        return new InformeDeSalud(laVersion, lasTablas, lasRecetas, losUsuarios, lasMasVistas.FirstOrDefault());
    }
}
