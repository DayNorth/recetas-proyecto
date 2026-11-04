using Microsoft.EntityFrameworkCore;
using Recetas.Api.Aplicacion.Comun;
using Recetas.Api.Aplicacion.Dtos;
using Recetas.Api.Aplicacion.Puertos;
using Recetas.Api.Dominio;
using Recetas.Api.Infraestructura.Persistencia;
using Recetas.Api.Infraestructura.Persistencia.Entidades;

namespace Recetas.Api.Infraestructura.Repositorios;

public class RepositorioDeModeracion(RecetasDbContext laBaseDeDatos) : IRepositorioDeModeracion
{
    private const string SIN_TEXTO_SOLO_CALIFICACION = "(solo calificación)";

    public Task<int?> BusqueElAutorDelComentarioVisible(int elComentarioId, CancellationToken cancelacion) =>
        laBaseDeDatos.Comentarios.AsNoTracking()
            .Where(comentario => comentario.ComentarioId == elComentarioId && comentario.Visible
                                 && comentario.Receta.Estado == EstadosDeLaReceta.PUBLICADA)
            .Select(comentario => (int?)comentario.UsuarioId).FirstOrDefaultAsync(cancelacion);

    public Task<bool> ExisteUnaDenunciaPendiente(int elUsuarioId, int? laRecetaId, int? elComentarioId, CancellationToken cancelacion) =>
        laBaseDeDatos.ReportesContenido.AnyAsync(denuncia => denuncia.UsuarioId == elUsuarioId && denuncia.Estado == EstadosDeLaDenuncia.PENDIENTE
                                                             && denuncia.RecetaId == laRecetaId && denuncia.ComentarioId == elComentarioId, cancelacion);

    public async Task<ReporteCreadoDto> AgregueLaDenuncia(int elUsuarioId, int? laRecetaId, int? elComentarioId, string elMotivo,
        string? laDescripcion, CancellationToken cancelacion)
    {
        var laDenuncia = new ReporteContenido
        {
            UsuarioId = elUsuarioId,
            RecetaId = laRecetaId,
            ComentarioId = elComentarioId,
            Motivo = elMotivo,
            Descripcion = laDescripcion,
            Estado = EstadosDeLaDenuncia.PENDIENTE
        };
        laBaseDeDatos.ReportesContenido.Add(laDenuncia);
        await laBaseDeDatos.SaveChangesAsync(cancelacion);
        return new ReporteCreadoDto(laDenuncia.ReporteContenidoId, laDenuncia.Estado);
    }

    public Task<Paginado<ReporteListadoDto>> ListeLasDenunciasPendientes(SolicitudDePagina laPagina, CancellationToken cancelacion) =>
        laBaseDeDatos.VwReportesContenidoPendientes.AsNoTracking().OrderBy(vista => vista.Fecha).ThenBy(vista => vista.ReporteContenidoId)
            .Select(vista => new ReporteListadoDto(vista.ReporteContenidoId!.Value, vista.Fecha!.Value, vista.Motivo!, vista.Descripcion,
                vista.TipoContenido!, vista.Contenido, vista.ReportadoPor!, EstadosDeLaDenuncia.PENDIENTE))
            .PagineLaConsulta(laPagina, cancelacion);

    public Task<Paginado<ReporteListadoDto>> ListeLasDenunciasAtendidas(string elEstado, SolicitudDePagina laPagina,
        CancellationToken cancelacion) =>
        laBaseDeDatos.ReportesContenido.AsNoTracking().Where(denuncia => denuncia.Estado == elEstado)
            .OrderByDescending(denuncia => denuncia.FechaAtencion).ThenByDescending(denuncia => denuncia.ReporteContenidoId)
            .Select(denuncia => new ReporteListadoDto(denuncia.ReporteContenidoId, denuncia.Fecha, denuncia.Motivo, denuncia.Descripcion,
                denuncia.RecetaId != null ? "RECETA" : "COMENTARIO",
                denuncia.Receta != null ? denuncia.Receta.Titulo : (denuncia.Comentario!.Comentario1 ?? SIN_TEXTO_SOLO_CALIFICACION),
                denuncia.Usuario.Nombre + " " + denuncia.Usuario.Apellidos, denuncia.Estado))
            .PagineLaConsulta(laPagina, cancelacion);

    public async Task<ReporteDetalleDto?> BusqueElDetalleDeLaDenuncia(int laDenunciaId, CancellationToken cancelacion)
    {
        var laDenuncia = await laBaseDeDatos.ReportesContenido.AsNoTracking()
            .Include(denuncia => denuncia.Usuario).Include(denuncia => denuncia.AtendidoPorNavigation)
            .Include(denuncia => denuncia.Receta).ThenInclude(receta => receta!.Usuario)
            .Include(denuncia => denuncia.Comentario).ThenInclude(comentario => comentario!.Usuario)
            .Include(denuncia => denuncia.Comentario).ThenInclude(comentario => comentario!.Receta)
            .FirstOrDefaultAsync(denuncia => denuncia.ReporteContenidoId == laDenunciaId, cancelacion);
        if (laDenuncia is null) return null;

        var laRecetaId = laDenuncia.RecetaId;
        var elComentarioId = laDenuncia.ComentarioId;
        var lasOtras = await laBaseDeDatos.ReportesContenido.AsNoTracking()
            .Where(otra => otra.ReporteContenidoId != laDenunciaId && otra.RecetaId == laRecetaId && otra.ComentarioId == elComentarioId)
            .OrderBy(otra => otra.Fecha)
            .Select(otra => new OtraDenunciaDto(otra.ReporteContenidoId, otra.Estado, otra.Motivo, otra.Fecha,
                otra.Usuario.Nombre + " " + otra.Usuario.Apellidos))
            .ToListAsync(cancelacion);
        var elHistorial = await ProyecteElHistorial(laBaseDeDatos.Moderaciones.AsNoTracking()
                .Where(accion => accion.RecetaId == laRecetaId && accion.ComentarioId == elComentarioId).OrderBy(accion => accion.Fecha))
            .ToListAsync(cancelacion);

        static PersonaDto ConviertaALaPersona(Usuario elUsuario) => new(elUsuario.UsuarioId, elUsuario.Nombre + " " + elUsuario.Apellidos);
        return new ReporteDetalleDto(laDenuncia.ReporteContenidoId, laDenuncia.Estado, laDenuncia.Motivo, laDenuncia.Descripcion, laDenuncia.Fecha,
            ConviertaALaPersona(laDenuncia.Usuario), laDenuncia.Resolucion,
            laDenuncia.AtendidoPorNavigation is null ? null : laDenuncia.AtendidoPorNavigation.Nombre + " " + laDenuncia.AtendidoPorNavigation.Apellidos,
            laDenuncia.FechaAtencion, laDenuncia.RecetaId != null ? "RECETA" : "COMENTARIO",
            laDenuncia.Receta is null
                ? null
                : new RecetaModDto(laDenuncia.Receta.RecetaId, laDenuncia.Receta.Titulo, laDenuncia.Receta.Estado,
                    laDenuncia.Receta.Descripcion, ConviertaALaPersona(laDenuncia.Receta.Usuario)),
            laDenuncia.Comentario is null
                ? null
                : new ComentarioModDto(laDenuncia.Comentario.ComentarioId, laDenuncia.Comentario.RecetaId, laDenuncia.Comentario.Receta.Titulo,
                    laDenuncia.Comentario.Calificacion, laDenuncia.Comentario.Comentario1, laDenuncia.Comentario.Visible,
                    ConviertaALaPersona(laDenuncia.Comentario.Usuario)),
            lasOtras, elHistorial);
    }

    public async Task<string?> BloqueeLaDenunciaYObtengaSuEstado(int laDenunciaId, CancellationToken cancelacion)
    {
        var losEstados = await laBaseDeDatos.Database.SqlQuery<string>(
            $"SELECT estado AS \"Value\" FROM reportes_contenido WHERE reporte_contenido_id = {laDenunciaId} FOR UPDATE").ToListAsync(cancelacion);
        return losEstados.Count == 0 ? null : losEstados[0];
    }

    public Task<ContenidoDenunciado> BusqueElContenidoDenunciado(int laDenunciaId, CancellationToken cancelacion) =>
        laBaseDeDatos.ReportesContenido.AsNoTracking().Where(denuncia => denuncia.ReporteContenidoId == laDenunciaId)
            .Select(denuncia => new ContenidoDenunciado(denuncia.RecetaId, denuncia.ComentarioId)).FirstAsync(cancelacion);

    public Task MarqueLaDenuncia(int laDenunciaId, string elEstado, string laResolucion, int elAdminId, DateTime laFecha,
        CancellationToken cancelacion) =>
        laBaseDeDatos.ReportesContenido.Where(denuncia => denuncia.ReporteContenidoId == laDenunciaId)
            .ExecuteUpdateAsync(cambios => cambios
                .SetProperty(denuncia => denuncia.Estado, elEstado).SetProperty(denuncia => denuncia.Resolucion, laResolucion)
                .SetProperty(denuncia => denuncia.AtendidoPor, elAdminId).SetProperty(denuncia => denuncia.FechaAtencion, laFecha), cancelacion);

    public Task<int> CierreLasDemasDenunciasPendientes(int laDenunciaId, ContenidoDenunciado elContenido, string laResolucion,
        int elAdminId, DateTime laFecha, CancellationToken cancelacion)
    {
        var laRecetaId = elContenido.RecetaId;
        var elComentarioId = elContenido.ComentarioId;
        return laBaseDeDatos.ReportesContenido
            .Where(denuncia => denuncia.Estado == EstadosDeLaDenuncia.PENDIENTE && denuncia.ReporteContenidoId != laDenunciaId
                               && denuncia.RecetaId == laRecetaId && denuncia.ComentarioId == elComentarioId)
            .ExecuteUpdateAsync(cambios => cambios
                .SetProperty(denuncia => denuncia.Estado, EstadosDeLaDenuncia.MODERADO).SetProperty(denuncia => denuncia.Resolucion, laResolucion)
                .SetProperty(denuncia => denuncia.AtendidoPor, elAdminId).SetProperty(denuncia => denuncia.FechaAtencion, laFecha), cancelacion);
    }

    public Task<bool> LaRecetaNoEstaModerada(int laRecetaId, CancellationToken cancelacion) =>
        laBaseDeDatos.Recetas.AnyAsync(receta => receta.RecetaId == laRecetaId && receta.Estado != EstadosDeLaReceta.MODERADA, cancelacion);

    public async Task<bool> OculteElComentarioVisible(int elComentarioId, CancellationToken cancelacion) =>
        await CambieLaVisibilidadDelComentario(elComentarioId, eraVisible: true, cancelacion);

    public async Task<bool> CambieLaVisibilidadDelComentario(int elComentarioId, bool eraVisible, CancellationToken cancelacion) =>
        await laBaseDeDatos.Comentarios.Where(comentario => comentario.ComentarioId == elComentarioId && comentario.Visible == eraVisible)
            .ExecuteUpdateAsync(cambios => cambios.SetProperty(comentario => comentario.Visible, !eraVisible), cancelacion) > 0;

    public Task<bool> ExisteElComentario(int elComentarioId, CancellationToken cancelacion) =>
        laBaseDeDatos.Comentarios.AnyAsync(comentario => comentario.ComentarioId == elComentarioId, cancelacion);

    public async Task AgregueLaAccionDeModeracion(int elAdminId, int? laRecetaId, int? elComentarioId, int? laDenunciaId, string laAccion,
        string elMotivo, CancellationToken cancelacion)
    {
        laBaseDeDatos.Moderaciones.Add(new Moderacion
        {
            AdminId = elAdminId,
            RecetaId = laRecetaId,
            ComentarioId = elComentarioId,
            ReporteContenidoId = laDenunciaId,
            Accion = laAccion,
            Motivo = elMotivo
        });
        await laBaseDeDatos.SaveChangesAsync(cancelacion);
    }

    public Task<Paginado<HistorialDto>> ListeElHistorial(int? laRecetaId, int? elComentarioId, DateTime? desdeUtc, DateTime? hastaUtcExclusivo,
        SolicitudDePagina laPagina, CancellationToken cancelacion)
    {
        var laConsulta = laBaseDeDatos.Moderaciones.AsNoTracking().AsQueryable();
        if (laRecetaId is not null) laConsulta = laConsulta.Where(accion => accion.RecetaId == laRecetaId);
        if (elComentarioId is not null) laConsulta = laConsulta.Where(accion => accion.ComentarioId == elComentarioId);
        if (desdeUtc is not null) laConsulta = laConsulta.Where(accion => accion.Fecha >= desdeUtc);
        if (hastaUtcExclusivo is not null) laConsulta = laConsulta.Where(accion => accion.Fecha < hastaUtcExclusivo);
        return ProyecteElHistorial(laConsulta.OrderByDescending(accion => accion.Fecha).ThenByDescending(accion => accion.ModeracionId))
            .PagineLaConsulta(laPagina, cancelacion);
    }

    private static IQueryable<HistorialDto> ProyecteElHistorial(IQueryable<Moderacion> lasAcciones) =>
        lasAcciones.Select(accion => new HistorialDto(accion.ModeracionId, accion.Fecha, accion.Admin.Nombre + " " + accion.Admin.Apellidos,
            accion.Accion, accion.Motivo, accion.RecetaId, accion.Receta != null ? accion.Receta.Titulo : null, accion.ComentarioId,
            accion.ReporteContenidoId));
}
