namespace Recetas.Api.Dominio;

public static class EstadosDeLaReceta
{
    public const string BORRADOR = "BORRADOR";
    public const string PUBLICADA = "PUBLICADA";
    public const string INACTIVA = "INACTIVA";
    public const string MODERADA = "MODERADA";

    public static readonly string[] TODOS = [BORRADOR, PUBLICADA, INACTIVA, MODERADA];
}
