using System.Reflection;

namespace Recetas.Api.Tests;

public class ArquitecturaHexagonalTests
{
    private const string DOMINIO = "Recetas.Api.Dominio";
    private const string APLICACION = "Recetas.Api.Aplicacion";
    private const string INFRAESTRUCTURA = "Recetas.Api.Infraestructura";
    private const string PRESENTACION = "Recetas.Api.Presentacion";
    private const string CONTROLADORES = "Recetas.Api.Presentacion.Controladores";

    private static readonly string[] ENSAMBLADOS_DE_TECNOLOGIA_PROHIBIDOS =
    [
        "Microsoft.AspNetCore", "Microsoft.EntityFrameworkCore", "Npgsql", "ClosedXML", "QuestPDF", "BCrypt",
        "Microsoft.IdentityModel", "System.IdentityModel"
    ];

    private static readonly Assembly ENSAMBLADO_DE_LA_API = typeof(Program).Assembly;

    private static bool EstaEnLaCapa(Type elTipo, string laCapa) =>
        elTipo.Namespace is { } elEspacio && (elEspacio == laCapa || elEspacio.StartsWith(laCapa + "."));

    private static IEnumerable<Type> ListeLosTiposDeLaCapa(string laCapa) =>
        ENSAMBLADO_DE_LA_API.GetTypes().Where(elTipo => EstaEnLaCapa(elTipo, laCapa));

    private static IEnumerable<Type> ListeLosTiposReferenciados(Type elTipo)
    {
        const BindingFlags TODOS = BindingFlags.Instance | BindingFlags.Static | BindingFlags.Public | BindingFlags.NonPublic
                                   | BindingFlags.DeclaredOnly;
        var losTipos = new List<Type?> { elTipo.BaseType };
        losTipos.AddRange(elTipo.GetInterfaces());
        losTipos.AddRange(elTipo.GetFields(TODOS).Select(elCampo => elCampo.FieldType));
        losTipos.AddRange(elTipo.GetProperties(TODOS).Select(laPropiedad => laPropiedad.PropertyType));
        losTipos.AddRange(elTipo.GetConstructors(TODOS).SelectMany(elConstructor => elConstructor.GetParameters()).Select(elParametro => elParametro.ParameterType));
        foreach (var elMetodo in elTipo.GetMethods(TODOS))
        {
            losTipos.Add(elMetodo.ReturnType);
            losTipos.AddRange(elMetodo.GetParameters().Select(elParametro => elParametro.ParameterType));
        }
        return losTipos.Where(otro => otro is not null).SelectMany(otro => DesenvuelvaElTipo(otro!));
    }

    private static IEnumerable<Type> DesenvuelvaElTipo(Type elTipo)
    {
        yield return elTipo;
        if (elTipo.HasElementType)
            foreach (var elInterno in DesenvuelvaElTipo(elTipo.GetElementType()!)) yield return elInterno;
        if (elTipo.IsGenericType)
            foreach (var elArgumento in elTipo.GetGenericArguments().SelectMany(DesenvuelvaElTipo)) yield return elArgumento;
    }

    private static bool EsTecnologiaProhibida(Type elTipo) =>
        ENSAMBLADOS_DE_TECNOLOGIA_PROHIBIDOS.Any(elPrefijo => (elTipo.Assembly.GetName().Name ?? "").StartsWith(elPrefijo));

    private static List<string> BusqueLasInfracciones(string laCapa, Func<Type, bool> esProhibido) =>
        ListeLosTiposDeLaCapa(laCapa)
            .SelectMany(elTipo => ListeLosTiposReferenciados(elTipo).Where(esProhibido).Select(elProhibido => $"{elTipo.FullName} -> {elProhibido.FullName}"))
            .Distinct().Order().ToList();

    [Fact]
    public void Las_capas_internas_existen_y_no_estan_vacias()
    {
        Assert.NotEmpty(ListeLosTiposDeLaCapa(DOMINIO));
        Assert.NotEmpty(ListeLosTiposDeLaCapa(APLICACION));
        Assert.NotEmpty(ListeLosTiposDeLaCapa(INFRAESTRUCTURA));
        Assert.NotEmpty(ListeLosTiposDeLaCapa(PRESENTACION));
    }

    [Fact]
    public void El_dominio_no_depende_de_ninguna_otra_capa_ni_de_tecnologias()
    {
        var infracciones = BusqueLasInfracciones(DOMINIO, elTipo =>
            EsTecnologiaProhibida(elTipo) || EstaEnLaCapa(elTipo, APLICACION) || EstaEnLaCapa(elTipo, INFRAESTRUCTURA)
            || EstaEnLaCapa(elTipo, PRESENTACION));
        Assert.Empty(infracciones);
    }

    [Fact]
    public void La_aplicacion_solo_depende_del_dominio_y_no_conoce_tecnologias()
    {
        var infracciones = BusqueLasInfracciones(APLICACION, elTipo =>
            EsTecnologiaProhibida(elTipo) || EstaEnLaCapa(elTipo, INFRAESTRUCTURA) || EstaEnLaCapa(elTipo, PRESENTACION));
        Assert.Empty(infracciones);
    }

    [Fact]
    public void La_infraestructura_no_depende_de_la_presentacion()
    {
        var infracciones = BusqueLasInfracciones(INFRAESTRUCTURA, elTipo => EstaEnLaCapa(elTipo, PRESENTACION));
        Assert.Empty(infracciones);
    }

    [Fact]
    public void Los_controladores_hablan_con_los_casos_de_uso_y_no_con_la_persistencia()
    {
        var infracciones = BusqueLasInfracciones(CONTROLADORES, elTipo =>
            EstaEnLaCapa(elTipo, INFRAESTRUCTURA) || (elTipo.Assembly.GetName().Name ?? "").StartsWith("Microsoft.EntityFrameworkCore")
            || (elTipo.Assembly.GetName().Name ?? "").StartsWith("Npgsql"));
        Assert.Empty(infracciones);
    }
}
