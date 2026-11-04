using Microsoft.EntityFrameworkCore;
using Recetas.Api.Aplicacion.Comun;
using Recetas.Api.Aplicacion.Dtos;
using Recetas.Api.Aplicacion.Puertos;
using Recetas.Api.Aplicacion.Solicitudes;
using Recetas.Api.Dominio;
using Recetas.Api.Infraestructura.Persistencia;
using Recetas.Api.Infraestructura.Persistencia.Entidades;

namespace Recetas.Api.Infraestructura.Repositorios;

public class RepositorioDeUsuarios(RecetasDbContext laBaseDeDatos) : IRepositorioDeUsuarios
{
    private static UsuarioDto ConviertaAlDto(Usuario elUsuario, string elRol) =>
        new(elUsuario.UsuarioId, elUsuario.Nombre, elUsuario.Apellidos, elUsuario.Correo, elRol, elUsuario.FotoPerfil,
            elUsuario.Descripcion, elUsuario.FechaRegistro);

    public Task<bool> ExisteElCorreo(string elCorreo, CancellationToken cancelacion) =>
        laBaseDeDatos.Usuarios.AnyAsync(usuario => usuario.Correo.ToLower() == elCorreo, cancelacion);

    public async Task<UsuarioDto> AgregueAlUsuario(string elNombre, string losApellidos, string elCorreo, string elHash,
        CancellationToken cancelacion)
    {
        var elRolId = await laBaseDeDatos.Roles.Where(rol => rol.Nombre == Roles.USUARIO).Select(rol => rol.RolId).SingleAsync(cancelacion);
        var elUsuario = new Usuario
        {
            RolId = elRolId,
            Nombre = elNombre,
            Apellidos = losApellidos,
            Correo = elCorreo,
            PasswordHash = elHash,
            Activo = true
        };
        laBaseDeDatos.Usuarios.Add(elUsuario);
        await laBaseDeDatos.SaveChangesAsync(cancelacion);
        return ConviertaAlDto(elUsuario, Roles.USUARIO);
    }

    public async Task<CuentaDeUsuario?> BusqueLaCuentaPorCorreo(string elCorreo, CancellationToken cancelacion)
    {
        var elUsuario = await laBaseDeDatos.Usuarios.AsNoTracking().Include(usuario => usuario.Rol)
            .FirstOrDefaultAsync(usuario => usuario.Correo.ToLower() == elCorreo, cancelacion);
        return elUsuario is null ? null : new CuentaDeUsuario(ConviertaAlDto(elUsuario, elUsuario.Rol.Nombre), elUsuario.PasswordHash, elUsuario.Activo);
    }

    public Task RegistreElUltimoAcceso(int elUsuarioId, DateTime laFecha, CancellationToken cancelacion) =>
        laBaseDeDatos.Usuarios.Where(usuario => usuario.UsuarioId == elUsuarioId)
            .ExecuteUpdateAsync(cambios => cambios.SetProperty(usuario => usuario.UltimoAcceso, laFecha), cancelacion);

    public async Task<UsuarioDto?> BusqueAlUsuario(int elUsuarioId, CancellationToken cancelacion)
    {
        var elUsuario = await laBaseDeDatos.Usuarios.AsNoTracking().Include(usuario => usuario.Rol)
            .FirstOrDefaultAsync(usuario => usuario.UsuarioId == elUsuarioId, cancelacion);
        return elUsuario is null ? null : ConviertaAlDto(elUsuario, elUsuario.Rol.Nombre);
    }

    public Task<string?> BusqueElRolDeLaCuentaActiva(int elUsuarioId, CancellationToken cancelacion) =>
        laBaseDeDatos.Usuarios.Where(usuario => usuario.UsuarioId == elUsuarioId && usuario.Activo)
            .Select(usuario => usuario.Rol.Nombre).FirstOrDefaultAsync(cancelacion);

    public async Task<UsuarioDto> ActualiceElPerfil(int elUsuarioId, string elNombre, string losApellidos, string? laDescripcion,
        CancellationToken cancelacion)
    {
        var elUsuario = await BusqueAlUsuarioConRol(elUsuarioId, cancelacion);
        elUsuario.Nombre = elNombre;
        elUsuario.Apellidos = losApellidos;
        elUsuario.Descripcion = laDescripcion;
        await laBaseDeDatos.SaveChangesAsync(cancelacion);
        return ConviertaAlDto(elUsuario, elUsuario.Rol.Nombre);
    }

    public async Task<(UsuarioDto Usuario, string? FotoAnterior)> ActualiceLaFoto(int elUsuarioId, string laRutaNueva,
        CancellationToken cancelacion)
    {
        var elUsuario = await BusqueAlUsuarioConRol(elUsuarioId, cancelacion);
        var laFotoAnterior = elUsuario.FotoPerfil;
        elUsuario.FotoPerfil = laRutaNueva;
        await laBaseDeDatos.SaveChangesAsync(cancelacion);
        return (ConviertaAlDto(elUsuario, elUsuario.Rol.Nombre), laFotoAnterior);
    }

    private Task<Usuario> BusqueAlUsuarioConRol(int elUsuarioId, CancellationToken cancelacion) =>
        laBaseDeDatos.Usuarios.Include(usuario => usuario.Rol).FirstAsync(usuario => usuario.UsuarioId == elUsuarioId, cancelacion);

    public Task<Paginado<UsuarioAdminDto>> ListeLosUsuarios(FiltroDeUsuarios elFiltro, SolicitudDePagina laPagina,
        CancellationToken cancelacion)
    {
        var laConsulta = laBaseDeDatos.Usuarios.AsNoTracking().AsQueryable();
        if (elFiltro.Texto is not null)
        {
            var elPatron = ExtensionesDeConsulta.ConstruyaElPatronDeContiene(elFiltro.Texto);
            laConsulta = laConsulta.Where(usuario => EF.Functions.ILike(usuario.Nombre + " " + usuario.Apellidos, elPatron, "\\")
                                                     || EF.Functions.ILike(usuario.Correo, elPatron, "\\"));
        }
        if (elFiltro.Activo is not null) laConsulta = laConsulta.Where(usuario => usuario.Activo == elFiltro.Activo);
        if (elFiltro.Rol is not null) laConsulta = laConsulta.Where(usuario => usuario.Rol.Nombre == elFiltro.Rol);

        return laConsulta.OrderBy(usuario => usuario.Apellidos).ThenBy(usuario => usuario.Nombre).ThenBy(usuario => usuario.UsuarioId)
            .Select(usuario => new UsuarioAdminDto(usuario.UsuarioId, usuario.Nombre, usuario.Apellidos, usuario.Correo,
                usuario.Rol.Nombre, usuario.Activo, usuario.FechaRegistro, usuario.UltimoAcceso))
            .PagineLaConsulta(laPagina, cancelacion);
    }

    public Task<UsuarioAdminDto?> BusqueAlUsuarioParaAdministracion(int elUsuarioId, CancellationToken cancelacion) =>
        laBaseDeDatos.Usuarios.AsNoTracking().Where(usuario => usuario.UsuarioId == elUsuarioId)
            .Select(usuario => new UsuarioAdminDto(usuario.UsuarioId, usuario.Nombre, usuario.Apellidos, usuario.Correo,
                usuario.Rol.Nombre, usuario.Activo, usuario.FechaRegistro, usuario.UltimoAcceso))
            .FirstOrDefaultAsync(cancelacion);

    public async Task BloqueeALosAdministradoresActivos(CancellationToken cancelacion) =>
        await laBaseDeDatos.Database.ExecuteSqlInterpolatedAsync(
            $"SELECT 1 FROM usuarios WHERE activo AND rol_id = (SELECT rol_id FROM roles WHERE nombre = 'ADMIN') FOR UPDATE", cancelacion);

    public Task<EstadoDeCuenta?> BusqueElEstadoDeLaCuenta(int elUsuarioId, CancellationToken cancelacion) =>
        laBaseDeDatos.Usuarios.AsNoTracking().Where(usuario => usuario.UsuarioId == elUsuarioId)
            .Select(usuario => new EstadoDeCuenta(usuario.Activo, usuario.Rol.Nombre))
            .FirstOrDefaultAsync(cancelacion);

    public Task<int> CuenteLosAdministradoresActivos(CancellationToken cancelacion) =>
        laBaseDeDatos.Usuarios.CountAsync(usuario => usuario.Activo && usuario.Rol.Nombre == Roles.ADMIN, cancelacion);

    public Task CambieElEstadoDeLaCuenta(int elUsuarioId, bool estaActiva, CancellationToken cancelacion) =>
        laBaseDeDatos.Usuarios.Where(usuario => usuario.UsuarioId == elUsuarioId)
            .ExecuteUpdateAsync(cambios => cambios.SetProperty(usuario => usuario.Activo, estaActiva), cancelacion);
}
