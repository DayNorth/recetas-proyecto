using Recetas.Api.Aplicacion.Dtos;
using Recetas.Api.Aplicacion.Puertos;
using Recetas.Api.Aplicacion.Solicitudes;
using Recetas.Api.Dominio;

namespace Recetas.Api.Aplicacion.Servicios;

public class ServicioDeAutenticacion(IRepositorioDeUsuarios losUsuarios, IServicioDeTokens losTokens,
    IRevocadorDeTokens losTokensRevocados, IHasheadorDeContrasenas elHasheador, IUsuarioActual elUsuarioActual,
    TimeProvider elReloj)
{
    private static readonly TimeSpan VIGENCIA_POR_DEFECTO_DEL_TOKEN = TimeSpan.FromHours(2);

    public async Task<UsuarioDto> RegistreAlUsuario(RegistroRequest laSolicitud, CancellationToken cancelacion)
    {
        var elCorreo = NormaliceElCorreo(laSolicitud.Correo);
        if (await losUsuarios.ExisteElCorreo(elCorreo, cancelacion))
            throw ExcepcionDeNegocio.PorConflicto("Ya existe una cuenta con ese correo.");

        return await losUsuarios.AgregueAlUsuario(laSolicitud.Nombre.Trim(), laSolicitud.Apellidos.Trim(), elCorreo,
            elHasheador.Hashee(laSolicitud.Password), cancelacion);
    }

    public async Task<LoginResponse> InicieLaSesion(LoginRequest laSolicitud, CancellationToken cancelacion)
    {
        var laCuenta = await losUsuarios.BusqueLaCuentaPorCorreo(NormaliceElCorreo(laSolicitud.Correo), cancelacion);

        var elHash = laCuenta?.PasswordHash ?? elHasheador.HashDeRelleno;
        var laClaveEsCorrecta = elHasheador.Verifique(laSolicitud.Password, elHash);
        if (laCuenta is null || !laClaveEsCorrecta || !laCuenta.Activo)
            throw ExcepcionDeNegocio.PorFaltaDeAutenticacion();

        await losUsuarios.RegistreElUltimoAcceso(laCuenta.Usuario.UsuarioId, elReloj.GetUtcNow().UtcDateTime, cancelacion);

        var elToken = losTokens.Cree(laCuenta.Usuario.UsuarioId, laCuenta.Usuario.Rol);
        return new LoginResponse(elToken.Token, elToken.Expira, laCuenta.Usuario);
    }

    public async Task<UsuarioDto> ObtengaAlUsuarioActual(CancellationToken cancelacion) =>
        await losUsuarios.BusqueAlUsuario(elUsuarioActual.IdRequerido, cancelacion)
        ?? throw ExcepcionDeNegocio.PorFaltaDeAutenticacion("Sesión no válida.");

    public void CierreLaSesion()
    {
        var elIdentificador = elUsuarioActual.IdentificadorDelToken;
        var laExpiracion = elUsuarioActual.ExpiracionDelToken ?? elReloj.GetUtcNow().Add(VIGENCIA_POR_DEFECTO_DEL_TOKEN);
        if (elIdentificador is not null) losTokensRevocados.Revoque(elIdentificador, laExpiracion);
    }

    private static string NormaliceElCorreo(string elCorreo) => elCorreo.Trim().ToLowerInvariant();
}
