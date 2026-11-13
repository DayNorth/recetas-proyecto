using System.Reflection;
using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging.Abstractions;
using Npgsql;
using NSubstitute;
using Recetas.Api.Dominio;
using Recetas.Api.Presentacion.Controladores;
using Recetas.Api.Presentacion.Middleware;
using Recetas.Api.Presentacion.Seguridad;

namespace Recetas.Api.Tests.Presentacion;

public class AutorizacionDeControladoresTests
{
    private record Accion(Type Controlador, MethodInfo Metodo)
    {
        public string Nombre => $"{Controlador.Name}.{Metodo.Name}";

        private T? Atributo<T>() where T : Attribute => Metodo.GetCustomAttribute<T>() ?? Controlador.GetCustomAttribute<T>();

        public bool EsAnonima => Metodo.GetCustomAttribute<AllowAnonymousAttribute>() is not null
                                 || (Metodo.GetCustomAttribute<AuthorizeAttribute>() is null && Controlador.GetCustomAttribute<AllowAnonymousAttribute>() is not null);

        public string? Politica => (Metodo.GetCustomAttribute<AuthorizeAttribute>() ?? Controlador.GetCustomAttribute<AuthorizeAttribute>())?.Policy;

        public bool TieneAutorizacionExplicita => Atributo<AllowAnonymousAttribute>() is not null || Atributo<AuthorizeAttribute>() is not null;
    }

    private static List<Accion> ListeLasAcciones() =>
        typeof(RecetasController).Assembly.GetTypes()
            .Where(t => !t.IsAbstract && typeof(ControllerBase).IsAssignableFrom(t))
            .SelectMany(c => c.GetMethods(BindingFlags.Public | BindingFlags.Instance | BindingFlags.DeclaredOnly)
                .Where(m => m.GetCustomAttributes().Any(a => a is Microsoft.AspNetCore.Mvc.Routing.HttpMethodAttribute))
                .Select(m => new Accion(c, m)))
            .ToList();

    [Fact]
    public void La_api_expone_63_endpoints()
    {
        Assert.Equal(63, ListeLasAcciones().Count);
    }

    [Fact]
    public void Todo_endpoint_declara_su_autorizacion_de_forma_explicita()
    {
        Assert.Empty(ListeLasAcciones().Where(a => !a.TieneAutorizacionExplicita).Select(a => a.Nombre));
    }

    [Fact]
    public void Los_endpoints_publicos_son_solo_los_previstos()
    {
        string[] esperados =
        [
            "AutenticacionController.RegistreAlUsuario", "AutenticacionController.InicieLaSesion",
            "CatalogosController.ListeLasCategorias", "CatalogosController.ListeLasDificultades", "CatalogosController.ListeLasUnidades",
            "CatalogosController.ListeLasRedesSociales", "CatalogosController.SugieraIngredientes",
            "SaludController.ObtengaElEstado",
            "RecetasController.BusqueLasRecetas", "RecetasController.ObtengaElDetalleDeLaReceta", "RecetasController.CalculeLosIngredientesPorPorciones",
            "InteraccionController.ListeLosComentarios", "InteraccionController.RegistreLaInteraccion", "InteraccionController.CompartaLaReceta",
            "NoticiasController.ListeLasNoticias", "NoticiasController.ObtengaLaNoticia"
        ];

        var losPublicos = ListeLasAcciones().Where(a => a.EsAnonima).Select(a => a.Nombre).Order().ToList();

        Assert.Equal(esperados.Order(), losPublicos);
    }

    [Theory]
    [InlineData(typeof(AdministracionController))]
    [InlineData(typeof(ModeracionController))]
    [InlineData(typeof(ReportesController))]
    public void Los_controladores_de_administracion_exigen_la_politica_de_administrador(Type elControlador)
    {
        var lasAcciones = ListeLasAcciones().Where(a => a.Controlador == elControlador).ToList();

        Assert.NotEmpty(lasAcciones);
        Assert.All(lasAcciones, a => Assert.Equal(Politicas.SOLO_ADMIN, a.Politica));
    }

    [Fact]
    public void Gestionar_noticias_es_solo_del_administrador_y_leerlas_es_publico()
    {
        var lasAcciones = ListeLasAcciones().Where(a => a.Controlador == typeof(NoticiasController)).ToDictionary(a => a.Metodo.Name);

        Assert.Equal(Politicas.SOLO_ADMIN, lasAcciones["CreeLaNoticia"].Politica);
        Assert.Equal(Politicas.SOLO_ADMIN, lasAcciones["EditeLaNoticia"].Politica);
        Assert.Equal(Politicas.SOLO_ADMIN, lasAcciones["DespubliqueLaNoticia"].Politica);
        Assert.True(lasAcciones["ListeLasNoticias"].EsAnonima);
    }

    [Fact]
    public void Las_acciones_de_un_usuario_autenticado_usan_la_politica_de_autenticado()
    {
        var lasProtegidasDeUsuario = ListeLasAcciones().Where(a => !a.EsAnonima && a.Politica == Politicas.AUTENTICADO).ToList();

        Assert.Equal(25, lasProtegidasDeUsuario.Count);
        Assert.Equal(22, ListeLasAcciones().Count(a => a.Politica == Politicas.SOLO_ADMIN));
    }

    [Fact]
    public void Las_rutas_de_los_reportes_y_otros_controladores_no_dependen_del_nombre_del_metodo()
    {
        Assert.All(ListeLasAcciones(), a =>
            Assert.True(a.Controlador.GetCustomAttribute<RouteAttribute>() is not null
                        || a.Metodo.GetCustomAttributes().OfType<Microsoft.AspNetCore.Mvc.Routing.HttpMethodAttribute>().Any(h => h.Template is not null),
                $"{a.Nombre} no tiene ruta explícita"));
    }
}

public class UsuarioActualTests
{
    private static UsuarioActual CreeElUsuario(params Claim[] losClaims)
    {
        var elAccesor = Substitute.For<IHttpContextAccessor>();
        elAccesor.HttpContext.Returns(new DefaultHttpContext
        {
            User = new ClaimsPrincipal(new ClaimsIdentity(losClaims, losClaims.Length == 0 ? null : "Bearer"))
        });
        return new UsuarioActual(elAccesor);
    }

    [Fact]
    public void Lee_el_id_el_rol_el_jti_y_la_expiracion_del_token()
    {
        var elUsuario = CreeElUsuario(new Claim("sub", "42"), new Claim("rol", "ADMIN"), new Claim("jti", "abc"), new Claim("exp", "1800000000"));

        Assert.True(elUsuario.EstaAutenticado);
        Assert.Equal((42, 42, "ADMIN", true, "abc"), (elUsuario.Id, elUsuario.IdRequerido, elUsuario.Rol, elUsuario.EsAdministrador, elUsuario.IdentificadorDelToken));
        Assert.Equal(DateTimeOffset.FromUnixTimeSeconds(1800000000), elUsuario.ExpiracionDelToken);
    }

    [Fact]
    public void Sin_sesion_no_hay_usuario_y_pedir_el_id_requerido_lanza()
    {
        var elAnonimo = CreeElUsuario();

        Assert.False(elAnonimo.EstaAutenticado);
        Assert.Null(elAnonimo.Id);
        Assert.False(elAnonimo.EsAdministrador);
        Assert.Throws<InvalidOperationException>(() => elAnonimo.IdRequerido);
    }

    [Fact]
    public void Un_usuario_comun_no_es_administrador() =>
        Assert.False(CreeElUsuario(new Claim("sub", "5"), new Claim("rol", "USUARIO")).EsAdministrador);
}

public class ManejadorGlobalDeExcepcionesTests
{
    private static async Task<int> ObtengaElEstado(Exception laExcepcion)
    {
        var losProblemas = Substitute.For<IProblemDetailsService>();
        losProblemas.TryWriteAsync(Arg.Any<ProblemDetailsContext>()).Returns(ValueTask.FromResult(true));
        var elContexto = new DefaultHttpContext();

        await new ManejadorGlobalDeExcepciones(losProblemas, NullLogger<ManejadorGlobalDeExcepciones>.Instance)
            .TryHandleAsync(elContexto, laExcepcion, CancellationToken.None);

        return elContexto.Response.StatusCode;
    }

    [Theory]
    [InlineData(TipoDeFalla.SolicitudInvalida, 400)]
    [InlineData(TipoDeFalla.FaltaDeAutenticacion, 401)]
    [InlineData(TipoDeFalla.AccesoProhibido, 403)]
    [InlineData(TipoDeFalla.RecursoInexistente, 404)]
    [InlineData(TipoDeFalla.Conflicto, 409)]
    [InlineData(TipoDeFalla.ServicioNoDisponible, 503)]
    public async Task Cada_tipo_de_falla_de_negocio_se_traduce_a_su_codigo_http(TipoDeFalla elTipo, int elCodigo) =>
        Assert.Equal(elCodigo, await ObtengaElEstado(new ExcepcionDeNegocio(elTipo, "x")));

    [Theory]
    [InlineData("23505", 409)]
    [InlineData("23503", 409)]
    [InlineData("23514", 422)]
    [InlineData("40001", 409)]
    [InlineData("40P01", 409)]
    [InlineData("XX000", 500)]
    public async Task Los_errores_de_postgres_se_traducen_segun_su_sqlstate(string elSqlState, int elCodigo) =>
        Assert.Equal(elCodigo, await ObtengaElEstado(new PostgresException("detalle técnico", "ERROR", "ERROR", elSqlState)));

    [Fact]
    public async Task Un_error_inesperado_da_500_sin_exponer_datos_y_una_peticion_mal_formada_da_400()
    {
        Assert.Equal(500, await ObtengaElEstado(new InvalidOperationException("secreto")));
        Assert.Equal(400, await ObtengaElEstado(new BadHttpRequestException("json roto")));
    }

    [Fact]
    public async Task Los_errores_de_validacion_dan_400()
    {
        var laFalla = new FluentValidation.ValidationException([new FluentValidation.Results.ValidationFailure("Titulo", "obligatorio")]);
        Assert.Equal(400, await ObtengaElEstado(laFalla));
    }
}
