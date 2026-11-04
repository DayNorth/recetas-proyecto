using Recetas.Api.Aplicacion.Solicitudes;
using Recetas.Api.Presentacion.Controladores;

namespace Recetas.Api.Tests.Aplicacion;

public class ValidadoresTests
{
    private static string[] Errores<T>(FluentValidation.IValidator<T> elValidador, T laSolicitud) =>
        [.. elValidador.Validate(laSolicitud).Errors.Select(e => e.ErrorMessage)];

    private static RegistroRequest UnRegistro(string clave = "clave1234", string confirmacion = "clave1234", string correo = "ana@correo.com") =>
        new("Ana", "Pérez", correo, clave, confirmacion);

    [Fact]
    public void Un_registro_correcto_no_tiene_errores() =>
        Assert.Empty(Errores(new RegistroRequestValidator(), UnRegistro()));

    [Theory]
    [InlineData("corta1", "La contraseña debe tener al menos 8 caracteres.")]
    [InlineData("sololetras", "La contraseña debe incluir al menos un número.")]
    [InlineData("12345678", "La contraseña debe incluir al menos una letra.")]
    public void La_contrasena_debe_ser_segura(string laClave, string elErrorEsperado) =>
        Assert.Contains(elErrorEsperado, Errores(new RegistroRequestValidator(), UnRegistro(laClave, laClave)));

    [Fact]
    public void La_contrasena_no_puede_superar_72_caracteres_limite_de_bcrypt() =>
        Assert.Contains("La contraseña no puede superar 72 caracteres.", Errores(new RegistroRequestValidator(),
            UnRegistro(new string('a', 72) + "1", new string('a', 72) + "1")));

    [Fact]
    public void La_confirmacion_debe_coincidir_y_el_correo_ser_valido()
    {
        var losErrores = Errores(new RegistroRequestValidator(), UnRegistro(confirmacion: "otra1234", correo: "no-es-correo"));

        Assert.Contains("La confirmación no coincide con la contraseña.", losErrores);
        Assert.Contains("El correo no es válido.", losErrores);
    }

    private static CrearRecetaRequest UnaReceta(List<IngredienteEntrada>? losIngredientes = null, List<string>? losPasos = null,
        string elTitulo = "Arroz", int elTiempo = 30, int lasPorciones = 4) =>
        new(elTitulo, null, 1, 1, elTiempo, lasPorciones, losIngredientes ?? [new IngredienteEntrada(null, "Sal", 1, 1m, null)], losPasos ?? ["Cocinar"]);

    [Fact]
    public void Una_receta_correcta_no_tiene_errores() =>
        Assert.Empty(Errores(new CrearRecetaRequestValidator(), UnaReceta()));

    [Fact]
    public void Una_receta_necesita_titulo_ingredientes_y_pasos()
    {
        var losErrores = Errores(new CrearRecetaRequestValidator(), UnaReceta(losIngredientes: [], losPasos: [], elTitulo: ""));

        Assert.Contains("El título es obligatorio.", losErrores);
        Assert.Contains("Debe incluir al menos un ingrediente.", losErrores);
        Assert.Contains("Debe incluir al menos un paso.", losErrores);
    }

    [Fact]
    public void Un_ingrediente_debe_indicar_id_o_nombre_pero_no_ambos_ni_ninguno()
    {
        var ambos = Errores(new CrearRecetaRequestValidator(), UnaReceta([new IngredienteEntrada(1, "Sal", 1, 1m, null)]));
        var ninguno = Errores(new CrearRecetaRequestValidator(), UnaReceta([new IngredienteEntrada(null, "  ", 1, 1m, null)]));

        Assert.Contains("Cada ingrediente debe indicar ingredienteId o nombre (uno solo).", ambos);
        Assert.Contains("Cada ingrediente debe indicar ingredienteId o nombre (uno solo).", ninguno);
    }

    [Theory]
    [InlineData(0)]
    [InlineData(-1)]
    public void La_cantidad_debe_ser_positiva(double laCantidad) =>
        Assert.Contains("La cantidad debe ser mayor que 0.", Errores(new CrearRecetaRequestValidator(),
            UnaReceta([new IngredienteEntrada(null, "Sal", 1, (decimal)laCantidad, null)])));

    [Fact]
    public void Los_ingredientes_repetidos_por_id_o_por_nombre_sin_distinguir_mayusculas_se_detectan()
    {
        var porId = Errores(new CrearRecetaRequestValidator(), UnaReceta([new IngredienteEntrada(5, null, 1, 1m, null), new IngredienteEntrada(5, null, 2, 2m, null)]));
        var porNombre = Errores(new CrearRecetaRequestValidator(), UnaReceta([new IngredienteEntrada(null, "Sal", 1, 1m, null), new IngredienteEntrada(null, "  SAL ", 1, 1m, null)]));

        Assert.Contains("Hay ingredientes repetidos.", porId);
        Assert.Contains("Hay ingredientes repetidos.", porNombre);
    }

    [Theory]
    [InlineData(0, 4, "El tiempo de preparación debe ser mayor que 0.")]
    [InlineData(30, 0, "Las porciones deben estar entre 1 y 1000.")]
    [InlineData(30, 1001, "Las porciones deben estar entre 1 y 1000.")]
    public void El_tiempo_y_las_porciones_tienen_limites(int elTiempo, int lasPorciones, string elError) =>
        Assert.Contains(elError, Errores(new CrearRecetaRequestValidator(), UnaReceta(elTiempo: elTiempo, lasPorciones: lasPorciones)));

    [Fact]
    public void Un_paso_no_puede_estar_vacio() =>
        Assert.Contains("Un paso no puede estar vacío.", Errores(new CrearRecetaRequestValidator(), UnaReceta(losPasos: ["Cocinar", " "])));

    [Theory]
    [InlineData(null, null, false)]
    [InlineData(null, "  ", false)]
    [InlineData(0, null, false)]
    [InlineData(6, null, false)]
    [InlineData(5, null, true)]
    [InlineData(null, "rico", true)]
    public void Calificar_exige_una_nota_de_1_a_5_o_un_comentario(int? laNota, string? elComentario, bool esValida) =>
        Assert.Equal(esValida, new CalificarRequestValidator().Validate(new CalificarRequest(laNota, elComentario)).IsValid);

    [Theory]
    [InlineData(10, null, "SPAM", true)]
    [InlineData(null, 7, "otro", true)]
    [InlineData(10, 7, "SPAM", false)]
    [InlineData(null, null, "SPAM", false)]
    [InlineData(10, null, "ACOSO", false)]
    [InlineData(10, null, "", false)]
    public void Una_denuncia_apunta_a_un_solo_objetivo_con_un_motivo_de_la_lista(int? laReceta, int? elComentario, string elMotivo, bool esValida) =>
        Assert.Equal(esValida, new CrearReporteContenidoRequestValidator().Validate(new CrearReporteContenidoRequest(laReceta, elComentario, elMotivo, null)).IsValid);

    [Theory]
    [InlineData("MANTENER", "Se revisó y está bien", true)]
    [InlineData("moderar", "Se revisó y está mal!", true)]
    [InlineData("BORRAR", "Se revisó y está mal!", false)]
    [InlineData("MODERAR", "corta", false)]
    [InlineData("MODERAR", "", false)]
    public void Resolver_exige_una_decision_valida_y_una_resolucion_de_al_menos_10_caracteres(string laDecision, string laResolucion, bool esValida) =>
        Assert.Equal(esValida, new ResolverReporteRequestValidator().Validate(new ResolverReporteRequest(laDecision, laResolucion)).IsValid);

    [Fact]
    public void El_motivo_de_una_accion_es_obligatorio_y_tiene_tope()
    {
        Assert.False(new MotivoRequestValidator().Validate(new MotivoRequest("")).IsValid);
        Assert.False(new MotivoRequestValidator().Validate(new MotivoRequest(new string('x', 501))).IsValid);
        Assert.True(new MotivoRequestValidator().Validate(new MotivoRequest("razón")).IsValid);
    }

    [Theory]
    [InlineData(1, 1, true)]
    [InlineData(0, 1, false)]
    [InlineData(1, 0, false)]
    [InlineData(1, 101, false)]
    public void Crear_una_lista_exige_receta_y_porciones_de_1_a_100(int laReceta, int lasPorciones, bool esValida) =>
        Assert.Equal(esValida, new CrearListaRequestValidator().Validate(new CrearListaRequest(laReceta, lasPorciones, null)).IsValid);

    [Fact]
    public void Una_categoria_exige_nombre_de_hasta_60_caracteres()
    {
        Assert.False(new CategoriaRequestValidator().Validate(new CategoriaRequest("", null)).IsValid);
        Assert.False(new CategoriaRequestValidator().Validate(new CategoriaRequest(new string('x', 61), null)).IsValid);
        Assert.True(new CategoriaRequestValidator().Validate(new CategoriaRequest("Postres", null)).IsValid);
    }

    [Fact]
    public void Una_noticia_exige_titulo_y_contenido()
    {
        Assert.False(new NoticiaFormValidator().Validate(new NoticiaForm { Titulo = "", Contenido = "x" }).IsValid);
        Assert.False(new NoticiaFormValidator().Validate(new NoticiaForm { Titulo = "x", Contenido = null }).IsValid);
        Assert.True(new NoticiaFormValidator().Validate(new NoticiaForm { Titulo = "x", Contenido = "y" }).IsValid);
    }

    [Fact]
    public void Actualizar_el_perfil_limita_los_largos()
    {
        Assert.True(new ActualizarPerfilRequestValidator().Validate(new ActualizarPerfilRequest("Ana", "Pérez", null)).IsValid);
        Assert.False(new ActualizarPerfilRequestValidator().Validate(new ActualizarPerfilRequest("Ana", "Pérez", new string('x', 301))).IsValid);
    }
}
