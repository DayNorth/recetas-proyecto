using Recetas.Api.Aplicacion.Comun;
using Recetas.Api.Aplicacion.Dtos;
using Recetas.Api.Aplicacion.Puertos;
using Recetas.Api.Aplicacion.Solicitudes;
using Recetas.Api.Dominio;

namespace Recetas.Api.Aplicacion.Servicios;

public class ServicioDeAdministracion(IRepositorioDeCatalogos losCatalogos, IRepositorioDeUsuarios losUsuarios,
    IUsuarioActual elUsuarioActual, IEjecutorDeTransacciones elEjecutor)
{

    public Task<IReadOnlyList<CategoriaAdminDto>> ListeLasCategorias(CancellationToken cancelacion) =>
        losCatalogos.ListeLasCategoriasParaAdministracion(cancelacion);

    public async Task<CategoriaAdminDto> CreeLaCategoria(CategoriaRequest laSolicitud, CancellationToken cancelacion)
    {
        var elNombre = Texto.NormaliceElTexto(laSolicitud.Nombre);
        await ExijaQueElNombreEsteLibre(elNombre, null, cancelacion);
        var elId = await losCatalogos.AgregueLaCategoria(elNombre, Texto.RecorteOMarqueNulo(laSolicitud.Descripcion), cancelacion);
        return await ObtengaLaCategoria(elId, cancelacion);
    }

    public async Task<CategoriaAdminDto> EditeLaCategoria(int laCategoriaId, CategoriaRequest laSolicitud, CancellationToken cancelacion)
    {
        _ = await losCatalogos.BusqueLaCategoriaParaAdministracion(laCategoriaId, cancelacion)
            ?? throw ExcepcionDeNegocio.PorRecursoInexistente("La categoría no existe.");
        var elNombre = Texto.NormaliceElTexto(laSolicitud.Nombre);
        await ExijaQueElNombreEsteLibre(elNombre, laCategoriaId, cancelacion);
        await losCatalogos.ActualiceLaCategoria(laCategoriaId, elNombre, Texto.RecorteOMarqueNulo(laSolicitud.Descripcion), cancelacion);
        return await ObtengaLaCategoria(laCategoriaId, cancelacion);
    }

    public async Task<CategoriaAdminDto> CambieElEstadoDeLaCategoria(int laCategoriaId, bool estaActiva, CancellationToken cancelacion)
    {
        if (!await losCatalogos.CambieElEstadoDeLaCategoria(laCategoriaId, estaActiva, cancelacion))
            throw ExcepcionDeNegocio.PorRecursoInexistente("La categoría no existe.");
        return await ObtengaLaCategoria(laCategoriaId, cancelacion);
    }

    private async Task<CategoriaAdminDto> ObtengaLaCategoria(int laCategoriaId, CancellationToken cancelacion) =>
        await losCatalogos.BusqueLaCategoriaParaAdministracion(laCategoriaId, cancelacion)
        ?? throw ExcepcionDeNegocio.PorRecursoInexistente("La categoría no existe.");

    private async Task ExijaQueElNombreEsteLibre(string elNombre, int? laCategoriaExcluidaId, CancellationToken cancelacion)
    {
        if (await losCatalogos.ExisteOtraCategoriaConElNombre(elNombre, laCategoriaExcluidaId, cancelacion))
            throw ExcepcionDeNegocio.PorConflicto("Ya existe una categoría con ese nombre.");
    }

    public Task<Paginado<UsuarioAdminDto>> ListeLosUsuarios(string? elTexto, string? elEstado, string? elRol, int? laPagina,
        int? elTamano, CancellationToken cancelacion)
    {
        bool? estaActivo = null;
        if (!string.IsNullOrWhiteSpace(elEstado))
        {
            estaActivo = elEstado.Trim().ToLowerInvariant() switch
            {
                "activo" => true,
                "suspendido" => false,
                _ => throw ExcepcionDeNegocio.PorSolicitudInvalida("Estado inválido.", "Use activo o suspendido.")
            };
        }

        string? elRolNormalizado = null;
        if (!string.IsNullOrWhiteSpace(elRol))
        {
            elRolNormalizado = elRol.Trim().ToUpperInvariant();
            if (elRolNormalizado is not (Roles.ADMIN or Roles.USUARIO))
                throw ExcepcionDeNegocio.PorSolicitudInvalida("Rol inválido.", "Use ADMIN o USUARIO.");
        }

        var elTextoBuscado = string.IsNullOrWhiteSpace(elTexto) ? null : elTexto.Trim();
        return losUsuarios.ListeLosUsuarios(new FiltroDeUsuarios(elTextoBuscado, estaActivo, elRolNormalizado),
            SolicitudDePagina.Normalice(laPagina, elTamano), cancelacion);
    }

    public async Task<UsuarioAdminDto> CambieElEstadoDelUsuario(int elUsuarioId, bool estaActivo, CancellationToken cancelacion)
    {
        if (!estaActivo && elUsuarioId == elUsuarioActual.IdRequerido)
            throw ExcepcionDeNegocio.PorConflicto("No puede suspender su propia cuenta.");

        await elEjecutor.EjecuteEnTransaccion(async laCancelacion =>
        {
            await losUsuarios.BloqueeALosAdministradoresActivos(laCancelacion);
            var laCuenta = await losUsuarios.BusqueElEstadoDeLaCuenta(elUsuarioId, laCancelacion)
                           ?? throw ExcepcionDeNegocio.PorRecursoInexistente("El usuario no existe.");

            var seSuspendeUnAdministradorActivo = !estaActivo && laCuenta.Activo && laCuenta.Rol == Roles.ADMIN;
            if (seSuspendeUnAdministradorActivo && await losUsuarios.CuenteLosAdministradoresActivos(laCancelacion) <= 1)
                throw ExcepcionDeNegocio.PorConflicto("No se puede suspender al último administrador activo.");

            await losUsuarios.CambieElEstadoDeLaCuenta(elUsuarioId, estaActivo, laCancelacion);
        }, cancelacion);

        return await losUsuarios.BusqueAlUsuarioParaAdministracion(elUsuarioId, cancelacion)
               ?? throw ExcepcionDeNegocio.PorRecursoInexistente("El usuario no existe.");
    }
}
