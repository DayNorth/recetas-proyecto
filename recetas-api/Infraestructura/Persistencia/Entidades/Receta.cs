using System;
using System.Collections.Generic;

namespace Recetas.Api.Infraestructura.Persistencia.Entidades;

public partial class Receta
{
    public int RecetaId { get; set; }

    public int UsuarioId { get; set; }

    public short CategoriaId { get; set; }

    public short DificultadId { get; set; }

    public string Titulo { get; set; } = null!;

    public string? Descripcion { get; set; }

    public short TiempoPrepMin { get; set; }

    public short PorcionesBase { get; set; }

    public string Origen { get; set; } = null!;

    public string? ApiReferenciaId { get; set; }

    public string Estado { get; set; } = null!;

    public DateTime FechaCreacion { get; set; }

    public DateTime? FechaActualizacion { get; set; }

    public virtual Categoria Categoria { get; set; } = null!;

    public virtual ICollection<Comentario> Comentarios { get; set; } = new List<Comentario>();

    public virtual ICollection<Compartido> Compartidos { get; set; } = new List<Compartido>();

    public virtual NivelDificultad Dificultad { get; set; } = null!;

    public virtual ICollection<Favorito> Favoritos { get; set; } = new List<Favorito>();

    public virtual ICollection<ListaCompraItem> ListaCompraItems { get; set; } = new List<ListaCompraItem>();

    public virtual ICollection<MeGusta> MeGusta { get; set; } = new List<MeGusta>();

    public virtual ICollection<Moderacion> Moderaciones { get; set; } = new List<Moderacion>();

    public virtual ICollection<RecetaIngrediente> RecetaIngredientes { get; set; } = new List<RecetaIngrediente>();

    public virtual ICollection<RecetaMultimedia> Multimedia { get; set; } = new List<RecetaMultimedia>();

    public virtual ICollection<RecetaPaso> RecetaPasos { get; set; } = new List<RecetaPaso>();

    public virtual ICollection<ReporteContenido> ReportesContenido { get; set; } = new List<ReporteContenido>();

    public virtual Usuario Usuario { get; set; } = null!;

    public virtual ICollection<VisitaReceta> VisitasReceta { get; set; } = new List<VisitaReceta>();
}
