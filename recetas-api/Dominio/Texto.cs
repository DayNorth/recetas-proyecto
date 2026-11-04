namespace Recetas.Api.Dominio;

public static class Texto
{
    public static string NormaliceElTexto(string elTexto) =>
        string.Join(' ', elTexto.Split((char[]?)null, StringSplitOptions.RemoveEmptyEntries));

    public static string CalculeLaClaveDelNombre(string elNombre) => NormaliceElTexto(elNombre).ToLowerInvariant();

    public static string? RecorteOMarqueNulo(string? elTexto) =>
        string.IsNullOrWhiteSpace(elTexto) ? null : elTexto.Trim();

    public static string LimiteElLargo(string elTexto, int elMaximo) =>
        elTexto.Length > elMaximo ? elTexto[..elMaximo] : elTexto;
}
