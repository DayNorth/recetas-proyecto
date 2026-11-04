namespace Recetas.Api.Aplicacion.Solicitudes;

public record ConsultaDelCatalogo(string? Texto, short? CategoriaId, short? DificultadId, int? TiempoMaximo,
    int[]? Ingredientes, string? Orden, bool Registrar, int? Pagina, int? Tamano);

public class ConsultaDeReporte
{
    public DateOnly? Inicio { get; set; }
    public DateOnly? Fin { get; set; }
    public string? MesInicio { get; set; }
    public string? MesFin { get; set; }
    public int? Limite { get; set; }
    public int? MinVotos { get; set; }
    public int? MinVisitas { get; set; }
    public string? Formato { get; set; }
}

public record DatosDeLaNoticia(string Titulo, string Contenido, bool? Publicada, Puertos.ArchivoSubido? Imagen, bool QuitarImagen);
