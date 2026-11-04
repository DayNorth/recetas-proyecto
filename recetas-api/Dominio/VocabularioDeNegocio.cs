namespace Recetas.Api.Dominio;

public static class EstadosDeLaDenuncia
{
    public const string PENDIENTE = "PENDIENTE";
    public const string MANTENIDO = "MANTENIDO";
    public const string MODERADO = "MODERADO";

    public static readonly string[] TODOS = [PENDIENTE, MANTENIDO, MODERADO];
}

public static class DecisionesDeModeracion
{
    public const string MANTENER = "MANTENER";
    public const string MODERAR = "MODERAR";
}

public static class AccionesDeModeracion
{
    public const string RETIRAR = "RETIRAR";
    public const string RESTAURAR = "RESTAURAR";
}

public static class MotivosDeDenuncia
{
    public static readonly string[] VALIDOS = ["SPAM", "OFENSIVO", "INCORRECTO", "DERECHOS_AUTOR", "OTRO"];
}

public static class EstadosDeLaLista
{
    public const string ACTIVA = "ACTIVA";
    public const string COMPLETADA = "COMPLETADA";

    public static readonly string[] TODOS = [ACTIVA, COMPLETADA];
}

public static class TiposDeMultimedia
{
    public const string IMAGEN = "IMAGEN";
    public const string VIDEO = "VIDEO";
    public const string DOCUMENTO = "DOCUMENTO";
}

public static class OrigenesDeLaReceta
{
    public const string LOCAL = "LOCAL";
}
