using NSubstitute;
using Recetas.Api.Aplicacion.Dtos;
using Recetas.Api.Aplicacion.Puertos;
using Recetas.Api.Aplicacion.Servicios;
using Recetas.Api.Aplicacion.Solicitudes;
using Recetas.Api.Dominio;
using Recetas.Api.Tests.Comun;
using static Recetas.Api.Tests.Comun.Dobles;

namespace Recetas.Api.Tests.Aplicacion;

public class ServicioDeAdministracionTests
{
    private readonly IRepositorioDeCatalogos _catalogos = Substitute.For<IRepositorioDeCatalogos>();
    private readonly IRepositorioDeUsuarios _usuarios = Substitute.For<IRepositorioDeUsuarios>();
    private readonly EjecutorSinBaseDeDatos _ejecutor = new();

    private ServicioDeAdministracion CreeElServicio(int elAdminId = 1) => new(_catalogos, _usuarios, Usuario(elAdminId, true), _ejecutor);

    private static CategoriaAdminDto UnaCategoria(short elId = 5) => new(elId, "Postres", null, true, 0);

    private static UsuarioAdminDto UnUsuarioAdmin(int elId) => new(elId, "Ana", "Pérez", "a@b.com", Roles.ADMIN, true, DateTime.UtcNow, null);

    [Fact]
    public async Task Crear_una_categoria_con_nombre_repetido_da_conflicto_sin_insertar()
    {
        _catalogos.ExisteOtraCategoriaConElNombre("Postres", null, CT).Returns(true);

        await FalleCon(TipoDeFalla.Conflicto, () => CreeElServicio().CreeLaCategoria(new CategoriaRequest("Postres", null), CT));

        await _catalogos.DidNotReceiveWithAnyArgs().AgregueLaCategoria(default!, default, default);
    }

    [Fact]
    public async Task Crear_una_categoria_normaliza_los_espacios_y_descarta_la_descripcion_vacia()
    {
        _catalogos.AgregueLaCategoria("Platos fuertes", null, CT).Returns((short)5);
        _catalogos.BusqueLaCategoriaParaAdministracion(5, CT).Returns(UnaCategoria());

        var laCategoria = await CreeElServicio().CreeLaCategoria(new CategoriaRequest("  Platos   fuertes ", "   "), CT);

        Assert.Equal(5, laCategoria.CategoriaId);
        await _catalogos.Received(1).AgregueLaCategoria("Platos fuertes", null, CT);
    }

    [Fact]
    public async Task Editar_una_categoria_inexistente_da_404_antes_de_validar_el_nombre()
    {
        _catalogos.BusqueLaCategoriaParaAdministracion(9, CT).Returns((CategoriaAdminDto?)null);

        await FalleCon(TipoDeFalla.RecursoInexistente, () => CreeElServicio().EditeLaCategoria(9, new CategoriaRequest("X", null), CT));

        await _catalogos.DidNotReceiveWithAnyArgs().ExisteOtraCategoriaConElNombre(default!, default, default);
    }

    [Fact]
    public async Task Editar_una_categoria_permite_conservar_su_propio_nombre()
    {
        _catalogos.BusqueLaCategoriaParaAdministracion(5, CT).Returns(UnaCategoria());
        _catalogos.ExisteOtraCategoriaConElNombre("Postres", 5, CT).Returns(false);

        await CreeElServicio().EditeLaCategoria(5, new CategoriaRequest("Postres", "dulces"), CT);

        await _catalogos.Received(1).ActualiceLaCategoria(5, "Postres", "dulces", CT);
    }

    [Fact]
    public async Task Cambiar_el_estado_de_una_categoria_inexistente_da_404()
    {
        _catalogos.CambieElEstadoDeLaCategoria(9, false, CT).Returns(false);
        await FalleCon(TipoDeFalla.RecursoInexistente, () => CreeElServicio().CambieElEstadoDeLaCategoria(9, false, CT));
    }

    [Theory]
    [InlineData("activo", true)]
    [InlineData(" SUSPENDIDO ", false)]
    public async Task Listar_usuarios_traduce_el_filtro_de_estado(string elEstado, bool seEsperaActivo)
    {
        await CreeElServicio().ListeLosUsuarios("  ana ", elEstado, "admin", 2, 5, CT);

        await _usuarios.Received(1).ListeLosUsuarios(new FiltroDeUsuarios("ana", seEsperaActivo, Roles.ADMIN),
            new Recetas.Api.Aplicacion.Comun.SolicitudDePagina(2, 5), CT);
    }

    [Fact]
    public async Task Listar_usuarios_con_estado_o_rol_invalido_da_400()
    {
        await FalleCon(TipoDeFalla.SolicitudInvalida, () => CreeElServicio().ListeLosUsuarios(null, "borracho", null, null, null, CT));
        await FalleCon(TipoDeFalla.SolicitudInvalida, () => CreeElServicio().ListeLosUsuarios(null, null, "SUPERUSUARIO", null, null, CT));
    }

    [Fact]
    public async Task Un_administrador_no_puede_suspender_su_propia_cuenta()
    {
        await FalleCon(TipoDeFalla.Conflicto, () => CreeElServicio(elAdminId: 1).CambieElEstadoDelUsuario(1, false, CT));

        Assert.Equal(0, _ejecutor.Ejecuciones);
    }

    [Fact]
    public async Task No_se_puede_suspender_al_ultimo_administrador_activo()
    {
        _usuarios.BusqueElEstadoDeLaCuenta(2, CT).Returns(new EstadoDeCuenta(true, Roles.ADMIN));
        _usuarios.CuenteLosAdministradoresActivos(CT).Returns(1);

        await FalleCon(TipoDeFalla.Conflicto, () => CreeElServicio().CambieElEstadoDelUsuario(2, false, CT));

        await _usuarios.DidNotReceiveWithAnyArgs().CambieElEstadoDeLaCuenta(default, default, default);
    }

    [Fact]
    public async Task Se_bloquea_a_los_administradores_antes_de_leer_y_se_puede_suspender_si_hay_otro_activo()
    {
        _usuarios.BusqueElEstadoDeLaCuenta(2, CT).Returns(new EstadoDeCuenta(true, Roles.ADMIN));
        _usuarios.CuenteLosAdministradoresActivos(CT).Returns(2);
        _usuarios.BusqueAlUsuarioParaAdministracion(2, CT).Returns(UnUsuarioAdmin(2));

        await CreeElServicio().CambieElEstadoDelUsuario(2, false, CT);

        Received.InOrder(() =>
        {
            _usuarios.BloqueeALosAdministradoresActivos(CT);
            _usuarios.BusqueElEstadoDeLaCuenta(2, CT);
            _usuarios.CambieElEstadoDeLaCuenta(2, false, CT);
        });
    }

    [Fact]
    public async Task Suspender_a_un_usuario_comun_no_cuenta_administradores()
    {
        _usuarios.BusqueElEstadoDeLaCuenta(3, CT).Returns(new EstadoDeCuenta(true, Roles.USUARIO));
        _usuarios.BusqueAlUsuarioParaAdministracion(3, CT).Returns(UnUsuarioAdmin(3));

        await CreeElServicio().CambieElEstadoDelUsuario(3, false, CT);

        await _usuarios.DidNotReceiveWithAnyArgs().CuenteLosAdministradoresActivos(default);
        await _usuarios.Received(1).CambieElEstadoDeLaCuenta(3, false, CT);
    }

    [Fact]
    public async Task Reactivar_a_un_administrador_no_exige_otro_activo()
    {
        _usuarios.BusqueElEstadoDeLaCuenta(2, CT).Returns(new EstadoDeCuenta(false, Roles.ADMIN));
        _usuarios.BusqueAlUsuarioParaAdministracion(2, CT).Returns(UnUsuarioAdmin(2));

        await CreeElServicio().CambieElEstadoDelUsuario(2, true, CT);

        await _usuarios.Received(1).CambieElEstadoDeLaCuenta(2, true, CT);
    }

    [Fact]
    public async Task Cambiar_el_estado_de_un_usuario_inexistente_da_404()
    {
        _usuarios.BusqueElEstadoDeLaCuenta(99, CT).Returns((EstadoDeCuenta?)null);
        await FalleCon(TipoDeFalla.RecursoInexistente, () => CreeElServicio().CambieElEstadoDelUsuario(99, false, CT));
    }
}
