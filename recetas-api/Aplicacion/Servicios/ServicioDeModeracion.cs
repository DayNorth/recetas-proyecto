using Recetas.Api.Aplicacion.Comun;
using Recetas.Api.Aplicacion.Dtos;
using Recetas.Api.Aplicacion.Puertos;
using Recetas.Api.Aplicacion.Solicitudes;
using Recetas.Api.Dominio;

namespace Recetas.Api.Aplicacion.Servicios;

public class ServicioDeModeracion(IRepositorioDeModeracion laModeracion, IRepositorioDeRecetas lasRecetas,
    IEjecutorDeTransacciones elEjecutor, IPuntoDeFalla elPuntoDeFalla, IUsuarioActual elUsuarioActual, TimeProvider elReloj)
{
    private const int LARGO_MAXIMO_DE_LA_RESOLUCION = 500;

    public async Task<ReporteCreadoDto> DenuncieElContenido(CrearReporteContenidoRequest laSolicitud, CancellationToken cancelacion)
    {
        var elUsuarioId = elUsuarioActual.IdRequerido;
        if (laSolicitud.RecetaId is not null)
        {
            var laReceta = await lasRecetas.BusqueLaRecetaBasica(laSolicitud.RecetaId.Value, cancelacion);
            if (laReceta is not { Estado: EstadosDeLaReceta.PUBLICADA })
                throw ExcepcionDeNegocio.PorRecursoInexistente("La receta no existe.");
            if (laReceta.AutorId == elUsuarioId)
                throw ExcepcionDeNegocio.PorSolicitudInvalida("No puede reportar su propia receta.");
        }
        else
        {
            var elAutorDelComentario = await laModeracion.BusqueElAutorDelComentarioVisible(laSolicitud.ComentarioId!.Value, cancelacion)
                                       ?? throw ExcepcionDeNegocio.PorRecursoInexistente("El comentario no existe.");
            if (elAutorDelComentario == elUsuarioId)
                throw ExcepcionDeNegocio.PorSolicitudInvalida("No puede reportar su propio comentario.");
        }

        if (await laModeracion.ExisteUnaDenunciaPendiente(elUsuarioId, laSolicitud.RecetaId, laSolicitud.ComentarioId, cancelacion))
            throw ExcepcionDeNegocio.PorConflicto("Ya tiene una denuncia pendiente sobre este contenido.");

        return await laModeracion.AgregueLaDenuncia(elUsuarioId, laSolicitud.RecetaId, laSolicitud.ComentarioId,
            laSolicitud.Motivo.Trim().ToUpperInvariant(), Texto.RecorteOMarqueNulo(laSolicitud.Descripcion), cancelacion);
    }

    public Task<Paginado<ReporteListadoDto>> ListeLasDenuncias(string? elEstado, int? laPagina, int? elTamano, CancellationToken cancelacion)
    {
        var elEstadoNormalizado = string.IsNullOrWhiteSpace(elEstado)
            ? EstadosDeLaDenuncia.PENDIENTE
            : elEstado.Trim().ToUpperInvariant();
        if (!EstadosDeLaDenuncia.TODOS.Contains(elEstadoNormalizado))
            throw ExcepcionDeNegocio.PorSolicitudInvalida("Estado inválido.", "Use PENDIENTE, MANTENIDO o MODERADO.");

        var laPaginaPedida = SolicitudDePagina.Normalice(laPagina, elTamano);
        return elEstadoNormalizado == EstadosDeLaDenuncia.PENDIENTE
            ? laModeracion.ListeLasDenunciasPendientes(laPaginaPedida, cancelacion)
            : laModeracion.ListeLasDenunciasAtendidas(elEstadoNormalizado, laPaginaPedida, cancelacion);
    }

    public async Task<ReporteDetalleDto> ObtengaLaDenuncia(int laDenunciaId, CancellationToken cancelacion) =>
        await laModeracion.BusqueElDetalleDeLaDenuncia(laDenunciaId, cancelacion)
        ?? throw ExcepcionDeNegocio.PorRecursoInexistente("La denuncia no existe.");

    public Task<ResolucionDto> ResuelvaLaDenuncia(int laDenunciaId, ResolverReporteRequest laSolicitud, CancellationToken cancelacion)
    {
        var elAdminId = elUsuarioActual.IdRequerido;
        var seModera = laSolicitud.Decision.ToUpperInvariant() == DecisionesDeModeracion.MODERAR;
        var laResolucion = laSolicitud.Resolucion.Trim();

        return elEjecutor.EjecuteEnTransaccion(async laCancelacion =>
        {
            var elEstadoActual = await laModeracion.BloqueeLaDenunciaYObtengaSuEstado(laDenunciaId, laCancelacion)
                                 ?? throw ExcepcionDeNegocio.PorRecursoInexistente("La denuncia no existe.");
            if (elEstadoActual != EstadosDeLaDenuncia.PENDIENTE)
                throw ExcepcionDeNegocio.PorConflicto("La denuncia ya fue resuelta.");

            var elContenido = await laModeracion.BusqueElContenidoDenunciado(laDenunciaId, laCancelacion);
            var laFecha = elReloj.GetUtcNow().UtcDateTime;

            if (!seModera)
            {
                await laModeracion.MarqueLaDenuncia(laDenunciaId, EstadosDeLaDenuncia.MANTENIDO, laResolucion, elAdminId, laFecha, laCancelacion);
                return new ResolucionDto(laDenunciaId, EstadosDeLaDenuncia.MANTENIDO, 0);
            }

            var huboCambio = await RetireElContenidoDenunciado(elContenido, laCancelacion);
            elPuntoDeFalla.PaseElPuntoDeFalla("antes-de-moderacion");

            if (huboCambio)
                await laModeracion.AgregueLaAccionDeModeracion(elAdminId, elContenido.RecetaId, elContenido.ComentarioId, laDenunciaId,
                    AccionesDeModeracion.RETIRAR, laResolucion, laCancelacion);

            await laModeracion.MarqueLaDenuncia(laDenunciaId, EstadosDeLaDenuncia.MODERADO, laResolucion, elAdminId, laFecha, laCancelacion);
            var elTextoDeLasDemas = Texto.LimiteElLargo($"Resuelta junto con la denuncia #{laDenunciaId}: " + laResolucion,
                LARGO_MAXIMO_DE_LA_RESOLUCION);
            var lasCerradas = await laModeracion.CierreLasDemasDenunciasPendientes(laDenunciaId, elContenido, elTextoDeLasDemas,
                elAdminId, laFecha, laCancelacion);
            return new ResolucionDto(laDenunciaId, EstadosDeLaDenuncia.MODERADO, lasCerradas);
        }, cancelacion);
    }

    private async Task<bool> RetireElContenidoDenunciado(ContenidoDenunciado elContenido, CancellationToken cancelacion)
    {
        if (elContenido.RecetaId is not int laRecetaId)
            return await laModeracion.OculteElComentarioVisible(elContenido.ComentarioId!.Value, cancelacion);

        var huboCambio = await lasRecetas.CambieElEstadoSiEs(laRecetaId, EstadosDeLaReceta.PUBLICADA, EstadosDeLaReceta.MODERADA, cancelacion);
        if (!huboCambio && await laModeracion.LaRecetaNoEstaModerada(laRecetaId, cancelacion))
            throw ExcepcionDeNegocio.PorConflicto("La receta ya no está publicada; use MANTENER para cerrar la denuncia.");
        return huboCambio;
    }

    public Task<EstadoContenidoDto> RetireLaReceta(int laRecetaId, MotivoRequest laSolicitud, CancellationToken cancelacion) =>
        CambieElEstadoDeLaReceta(laRecetaId, laSolicitud.Motivo, EstadosDeLaReceta.PUBLICADA, EstadosDeLaReceta.MODERADA,
            AccionesDeModeracion.RETIRAR, cancelacion);

    public Task<EstadoContenidoDto> RestaureLaReceta(int laRecetaId, MotivoRequest laSolicitud, CancellationToken cancelacion) =>
        CambieElEstadoDeLaReceta(laRecetaId, laSolicitud.Motivo, EstadosDeLaReceta.MODERADA, EstadosDeLaReceta.PUBLICADA,
            AccionesDeModeracion.RESTAURAR, cancelacion);

    public Task<EstadoContenidoDto> RetireElComentario(int elComentarioId, MotivoRequest laSolicitud, CancellationToken cancelacion) =>
        CambieLaVisibilidadDelComentario(elComentarioId, laSolicitud.Motivo, eraVisible: true, AccionesDeModeracion.RETIRAR, cancelacion);

    public Task<EstadoContenidoDto> RestaureElComentario(int elComentarioId, MotivoRequest laSolicitud, CancellationToken cancelacion) =>
        CambieLaVisibilidadDelComentario(elComentarioId, laSolicitud.Motivo, eraVisible: false, AccionesDeModeracion.RESTAURAR, cancelacion);

    private Task<EstadoContenidoDto> CambieElEstadoDeLaReceta(int laRecetaId, string elMotivo, string desde, string hasta,
        string laAccion, CancellationToken cancelacion)
    {
        var elAdminId = elUsuarioActual.IdRequerido;
        var elMotivoRecortado = elMotivo.Trim();
        return elEjecutor.EjecuteEnTransaccion(async laCancelacion =>
        {
            if (!await lasRecetas.CambieElEstadoSiEs(laRecetaId, desde, hasta, laCancelacion))
            {
                if (await lasRecetas.BusqueLaRecetaBasica(laRecetaId, laCancelacion) is null)
                    throw ExcepcionDeNegocio.PorRecursoInexistente("La receta no existe.");
                throw ExcepcionDeNegocio.PorConflicto($"Solo se puede {laAccion.ToLowerInvariant()} una receta en estado {desde}.");
            }
            elPuntoDeFalla.PaseElPuntoDeFalla("antes-de-moderacion");
            await laModeracion.AgregueLaAccionDeModeracion(elAdminId, laRecetaId, null, null, laAccion, elMotivoRecortado, laCancelacion);
            return new EstadoContenidoDto(laRecetaId, hasta);
        }, cancelacion);
    }

    private Task<EstadoContenidoDto> CambieLaVisibilidadDelComentario(int elComentarioId, string elMotivo, bool eraVisible,
        string laAccion, CancellationToken cancelacion)
    {
        var elAdminId = elUsuarioActual.IdRequerido;
        var elMotivoRecortado = elMotivo.Trim();
        return elEjecutor.EjecuteEnTransaccion(async laCancelacion =>
        {
            if (!await laModeracion.CambieLaVisibilidadDelComentario(elComentarioId, eraVisible, laCancelacion))
            {
                if (!await laModeracion.ExisteElComentario(elComentarioId, laCancelacion))
                    throw ExcepcionDeNegocio.PorRecursoInexistente("El comentario no existe.");
                throw ExcepcionDeNegocio.PorConflicto(eraVisible ? "El comentario ya está oculto." : "El comentario ya está visible.");
            }
            elPuntoDeFalla.PaseElPuntoDeFalla("antes-de-moderacion");
            await laModeracion.AgregueLaAccionDeModeracion(elAdminId, null, elComentarioId, null, laAccion, elMotivoRecortado, laCancelacion);
            return new EstadoContenidoDto(elComentarioId, eraVisible ? "OCULTO" : "VISIBLE");
        }, cancelacion);
    }

    public Task<Paginado<HistorialDto>> ListeElHistorial(FiltroDelHistorial elFiltro, int? laPagina, int? elTamano,
        CancellationToken cancelacion)
    {
        if (elFiltro.Desde is not null && elFiltro.Hasta is not null && elFiltro.Desde > elFiltro.Hasta)
            throw ExcepcionDeNegocio.PorSolicitudInvalida("desde no puede ser posterior a hasta.");

        var desdeUtc = elFiltro.Desde is null ? (DateTime?)null : ZonaHoraria.ConviertaElInicioDelDiaAUtc(elFiltro.Desde.Value);
        var hastaUtcExclusivo = elFiltro.Hasta is null ? (DateTime?)null : ZonaHoraria.ConviertaElInicioDelDiaAUtc(elFiltro.Hasta.Value.AddDays(1));
        return laModeracion.ListeElHistorial(elFiltro.RecetaId, elFiltro.ComentarioId, desdeUtc, hastaUtcExclusivo,
            SolicitudDePagina.Normalice(laPagina, elTamano), cancelacion);
    }
}
