using Recetas.Api.Aplicacion.Comun;
using Recetas.Api.Aplicacion.Dtos;
using Recetas.Api.Aplicacion.Puertos;
using Recetas.Api.Aplicacion.Solicitudes;
using Recetas.Api.Dominio;

namespace Recetas.Api.Aplicacion.Servicios;

public class ServicioDeListas(IRepositorioDeListas lasListas, ControlDeAccesoARecetas elControlDeAcceso,
    IEjecutorDeTransacciones elEjecutor, IPuntoDeFalla elPuntoDeFalla, IUsuarioActual elUsuarioActual)
{
    private const int LARGO_MAXIMO_DEL_NOMBRE = 80;

    public async Task<ListaDetalleDto> CreeLaLista(CrearListaRequest laSolicitud, CancellationToken cancelacion)
    {
        var laReceta = await elControlDeAcceso.ExijaLaRecetaPublicada(laSolicitud.RecetaId, cancelacion);
        var elUsuarioId = elUsuarioActual.IdRequerido;
        var elNombre = string.IsNullOrWhiteSpace(laSolicitud.Nombre)
            ? $"Lista de {laReceta.Titulo}"
            : Texto.NormaliceElTexto(laSolicitud.Nombre);
        elNombre = Texto.LimiteElLargo(elNombre, LARGO_MAXIMO_DEL_NOMBRE);

        var laListaId = await elEjecutor.EjecuteEnTransaccion(async laCancelacion =>
        {
            var elId = await lasListas.AgregueLaLista(elUsuarioId, elNombre, laCancelacion);
            var losItemsInsertados = await lasListas.AgregueLosItemsDesdeLaReceta(elId, laSolicitud.RecetaId, laSolicitud.Porciones, laCancelacion);
            if (losItemsInsertados == 0)
                throw ExcepcionDeNegocio.PorConflicto("La receta no tiene ingredientes para generar la lista.");
            return elId;
        }, cancelacion);

        return await ObtengaLaLista(laListaId, cancelacion);
    }

    public Task<Paginado<ListaResumenDto>> ListeLasListas(string? elEstado, int? laPagina, int? elTamano, CancellationToken cancelacion)
    {
        string? elEstadoNormalizado = null;
        if (!string.IsNullOrWhiteSpace(elEstado))
        {
            elEstadoNormalizado = elEstado.Trim().ToUpperInvariant();
            if (!EstadosDeLaLista.TODOS.Contains(elEstadoNormalizado))
                throw ExcepcionDeNegocio.PorSolicitudInvalida("Estado inválido.", "Use ACTIVA o COMPLETADA.");
        }
        return lasListas.ListeLasListas(elUsuarioActual.IdRequerido, elEstadoNormalizado,
            SolicitudDePagina.Normalice(laPagina, elTamano), cancelacion);
    }

    public async Task<ListaDetalleDto> ObtengaLaLista(int laListaId, CancellationToken cancelacion) =>
        await lasListas.BusqueElDetalleDeLaLista(elUsuarioActual.IdRequerido, laListaId, cancelacion)
        ?? throw ExcepcionDeNegocio.PorRecursoInexistente("La lista no existe.");

    public async Task<ItemActualizadoDto> MarqueElItem(int laListaId, int elItemId, MarcarItemRequest laSolicitud,
        CancellationToken cancelacion)
    {
        if (!await lasListas.ExisteLaListaDelUsuario(elUsuarioActual.IdRequerido, laListaId, cancelacion))
            throw ExcepcionDeNegocio.PorRecursoInexistente("La lista no existe.");

        var elEstadoDeLaLista = await elEjecutor.EjecuteEnTransaccion(async laCancelacion =>
        {
            await lasListas.BloqueeLaLista(laListaId, laCancelacion);
            if (!await lasListas.MarqueElItem(laListaId, elItemId, laSolicitud.Comprado, laCancelacion))
                throw ExcepcionDeNegocio.PorRecursoInexistente("El ítem no existe en esta lista.");
            elPuntoDeFalla.PaseElPuntoDeFalla("item-actualizado");

            var elEstadoNuevo = await lasListas.HayItemsPendientes(laListaId, laCancelacion)
                ? EstadosDeLaLista.ACTIVA
                : EstadosDeLaLista.COMPLETADA;
            await lasListas.EstablezcaElEstadoDeLaLista(laListaId, elEstadoNuevo, laCancelacion);
            return elEstadoNuevo;
        }, cancelacion);

        return new ItemActualizadoDto(await lasListas.BusqueElItem(elItemId, cancelacion), elEstadoDeLaLista);
    }

    public async Task BorreLaLista(int laListaId, CancellationToken cancelacion)
    {
        if (!await lasListas.BorreLaLista(elUsuarioActual.IdRequerido, laListaId, cancelacion))
            throw ExcepcionDeNegocio.PorRecursoInexistente("La lista no existe.");
    }
}
