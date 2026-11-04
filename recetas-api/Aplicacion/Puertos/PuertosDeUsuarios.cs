using Recetas.Api.Aplicacion.Comun;
using Recetas.Api.Aplicacion.Dtos;
using Recetas.Api.Aplicacion.Solicitudes;

namespace Recetas.Api.Aplicacion.Puertos;

public record CuentaDeUsuario(UsuarioDto Usuario, string PasswordHash, bool Activo);

public record EstadoDeCuenta(bool Activo, string Rol);

public interface IRepositorioDeUsuarios
{
    Task<bool> ExisteElCorreo(string elCorreo, CancellationToken cancelacion);

    Task<UsuarioDto> AgregueAlUsuario(string elNombre, string losApellidos, string elCorreo, string elHash,
        CancellationToken cancelacion);

    Task<CuentaDeUsuario?> BusqueLaCuentaPorCorreo(string elCorreo, CancellationToken cancelacion);

    Task RegistreElUltimoAcceso(int elUsuarioId, DateTime laFecha, CancellationToken cancelacion);

    Task<UsuarioDto?> BusqueAlUsuario(int elUsuarioId, CancellationToken cancelacion);

    Task<string?> BusqueElRolDeLaCuentaActiva(int elUsuarioId, CancellationToken cancelacion);

    Task<UsuarioDto> ActualiceElPerfil(int elUsuarioId, string elNombre, string losApellidos, string? laDescripcion,
        CancellationToken cancelacion);

    Task<(UsuarioDto Usuario, string? FotoAnterior)> ActualiceLaFoto(int elUsuarioId, string laRutaNueva,
        CancellationToken cancelacion);

    Task<Paginado<UsuarioAdminDto>> ListeLosUsuarios(FiltroDeUsuarios elFiltro, SolicitudDePagina laPagina,
        CancellationToken cancelacion);

    Task<UsuarioAdminDto?> BusqueAlUsuarioParaAdministracion(int elUsuarioId, CancellationToken cancelacion);

    Task BloqueeALosAdministradoresActivos(CancellationToken cancelacion);

    Task<EstadoDeCuenta?> BusqueElEstadoDeLaCuenta(int elUsuarioId, CancellationToken cancelacion);

    Task<int> CuenteLosAdministradoresActivos(CancellationToken cancelacion);

    Task CambieElEstadoDeLaCuenta(int elUsuarioId, bool estaActiva, CancellationToken cancelacion);
}
