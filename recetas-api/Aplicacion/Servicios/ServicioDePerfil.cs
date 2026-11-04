using Recetas.Api.Aplicacion.Dtos;
using Recetas.Api.Aplicacion.Puertos;
using Recetas.Api.Aplicacion.Solicitudes;
using Recetas.Api.Dominio;

namespace Recetas.Api.Aplicacion.Servicios;

public class ServicioDePerfil(IRepositorioDeUsuarios losUsuarios, IUsuarioActual elUsuarioActual, IAlmacenDeArchivos losArchivos)
{
    private const long MAXIMO_DE_BYTES_DE_LA_FOTO = 2 * 1024 * 1024;

    public async Task<UsuarioDto> ObtengaElPerfil(CancellationToken cancelacion) =>
        await losUsuarios.BusqueAlUsuario(elUsuarioActual.IdRequerido, cancelacion)
        ?? throw ExcepcionDeNegocio.PorFaltaDeAutenticacion("Sesión no válida.");

    public Task<UsuarioDto> ActualiceElPerfil(ActualizarPerfilRequest laSolicitud, CancellationToken cancelacion) =>
        losUsuarios.ActualiceElPerfil(elUsuarioActual.IdRequerido, laSolicitud.Nombre.Trim(), laSolicitud.Apellidos.Trim(),
            Texto.RecorteOMarqueNulo(laSolicitud.Descripcion), cancelacion);

    public async Task<UsuarioDto> CambieLaFoto(ArchivoSubido? laFoto, CancellationToken cancelacion)
    {
        if (laFoto is null || laFoto.Longitud == 0)
            throw ExcepcionDeNegocio.PorSolicitudInvalida("Debe enviar el archivo en el campo \"foto\".");

        await using var elFlujo = laFoto.AbraElFlujo();
        var laRutaNueva = await losArchivos.GuardeElArchivo(elFlujo, "perfiles", permitirImagenes: true, permitirPdf: false,
            MAXIMO_DE_BYTES_DE_LA_FOTO, cancelacion);

        UsuarioDto elUsuario;
        string? laFotoAnterior;
        try
        {
            (elUsuario, laFotoAnterior) = await losUsuarios.ActualiceLaFoto(elUsuarioActual.IdRequerido, laRutaNueva, cancelacion);
        }
        catch
        {
            losArchivos.BorreElArchivo(laRutaNueva);
            throw;
        }

        losArchivos.BorreElArchivo(laFotoAnterior);
        return elUsuario;
    }
}
