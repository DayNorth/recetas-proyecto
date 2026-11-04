using Recetas.Api.Aplicacion.Puertos;
using Recetas.Api.Dominio;

namespace Recetas.Api.Aplicacion.Servicios;

public class ControlDeAccesoARecetas(IRepositorioDeRecetas lasRecetas, IUsuarioActual elUsuarioActual)
{
    public bool PuedeVerLaReceta(string elEstado, int elAutorId) =>
        ReglasDeAcceso.PuedeVerLaReceta(elEstado, elAutorId, elUsuarioActual.EsAdministrador, elUsuarioActual.Id);

    public async Task<RecetaBasica> ExijaAlAutor(int laRecetaId, CancellationToken cancelacion)
    {
        var laReceta = await ExijaLaRecetaExistente(laRecetaId, cancelacion);
        if (laReceta.AutorId == elUsuarioActual.Id) return laReceta;
        if (!PuedeVerLaReceta(laReceta.Estado, laReceta.AutorId)) throw ExcepcionDeNegocio.PorRecursoInexistente("La receta no existe.");
        throw ExcepcionDeNegocio.PorAccesoProhibido("Solo el autor puede modificar esta receta.");
    }

    public async Task<RecetaBasica> ExijaLaRecetaPublicada(int laRecetaId, CancellationToken cancelacion)
    {
        var laReceta = await lasRecetas.BusqueLaRecetaBasica(laRecetaId, cancelacion);
        return laReceta is { Estado: EstadosDeLaReceta.PUBLICADA }
            ? laReceta
            : throw ExcepcionDeNegocio.PorRecursoInexistente("La receta no existe.");
    }

    public async Task<RecetaBasica> ExijaLaRecetaExistente(int laRecetaId, CancellationToken cancelacion) =>
        await lasRecetas.BusqueLaRecetaBasica(laRecetaId, cancelacion)
        ?? throw ExcepcionDeNegocio.PorRecursoInexistente("La receta no existe.");
}
