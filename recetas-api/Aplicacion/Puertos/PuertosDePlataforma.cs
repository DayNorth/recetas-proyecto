namespace Recetas.Api.Aplicacion.Puertos;

public interface IUsuarioActual
{
    bool EstaAutenticado { get; }
    int? Id { get; }
    string? Rol { get; }
    bool EsAdministrador { get; }

    int IdRequerido { get; }

    string? IdentificadorDelToken { get; }

    DateTimeOffset? ExpiracionDelToken { get; }
}

public interface IEjecutorDeTransacciones
{
    Task<T> EjecuteEnTransaccion<T>(Func<CancellationToken, Task<T>> laAccion, CancellationToken cancelacion = default);

    Task EjecuteEnTransaccion(Func<CancellationToken, Task> laAccion, CancellationToken cancelacion = default);
}

public interface IPuntoDeFalla
{
    void PaseElPuntoDeFalla(string elPunto);
}

public record ArchivoSubido(long Longitud, Func<Stream> AbraElFlujo);

public interface IAlmacenDeArchivos
{
    Task<string> GuardeElArchivo(Stream elContenido, string laCarpeta, bool permitirImagenes, bool permitirPdf,
        long maximoDeBytes, CancellationToken cancelacion = default);

    void BorreElArchivo(string? laRutaRelativa);

    string RutaRaiz { get; }
}

public record TokenEmitido(string Token, DateTimeOffset Expira);

public interface IServicioDeTokens
{
    TokenEmitido Cree(int elUsuarioId, string elRol);
}

public interface IRevocadorDeTokens
{
    void Revoque(string elIdentificador, DateTimeOffset laExpiracion);

    bool EstaRevocado(string? elIdentificador);
}

public interface IHasheadorDeContrasenas
{
    string Hashee(string laClave);

    bool Verifique(string laClave, string elHash);

    string HashDeRelleno { get; }
}

public interface IVerificadorDeSalud
{
    Task<InformeDeSalud> Verifique(CancellationToken cancelacion);
}

public record InformeDeSalud(string Version, int Tablas, int Recetas, int Usuarios, int RecetaMasVistaEnElRango);

public record OpcionesDeEnlaces(string UrlPublicaDeLaSpa);
