using NSubstitute;
using Recetas.Api.Aplicacion.Dtos;
using Recetas.Api.Aplicacion.Puertos;
using Recetas.Api.Dominio;

namespace Recetas.Api.Tests.Comun;

public class EjecutorSinBaseDeDatos : IEjecutorDeTransacciones
{
    public int Ejecuciones { get; private set; }

    public Task<T> EjecuteEnTransaccion<T>(Func<CancellationToken, Task<T>> laAccion, CancellationToken cancelacion = default)
    {
        Ejecuciones++;
        return laAccion(cancelacion);
    }

    public async Task EjecuteEnTransaccion(Func<CancellationToken, Task> laAccion, CancellationToken cancelacion = default)
    {
        Ejecuciones++;
        await laAccion(cancelacion);
    }
}

public class FallaEnElPunto(string elPunto) : IPuntoDeFalla
{
    public List<string> PuntosVisitados { get; } = [];

    public void PaseElPuntoDeFalla(string elPuntoActual)
    {
        PuntosVisitados.Add(elPuntoActual);
        if (elPuntoActual == elPunto) throw new InvalidOperationException("falla forzada en " + elPuntoActual);
    }
}

public class RelojFijo(DateTimeOffset laHora) : TimeProvider
{
    public override DateTimeOffset GetUtcNow() => laHora;
}

public static class Dobles
{
    public static readonly CancellationToken CT = CancellationToken.None;

    public static IUsuarioActual Usuario(int? elId, bool esAdministrador = false)
    {
        var elUsuario = Substitute.For<IUsuarioActual>();
        elUsuario.Id.Returns(elId);
        elUsuario.EstaAutenticado.Returns(elId is not null);
        elUsuario.EsAdministrador.Returns(esAdministrador);
        elUsuario.IdRequerido.Returns(elId ?? 0);
        return elUsuario;
    }

    public static UsuarioDto UnUsuarioDto(int elId = 1, string elRol = Roles.USUARIO) =>
        new(elId, "Ana", "Pérez", "ana@correo.com", elRol, null, null, new DateTime(2026, 1, 1));

    public static RecetaBasica UnaReceta(int elId = 10, int elAutorId = 2, string elEstado = EstadosDeLaReceta.PUBLICADA,
        short laCategoriaId = 1) => new(elId, "Arroz con pollo", elAutorId, elEstado, laCategoriaId, 4);

    public static async Task<ExcepcionDeNegocio> FalleCon(TipoDeFalla elTipo, Func<Task> laAccion)
    {
        var laExcepcion = await Assert.ThrowsAsync<ExcepcionDeNegocio>(laAccion);
        Assert.Equal(elTipo, laExcepcion.Tipo);
        return laExcepcion;
    }

    public static ArchivoSubido UnArchivo(long laLongitud = 10) => new(laLongitud, () => new MemoryStream([1, 2, 3]));
}
