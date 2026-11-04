using System;
using System.Collections.Generic;

namespace Recetas.Api.Infraestructura.Persistencia.Entidades;

public partial class Usuario
{
    public int UsuarioId { get; set; }

    public short RolId { get; set; }

    public string Nombre { get; set; } = null!;

    public string Apellidos { get; set; } = null!;

    public string Correo { get; set; } = null!;

    public string PasswordHash { get; set; } = null!;

    public string? FotoPerfil { get; set; }

    public string? Descripcion { get; set; }

    public bool Activo { get; set; }

    public DateTime FechaRegistro { get; set; }

    public DateTime? UltimoAcceso { get; set; }

    public virtual ICollection<Busqueda> Busquedas { get; set; } = new List<Busqueda>();

    public virtual ICollection<Comentario> Comentarios { get; set; } = new List<Comentario>();

    public virtual ICollection<Compartido> Compartidos { get; set; } = new List<Compartido>();

    public virtual ICollection<Favorito> Favoritos { get; set; } = new List<Favorito>();

    public virtual ICollection<ListaCompra> ListasCompra { get; set; } = new List<ListaCompra>();

    public virtual ICollection<MeGusta> MeGusta { get; set; } = new List<MeGusta>();

    public virtual ICollection<Moderacion> Moderaciones { get; set; } = new List<Moderacion>();

    public virtual ICollection<Noticia> Noticias { get; set; } = new List<Noticia>();

    public virtual ICollection<Receta> Recetas { get; set; } = new List<Receta>();

    public virtual ICollection<ReporteContenido> ReporteContenidoAtendidoPorNavigationes { get; set; } = new List<ReporteContenido>();

    public virtual ICollection<ReporteContenido> ReporteContenidoUsuarios { get; set; } = new List<ReporteContenido>();

    public virtual ICollection<ReporteGenerado> ReportesGenerados { get; set; } = new List<ReporteGenerado>();

    public virtual Rol Rol { get; set; } = null!;

    public virtual ICollection<VisitaReceta> VisitasReceta { get; set; } = new List<VisitaReceta>();
}
