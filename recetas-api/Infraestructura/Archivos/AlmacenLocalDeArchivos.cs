using Recetas.Api.Aplicacion.Puertos;
using Recetas.Api.Dominio;

namespace Recetas.Api.Infraestructura.Archivos;

public enum TipoDeArchivo { ImagenJpg, ImagenPng, ImagenWebp, Pdf }

public class OpcionesDeAlmacenamiento
{
    public string Path { get; set; } = "uploads";
}

public class AlmacenLocalDeArchivos(OpcionesDeAlmacenamiento lasOpciones, IHostEnvironment elEntorno) : IAlmacenDeArchivos
{
    private const int TAMANO_DEL_BUFFER = 81920;
    private const int BYTES_DE_LA_FIRMA = 16;

    public string RutaRaiz { get; } = System.IO.Path.GetFullPath(
        System.IO.Path.IsPathRooted(lasOpciones.Path)
            ? lasOpciones.Path
            : System.IO.Path.Combine(elEntorno.ContentRootPath, lasOpciones.Path));

    public async Task<string> GuardeElArchivo(Stream elContenido, string laCarpeta, bool permitirImagenes, bool permitirPdf,
        long maximoDeBytes, CancellationToken cancelacion = default)
    {
        if (laCarpeta.Contains("..") || laCarpeta.Contains('/') || laCarpeta.Contains('\\'))
            throw new ArgumentException("Carpeta inválida.", nameof(laCarpeta));

        using var laMemoria = new MemoryStream();
        var elBuffer = new byte[TAMANO_DEL_BUFFER];
        int cuantosBytesSeLeyeron;
        while ((cuantosBytesSeLeyeron = await elContenido.ReadAsync(elBuffer, cancelacion)) > 0)
        {
            laMemoria.Write(elBuffer, 0, cuantosBytesSeLeyeron);
            if (laMemoria.Length > maximoDeBytes)
                throw ExcepcionDeNegocio.PorSolicitudInvalida($"El archivo supera el máximo de {maximoDeBytes / (1024 * 1024)} MB.");
        }
        if (laMemoria.Length == 0) throw ExcepcionDeNegocio.PorSolicitudInvalida("El archivo está vacío.");

        var elTipo = DetecteElTipo(laMemoria.GetBuffer().AsSpan(0, (int)Math.Min(laMemoria.Length, BYTES_DE_LA_FIRMA)));
        var laExtension = elTipo switch
        {
            TipoDeArchivo.ImagenJpg when permitirImagenes => ".jpg",
            TipoDeArchivo.ImagenPng when permitirImagenes => ".png",
            TipoDeArchivo.ImagenWebp when permitirImagenes => ".webp",
            TipoDeArchivo.Pdf when permitirPdf => ".pdf",
            _ => throw ExcepcionDeNegocio.PorSolicitudInvalida("Tipo de archivo no permitido.",
                permitirPdf ? "Use JPG, PNG, WebP o PDF." : "Use JPG, PNG o WebP.")
        };

        var laCarpetaFisica = System.IO.Path.Combine(RutaRaiz, laCarpeta);
        Directory.CreateDirectory(laCarpetaFisica);
        var elNombre = $"{Guid.NewGuid():N}{laExtension}";
        await File.WriteAllBytesAsync(System.IO.Path.Combine(laCarpetaFisica, elNombre), laMemoria.ToArray(), cancelacion);
        return $"{laCarpeta}/{elNombre}";
    }

    public void BorreElArchivo(string? laRutaRelativa)
    {
        if (string.IsNullOrWhiteSpace(laRutaRelativa)) return;
        var laRutaCompleta = System.IO.Path.GetFullPath(System.IO.Path.Combine(RutaRaiz, laRutaRelativa));
        if (!laRutaCompleta.StartsWith(RutaRaiz, StringComparison.OrdinalIgnoreCase)) return;
        try { if (File.Exists(laRutaCompleta)) File.Delete(laRutaCompleta); }
        catch (IOException) { /* queda para la limpieza */ }
    }

    internal static TipoDeArchivo? DetecteElTipo(ReadOnlySpan<byte> losBytes)
    {
        if (losBytes.Length >= 3 && losBytes[0] == 0xFF && losBytes[1] == 0xD8 && losBytes[2] == 0xFF) return TipoDeArchivo.ImagenJpg;
        if (losBytes.Length >= 8 && losBytes[0] == 0x89 && losBytes[1] == 0x50 && losBytes[2] == 0x4E && losBytes[3] == 0x47 &&
            losBytes[4] == 0x0D && losBytes[5] == 0x0A && losBytes[6] == 0x1A && losBytes[7] == 0x0A) return TipoDeArchivo.ImagenPng;
        if (losBytes.Length >= 12 && losBytes[0] == 'R' && losBytes[1] == 'I' && losBytes[2] == 'F' && losBytes[3] == 'F' &&
            losBytes[8] == 'W' && losBytes[9] == 'E' && losBytes[10] == 'B' && losBytes[11] == 'P') return TipoDeArchivo.ImagenWebp;
        if (losBytes.Length >= 5 && losBytes[0] == '%' && losBytes[1] == 'P' && losBytes[2] == 'D' && losBytes[3] == 'F' && losBytes[4] == '-') return TipoDeArchivo.Pdf;
        return null;
    }
}
