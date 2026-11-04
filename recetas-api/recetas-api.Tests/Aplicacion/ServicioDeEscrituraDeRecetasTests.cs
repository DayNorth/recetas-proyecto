using NSubstitute;
using NSubstitute.ExceptionExtensions;
using Recetas.Api.Aplicacion.Puertos;
using Recetas.Api.Aplicacion.Servicios;
using Recetas.Api.Aplicacion.Solicitudes;
using Recetas.Api.Dominio;
using Recetas.Api.Tests.Comun;
using static Recetas.Api.Tests.Comun.Dobles;

namespace Recetas.Api.Tests.Aplicacion;

public class ServicioDeEscrituraDeRecetasTests
{
    private readonly IRepositorioDeRecetas _recetas = Substitute.For<IRepositorioDeRecetas>();
    private readonly IRepositorioDeCatalogos _catalogos = Substitute.For<IRepositorioDeCatalogos>();
    private readonly IAlmacenDeArchivos _archivos = Substitute.For<IAlmacenDeArchivos>();
    private readonly EjecutorSinBaseDeDatos _ejecutor = new();
    private IPuntoDeFalla _falla = Substitute.For<IPuntoDeFalla>();
    private int? _usuarioId = 2;

    public ServicioDeEscrituraDeRecetasTests()
    {
        _catalogos.ConsulteSiLaCategoriaEstaActiva(1, CT).Returns(true);
        _catalogos.ExisteLaDificultad(1, CT).Returns(true);
        _catalogos.CuenteLasUnidadesExistentes(Arg.Any<IReadOnlyCollection<short>>(), CT).Returns(1);
        _catalogos.CuenteLosIngredientesExistentes(Arg.Any<IReadOnlyCollection<int>>(), CT).Returns(1);
        _catalogos.AgregueLosIngredientesFaltantes(Arg.Any<IReadOnlyCollection<string>>(), CT)
            .Returns(new Dictionary<string, int> { ["sal"] = 7 });
        _recetas.AgregueLaReceta(Arg.Any<NuevaReceta>(), CT).Returns(100);
        _archivos.GuardeElArchivo(Arg.Any<Stream>(), "recetas", true, false, Arg.Any<long>(), CT).Returns("recetas/foto.jpg");
    }

    private ServicioDeEscrituraDeRecetas CreeElServicio() =>
        new(_recetas, _catalogos, new ControlDeAccesoARecetas(_recetas, Usuario(_usuarioId)), _ejecutor, _archivos, _falla, Usuario(_usuarioId));

    private static CrearRecetaRequest UnaSolicitud(short laCategoria = 1, params IngredienteEntrada[] losIngredientes) =>
        new("  Arroz   con pollo ", " rico ", laCategoria, 1, 30, 4,
            losIngredientes.Length > 0 ? [.. losIngredientes] : [new IngredienteEntrada(null, "Sal", 1, 1m, " al gusto ")],
            [" Cocinar ", "Servir"]);

    [Fact]
    public async Task Crear_con_categoria_inexistente_da_400_sin_abrir_transaccion_ni_guardar_imagen()
    {
        _catalogos.ConsulteSiLaCategoriaEstaActiva(9, CT).Returns((bool?)null);

        await FalleCon(TipoDeFalla.SolicitudInvalida, () => CreeElServicio().RegistreLaReceta(UnaSolicitud(9), UnArchivo(), CT));

        Assert.Equal(0, _ejecutor.Ejecuciones);
        await _archivos.DidNotReceiveWithAnyArgs().GuardeElArchivo(default!, default!, default, default, default, default);
    }

    [Fact]
    public async Task Crear_en_una_categoria_inactiva_da_400()
    {
        _catalogos.ConsulteSiLaCategoriaEstaActiva(2, CT).Returns(false);
        var laExcepcion = await FalleCon(TipoDeFalla.SolicitudInvalida, () => CreeElServicio().RegistreLaReceta(UnaSolicitud(2), null, CT));
        Assert.Equal("La categoría está inactiva.", laExcepcion.Titulo);
    }

    [Fact]
    public async Task Crear_valida_dificultad_unidades_e_ingredientes_por_id()
    {
        _catalogos.ExisteLaDificultad(1, CT).Returns(false);
        await FalleCon(TipoDeFalla.SolicitudInvalida, () => CreeElServicio().RegistreLaReceta(UnaSolicitud(), null, CT));

        _catalogos.ExisteLaDificultad(1, CT).Returns(true);
        _catalogos.CuenteLasUnidadesExistentes(Arg.Any<IReadOnlyCollection<short>>(), CT).Returns(0);
        await FalleCon(TipoDeFalla.SolicitudInvalida, () => CreeElServicio().RegistreLaReceta(UnaSolicitud(), null, CT));

        _catalogos.CuenteLasUnidadesExistentes(Arg.Any<IReadOnlyCollection<short>>(), CT).Returns(1);
        _catalogos.CuenteLosIngredientesExistentes(Arg.Any<IReadOnlyCollection<int>>(), CT).Returns(0);
        await FalleCon(TipoDeFalla.SolicitudInvalida,
            () => CreeElServicio().RegistreLaReceta(UnaSolicitud(1, new IngredienteEntrada(5, null, 1, 1m, null)), null, CT));
    }

    [Fact]
    public async Task Crear_ejecuta_los_pasos_en_orden_con_los_puntos_de_falla_entre_las_escrituras()
    {
        var laFalla = new FallaEnElPunto("ninguno");
        _falla = laFalla;

        var elId = await CreeElServicio().RegistreLaReceta(UnaSolicitud(), UnArchivo(), CT);

        Assert.Equal(100, elId);
        Assert.Equal(["receta-insertada", "detalle-insertado"], laFalla.PuntosVisitados);
        Received.InOrder(() =>
        {
            _catalogos.AgregueLosIngredientesFaltantes(Arg.Any<IReadOnlyCollection<string>>(), CT);
            _recetas.AgregueLaReceta(Arg.Any<NuevaReceta>(), CT);
            _recetas.AgregueElDetalleDeLaReceta(100, Arg.Any<DetalleDeLaReceta>(), CT);
        });
    }

    [Fact]
    public async Task Crear_normaliza_los_datos_y_deja_la_imagen_como_portada()
    {
        NuevaReceta? laNuevaReceta = null;
        DetalleDeLaReceta? elDetalle = null;
        await _recetas.AgregueLaReceta(Arg.Do<NuevaReceta>(r => laNuevaReceta = r), CT);
        await _recetas.AgregueElDetalleDeLaReceta(100, Arg.Do<DetalleDeLaReceta>(d => elDetalle = d), CT);

        await CreeElServicio().RegistreLaReceta(UnaSolicitud(), UnArchivo(), CT);

        Assert.Equal("Arroz con pollo", laNuevaReceta!.Datos.Titulo);
        Assert.Equal("rico", laNuevaReceta.Datos.Descripcion);
        Assert.Equal(OrigenesDeLaReceta.LOCAL, laNuevaReceta.Origen);
        Assert.Equal(2, laNuevaReceta.AutorId);
        Assert.Equal(["Cocinar", "Servir"], elDetalle!.Pasos);
        Assert.Equal("al gusto", elDetalle.Ingredientes[0].Nota);
        Assert.Equal(7, elDetalle.Ingredientes[0].IngredienteId);
        Assert.Equal([new MultimediaNueva(TiposDeMultimedia.IMAGEN, "recetas/foto.jpg", true)], elDetalle.Multimedia);
    }

    [Fact]
    public async Task Crear_con_el_mismo_ingrediente_por_id_y_por_nombre_da_409_y_borra_la_imagen()
    {
        var laSolicitud = UnaSolicitud(1, new IngredienteEntrada(7, null, 1, 1m, null), new IngredienteEntrada(null, "Sal", 1, 1m, null));

        await FalleCon(TipoDeFalla.Conflicto, () => CreeElServicio().RegistreLaReceta(laSolicitud, UnArchivo(), CT));

        _archivos.Received(1).BorreElArchivo("recetas/foto.jpg");
        await _recetas.DidNotReceiveWithAnyArgs().AgregueElDetalleDeLaReceta(default, default!, default);
    }

    [Fact]
    public async Task Crear_si_falla_tras_insertar_la_receta_no_inserta_el_detalle_y_borra_la_imagen()
    {
        _falla = new FallaEnElPunto("receta-insertada");

        await Assert.ThrowsAsync<InvalidOperationException>(() => CreeElServicio().RegistreLaReceta(UnaSolicitud(), UnArchivo(), CT));

        await _recetas.DidNotReceiveWithAnyArgs().AgregueElDetalleDeLaReceta(default, default!, default);
        _archivos.Received(1).BorreElArchivo("recetas/foto.jpg");
    }

    [Fact]
    public async Task Crear_sin_imagen_no_guarda_ni_borra_archivos_si_falla()
    {
        _recetas.AgregueLaReceta(Arg.Any<NuevaReceta>(), CT).ThrowsAsync(new InvalidOperationException());

        await Assert.ThrowsAsync<InvalidOperationException>(() => CreeElServicio().RegistreLaReceta(UnaSolicitud(), null, CT));

        _archivos.DidNotReceive().BorreElArchivo(Arg.Is<string?>(laRuta => laRuta != null));
        await _archivos.DidNotReceiveWithAnyArgs().GuardeElArchivo(default!, default!, default, default, default, default);
    }

    [Fact]
    public async Task Actualizar_una_receta_moderada_da_409()
    {
        _recetas.BusqueLaRecetaBasica(10, CT).Returns(UnaReceta(elAutorId: 2, elEstado: EstadosDeLaReceta.MODERADA));
        await FalleCon(TipoDeFalla.Conflicto, () => CreeElServicio().ActualiceLaReceta(10, UnaSolicitud(), CT));
        Assert.Equal(0, _ejecutor.Ejecuciones);
    }

    [Fact]
    public async Task Actualizar_una_receta_ajena_visible_da_403()
    {
        _recetas.BusqueLaRecetaBasica(10, CT).Returns(UnaReceta(elAutorId: 5));
        await FalleCon(TipoDeFalla.AccesoProhibido, () => CreeElServicio().ActualiceLaReceta(10, UnaSolicitud(), CT));
    }

    [Fact]
    public async Task Actualizar_conservando_una_categoria_ya_inactiva_esta_permitido()
    {
        _recetas.BusqueLaRecetaBasica(10, CT).Returns(UnaReceta(elAutorId: 2, laCategoriaId: 1));
        _catalogos.ConsulteSiLaCategoriaEstaActiva(1, CT).Returns(false);

        await CreeElServicio().ActualiceLaReceta(10, UnaSolicitud(1), CT);

        await _recetas.Received(1).ActualiceLosDatosDeLaReceta(10, Arg.Any<DatosDeLaReceta>(), CT);
    }

    [Fact]
    public async Task Actualizar_moviendo_la_receta_a_una_categoria_inactiva_da_400()
    {
        _recetas.BusqueLaRecetaBasica(10, CT).Returns(UnaReceta(elAutorId: 2, laCategoriaId: 1));
        _catalogos.ConsulteSiLaCategoriaEstaActiva(2, CT).Returns(false);

        await FalleCon(TipoDeFalla.SolicitudInvalida, () => CreeElServicio().ActualiceLaReceta(10, UnaSolicitud(2), CT));
    }

    [Fact]
    public async Task Actualizar_borra_el_detalle_antes_de_reinsertarlo_y_pasa_por_el_punto_de_falla()
    {
        var laFalla = new FallaEnElPunto("ninguno");
        _falla = laFalla;
        _recetas.BusqueLaRecetaBasica(10, CT).Returns(UnaReceta(elAutorId: 2));

        await CreeElServicio().ActualiceLaReceta(10, UnaSolicitud(), CT);

        Assert.Equal(["detalle-borrado"], laFalla.PuntosVisitados);
        Received.InOrder(() =>
        {
            _recetas.ActualiceLosDatosDeLaReceta(10, Arg.Any<DatosDeLaReceta>(), CT);
            _recetas.BorreElDetalleDeLaReceta(10, CT);
            _recetas.AgregueElDetalleDeLaReceta(10, Arg.Any<DetalleDeLaReceta>(), CT);
        });
    }

    [Fact]
    public async Task Actualizar_si_falla_tras_borrar_el_detalle_no_lo_reinserta()
    {
        _falla = new FallaEnElPunto("detalle-borrado");
        _recetas.BusqueLaRecetaBasica(10, CT).Returns(UnaReceta(elAutorId: 2));

        await Assert.ThrowsAsync<InvalidOperationException>(() => CreeElServicio().ActualiceLaReceta(10, UnaSolicitud(), CT));

        await _recetas.DidNotReceiveWithAnyArgs().AgregueElDetalleDeLaReceta(default, default!, default);
    }

    [Fact]
    public async Task Desactivar_una_receta_publicada_la_deja_inactiva()
    {
        _recetas.BusqueLaRecetaBasica(10, CT).Returns(UnaReceta(elAutorId: 2));
        await CreeElServicio().DesactiveLaReceta(10, CT);
        await _recetas.Received(1).EstablezcaElEstadoDeLaReceta(10, EstadosDeLaReceta.INACTIVA, CT);
    }

    [Fact]
    public async Task Desactivar_es_idempotente_si_ya_estaba_inactiva()
    {
        _recetas.BusqueLaRecetaBasica(10, CT).Returns(UnaReceta(elAutorId: 2, elEstado: EstadosDeLaReceta.INACTIVA));
        await CreeElServicio().DesactiveLaReceta(10, CT);
        await _recetas.DidNotReceiveWithAnyArgs().EstablezcaElEstadoDeLaReceta(default, default!, default);
    }

    [Fact]
    public async Task El_autor_no_puede_desactivar_una_receta_moderada()
    {
        _recetas.BusqueLaRecetaBasica(10, CT).Returns(UnaReceta(elAutorId: 2, elEstado: EstadosDeLaReceta.MODERADA));
        await FalleCon(TipoDeFalla.Conflicto, () => CreeElServicio().DesactiveLaReceta(10, CT));
    }
}
