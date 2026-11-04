namespace Recetas.Api.Dominio;

public static class ReglasDeAcceso
{
    public static bool PuedeVerLaReceta(string elEstado, int elAutorId, bool esAdministrador, int? elUsuarioId) =>
        elEstado == EstadosDeLaReceta.PUBLICADA || esAdministrador || elUsuarioId == elAutorId;
}
