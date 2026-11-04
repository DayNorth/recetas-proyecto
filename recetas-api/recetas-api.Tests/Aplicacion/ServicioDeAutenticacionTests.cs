using NSubstitute;
using Recetas.Api.Aplicacion.Dtos;
using Recetas.Api.Aplicacion.Puertos;
using Recetas.Api.Aplicacion.Servicios;
using Recetas.Api.Aplicacion.Solicitudes;
using Recetas.Api.Dominio;
using Recetas.Api.Tests.Comun;
using static Recetas.Api.Tests.Comun.Dobles;

namespace Recetas.Api.Tests.Aplicacion;

public class ServicioDeAutenticacionTests
{
    private readonly IRepositorioDeUsuarios _usuarios = Substitute.For<IRepositorioDeUsuarios>();
    private readonly IServicioDeTokens _tokens = Substitute.For<IServicioDeTokens>();
    private readonly IRevocadorDeTokens _revocador = Substitute.For<IRevocadorDeTokens>();
    private readonly IHasheadorDeContrasenas _hasheador = Substitute.For<IHasheadorDeContrasenas>();
    private readonly IUsuarioActual _actual = Usuario(1);
    private readonly DateTimeOffset _ahora = new(2026, 11, 3, 12, 0, 0, TimeSpan.Zero);

    private ServicioDeAutenticacion CreeElServicio() => new(_usuarios, _tokens, _revocador, _hasheador, _actual, new RelojFijo(_ahora));

    public ServicioDeAutenticacionTests()
    {
        _hasheador.HashDeRelleno.Returns("hash-de-relleno");
        _hasheador.Hashee(Arg.Any<string>()).Returns("hash-nuevo");
    }

    [Fact]
    public async Task Registro_con_un_correo_existente_da_conflicto_y_no_crea_la_cuenta()
    {
        _usuarios.ExisteElCorreo("ana@correo.com", CT).Returns(true);

        await FalleCon(TipoDeFalla.Conflicto, () =>
            CreeElServicio().RegistreAlUsuario(new RegistroRequest("Ana", "Pérez", "ana@correo.com", "clave1234", "clave1234"), CT));

        await _usuarios.DidNotReceiveWithAnyArgs().AgregueAlUsuario(default!, default!, default!, default!, default);
    }

    [Fact]
    public async Task Registro_normaliza_el_correo_recorta_los_nombres_y_guarda_el_hash()
    {
        _usuarios.AgregueAlUsuario("Ana", "Pérez", "ana@correo.com", "hash-nuevo", CT).Returns(UnUsuarioDto());

        var elUsuario = await CreeElServicio().RegistreAlUsuario(
            new RegistroRequest("  Ana ", " Pérez  ", "  ANA@Correo.com ", "clave1234", "clave1234"), CT);

        Assert.Equal("ana@correo.com", elUsuario.Correo);
        await _usuarios.Received(1).AgregueAlUsuario("Ana", "Pérez", "ana@correo.com", "hash-nuevo", CT);
    }

    [Fact]
    public async Task Login_con_un_correo_inexistente_verifica_contra_el_hash_de_relleno_y_da_401()
    {
        _usuarios.BusqueLaCuentaPorCorreo("nadie@correo.com", CT).Returns((CuentaDeUsuario?)null);
        _hasheador.Verifique("clave1234", "hash-de-relleno").Returns(false);

        await FalleCon(TipoDeFalla.FaltaDeAutenticacion, () =>
            CreeElServicio().InicieLaSesion(new LoginRequest("nadie@correo.com", "clave1234"), CT));

        _hasheador.Received(1).Verifique("clave1234", "hash-de-relleno");
    }

    [Fact]
    public async Task Login_con_clave_erronea_da_401_y_no_registra_acceso()
    {
        _usuarios.BusqueLaCuentaPorCorreo("ana@correo.com", CT).Returns(new CuentaDeUsuario(UnUsuarioDto(), "hash-real", true));
        _hasheador.Verifique("mala", "hash-real").Returns(false);

        await FalleCon(TipoDeFalla.FaltaDeAutenticacion, () =>
            CreeElServicio().InicieLaSesion(new LoginRequest("ana@correo.com", "mala"), CT));

        await _usuarios.DidNotReceiveWithAnyArgs().RegistreElUltimoAcceso(default, default, default);
    }

    [Fact]
    public async Task Login_de_una_cuenta_suspendida_da_el_mismo_401_aunque_la_clave_sea_correcta()
    {
        _usuarios.BusqueLaCuentaPorCorreo("ana@correo.com", CT).Returns(new CuentaDeUsuario(UnUsuarioDto(), "hash-real", false));
        _hasheador.Verifique("clave1234", "hash-real").Returns(true);

        var laExcepcion = await FalleCon(TipoDeFalla.FaltaDeAutenticacion, () =>
            CreeElServicio().InicieLaSesion(new LoginRequest("ana@correo.com", "clave1234"), CT));

        Assert.Equal("Credenciales inválidas.", laExcepcion.Titulo);
    }

    [Fact]
    public async Task Login_correcto_registra_el_acceso_y_devuelve_el_token_con_el_rol()
    {
        var elUsuario = UnUsuarioDto(7, Roles.ADMIN);
        _usuarios.BusqueLaCuentaPorCorreo("ana@correo.com", CT).Returns(new CuentaDeUsuario(elUsuario, "hash-real", true));
        _hasheador.Verifique("clave1234", "hash-real").Returns(true);
        _tokens.Cree(7, Roles.ADMIN).Returns(new TokenEmitido("token-jwt", _ahora.AddHours(2)));

        var laRespuesta = await CreeElServicio().InicieLaSesion(new LoginRequest(" Ana@Correo.com ", "clave1234"), CT);

        Assert.Equal("token-jwt", laRespuesta.Token);
        Assert.Equal(elUsuario, laRespuesta.Usuario);
        await _usuarios.Received(1).RegistreElUltimoAcceso(7, _ahora.UtcDateTime, CT);
    }

    [Fact]
    public async Task Usuario_actual_de_una_sesion_cuya_cuenta_ya_no_existe_da_401()
    {
        _usuarios.BusqueAlUsuario(1, CT).Returns((UsuarioDto?)null);

        await FalleCon(TipoDeFalla.FaltaDeAutenticacion, () => CreeElServicio().ObtengaAlUsuarioActual(CT));
    }

    [Fact]
    public void Cierre_de_sesion_revoca_el_token_hasta_su_vencimiento()
    {
        var elVencimiento = _ahora.AddHours(1);
        _actual.IdentificadorDelToken.Returns("jti-1");
        _actual.ExpiracionDelToken.Returns(elVencimiento);

        CreeElServicio().CierreLaSesion();

        _revocador.Received(1).Revoque("jti-1", elVencimiento);
    }

    [Fact]
    public void Cierre_de_sesion_sin_vencimiento_conocido_usa_dos_horas_y_sin_jti_no_revoca_nada()
    {
        _actual.IdentificadorDelToken.Returns("jti-2");
        _actual.ExpiracionDelToken.Returns((DateTimeOffset?)null);
        CreeElServicio().CierreLaSesion();
        _revocador.Received(1).Revoque("jti-2", _ahora.AddHours(2));

        _revocador.ClearReceivedCalls();
        _actual.IdentificadorDelToken.Returns((string?)null);
        CreeElServicio().CierreLaSesion();
        _revocador.DidNotReceiveWithAnyArgs().Revoque(default!, default);
    }
}
