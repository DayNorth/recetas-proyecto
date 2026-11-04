using System;
using System.Collections.Generic;
using Microsoft.EntityFrameworkCore;
using Recetas.Api.Infraestructura.Persistencia.Entidades;

namespace Recetas.Api.Infraestructura.Persistencia;

public partial class RecetasDbContext : DbContext
{
    public RecetasDbContext(DbContextOptions<RecetasDbContext> options)
        : base(options)
    {
    }

    public virtual DbSet<Busqueda> Busquedas { get; set; }

    public virtual DbSet<Categoria> Categorias { get; set; }

    public virtual DbSet<Comentario> Comentarios { get; set; }

    public virtual DbSet<Compartido> Compartidos { get; set; }

    public virtual DbSet<Favorito> Favoritos { get; set; }

    public virtual DbSet<Ingrediente> Ingredientes { get; set; }

    public virtual DbSet<ListaCompra> ListasCompra { get; set; }

    public virtual DbSet<ListaCompraItem> ListaCompraItems { get; set; }

    public virtual DbSet<MeGusta> MeGusta { get; set; }

    public virtual DbSet<Moderacion> Moderaciones { get; set; }

    public virtual DbSet<NivelDificultad> NivelesDificultad { get; set; }

    public virtual DbSet<Noticia> Noticias { get; set; }

    public virtual DbSet<Receta> Recetas { get; set; }

    public virtual DbSet<RecetaIngrediente> RecetaIngredientes { get; set; }

    public virtual DbSet<RecetaMultimedia> RecetaMultimedia { get; set; }

    public virtual DbSet<RecetaPaso> RecetaPasos { get; set; }

    public virtual DbSet<RedSocial> RedesSociales { get; set; }

    public virtual DbSet<ReporteContenido> ReportesContenido { get; set; }

    public virtual DbSet<ReporteGenerado> ReportesGenerados { get; set; }

    public virtual DbSet<Rol> Roles { get; set; }

    public virtual DbSet<UnidadMedida> UnidadesMedida { get; set; }

    public virtual DbSet<Usuario> Usuarios { get; set; }

    public virtual DbSet<VisitaReceta> VisitasReceta { get; set; }

    public virtual DbSet<VwIngredientesMasUsado> VwIngredientesMasUsados { get; set; }

    public virtual DbSet<VwReportesContenidoPendiente> VwReportesContenidoPendientes { get; set; }

    public virtual DbSet<VwResumenReceta> VwResumenRecetas { get; set; }

    public virtual DbSet<VwTerminosMasBuscado> VwTerminosMasBuscados { get; set; }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<Busqueda>(entity =>
        {
            entity.HasKey(e => e.BusquedaId).HasName("busquedas_pkey");

            entity.ToTable("busquedas");

            entity.HasIndex(e => e.Fecha, "idx_busquedas_fecha");

            entity.HasIndex(e => e.Termino, "idx_busquedas_termino");

            entity.Property(e => e.BusquedaId).HasColumnName("busqueda_id");
            entity.Property(e => e.CantidadResultados).HasColumnName("cantidad_resultados");
            entity.Property(e => e.CategoriaId).HasColumnName("categoria_id");
            entity.Property(e => e.Fecha)
                .HasDefaultValueSql("CURRENT_TIMESTAMP")
                .HasColumnName("fecha");
            entity.Property(e => e.Termino)
                .HasMaxLength(100)
                .HasColumnName("termino");
            entity.Property(e => e.UsuarioId).HasColumnName("usuario_id");

            entity.HasOne(d => d.Categoria).WithMany(p => p.Busquedas)
                .HasForeignKey(d => d.CategoriaId)
                .OnDelete(DeleteBehavior.SetNull)
                .HasConstraintName("fk_busq_categoria");

            entity.HasOne(d => d.Usuario).WithMany(p => p.Busquedas)
                .HasForeignKey(d => d.UsuarioId)
                .OnDelete(DeleteBehavior.SetNull)
                .HasConstraintName("fk_busq_usuario");
        });

        modelBuilder.Entity<Categoria>(entity =>
        {
            entity.HasKey(e => e.CategoriaId).HasName("categorias_pkey");

            entity.ToTable("categorias");

            entity.HasIndex(e => e.Nombre, "categorias_nombre_key").IsUnique();

            entity.Property(e => e.CategoriaId).HasColumnName("categoria_id");
            entity.Property(e => e.Activo)
                .HasDefaultValue(true)
                .HasColumnName("activo");
            entity.Property(e => e.Descripcion)
                .HasMaxLength(200)
                .HasColumnName("descripcion");
            entity.Property(e => e.Nombre)
                .HasMaxLength(60)
                .HasColumnName("nombre");
        });

        modelBuilder.Entity<Comentario>(entity =>
        {
            entity.HasKey(e => e.ComentarioId).HasName("comentarios_pkey");

            entity.ToTable("comentarios");

            entity.HasIndex(e => e.Fecha, "idx_comentarios_fecha");

            entity.HasIndex(e => e.RecetaId, "idx_comentarios_receta");

            entity.HasIndex(e => new { e.UsuarioId, e.RecetaId }, "uq_com_usuario_receta").IsUnique();

            entity.Property(e => e.ComentarioId).HasColumnName("comentario_id");
            entity.Property(e => e.Calificacion).HasColumnName("calificacion");
            entity.Property(e => e.Comentario1)
                .HasMaxLength(500)
                .HasColumnName("comentario");
            entity.Property(e => e.Fecha)
                .HasDefaultValueSql("CURRENT_TIMESTAMP")
                .HasColumnName("fecha");
            entity.Property(e => e.RecetaId).HasColumnName("receta_id");
            entity.Property(e => e.UsuarioId).HasColumnName("usuario_id");
            entity.Property(e => e.Visible)
                .HasDefaultValue(true)
                .HasColumnName("visible");

            entity.HasOne(d => d.Receta).WithMany(p => p.Comentarios)
                .HasForeignKey(d => d.RecetaId)
                .HasConstraintName("fk_com_receta");

            entity.HasOne(d => d.Usuario).WithMany(p => p.Comentarios)
                .HasForeignKey(d => d.UsuarioId)
                .HasConstraintName("fk_com_usuario");
        });

        modelBuilder.Entity<Compartido>(entity =>
        {
            entity.HasKey(e => e.CompartidoId).HasName("compartidos_pkey");

            entity.ToTable("compartidos");

            entity.HasIndex(e => e.Fecha, "idx_compartidos_fecha");

            entity.HasIndex(e => e.RecetaId, "idx_compartidos_receta");

            entity.Property(e => e.CompartidoId).HasColumnName("compartido_id");
            entity.Property(e => e.Fecha)
                .HasDefaultValueSql("CURRENT_TIMESTAMP")
                .HasColumnName("fecha");
            entity.Property(e => e.RecetaId).HasColumnName("receta_id");
            entity.Property(e => e.RedId).HasColumnName("red_id");
            entity.Property(e => e.UsuarioId).HasColumnName("usuario_id");

            entity.HasOne(d => d.Receta).WithMany(p => p.Compartidos)
                .HasForeignKey(d => d.RecetaId)
                .HasConstraintName("fk_comp_receta");

            entity.HasOne(d => d.Red).WithMany(p => p.Compartidos)
                .HasForeignKey(d => d.RedId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("fk_comp_red");

            entity.HasOne(d => d.Usuario).WithMany(p => p.Compartidos)
                .HasForeignKey(d => d.UsuarioId)
                .OnDelete(DeleteBehavior.SetNull)
                .HasConstraintName("fk_comp_usuario");
        });

        modelBuilder.Entity<Favorito>(entity =>
        {
            entity.HasKey(e => new { e.UsuarioId, e.RecetaId }).HasName("favoritos_pkey");

            entity.ToTable("favoritos");

            entity.HasIndex(e => e.Fecha, "idx_favoritos_fecha");

            entity.HasIndex(e => e.RecetaId, "idx_favoritos_receta");

            entity.Property(e => e.UsuarioId).HasColumnName("usuario_id");
            entity.Property(e => e.RecetaId).HasColumnName("receta_id");
            entity.Property(e => e.Fecha)
                .HasDefaultValueSql("CURRENT_TIMESTAMP")
                .HasColumnName("fecha");

            entity.HasOne(d => d.Receta).WithMany(p => p.Favoritos)
                .HasForeignKey(d => d.RecetaId)
                .HasConstraintName("fk_fav_receta");

            entity.HasOne(d => d.Usuario).WithMany(p => p.Favoritos)
                .HasForeignKey(d => d.UsuarioId)
                .HasConstraintName("fk_fav_usuario");
        });

        modelBuilder.Entity<Ingrediente>(entity =>
        {
            entity.HasKey(e => e.IngredienteId).HasName("ingredientes_pkey");

            entity.ToTable("ingredientes");

            entity.HasIndex(e => e.Nombre, "ingredientes_nombre_key").IsUnique();

            entity.Property(e => e.IngredienteId).HasColumnName("ingrediente_id");
            entity.Property(e => e.Nombre)
                .HasMaxLength(80)
                .HasColumnName("nombre");
        });

        modelBuilder.Entity<ListaCompra>(entity =>
        {
            entity.HasKey(e => e.ListaId).HasName("listas_compras_pkey");

            entity.ToTable("listas_compras");

            entity.HasIndex(e => e.UsuarioId, "idx_listas_usuario");

            entity.Property(e => e.ListaId).HasColumnName("lista_id");
            entity.Property(e => e.Estado)
                .HasMaxLength(10)
                .HasDefaultValueSql("'ACTIVA'::character varying")
                .HasColumnName("estado");
            entity.Property(e => e.FechaCreacion)
                .HasDefaultValueSql("CURRENT_TIMESTAMP")
                .HasColumnName("fecha_creacion");
            entity.Property(e => e.Nombre)
                .HasMaxLength(80)
                .HasDefaultValueSql("'Mi lista de compras'::character varying")
                .HasColumnName("nombre");
            entity.Property(e => e.UsuarioId).HasColumnName("usuario_id");

            entity.HasOne(d => d.Usuario).WithMany(p => p.ListasCompra)
                .HasForeignKey(d => d.UsuarioId)
                .HasConstraintName("fk_lista_usuario");
        });

        modelBuilder.Entity<ListaCompraItem>(entity =>
        {
            entity.HasKey(e => e.ItemId).HasName("lista_compra_items_pkey");

            entity.ToTable("lista_compra_items");

            entity.HasIndex(e => e.ListaId, "idx_items_lista");

            entity.Property(e => e.ItemId).HasColumnName("item_id");
            entity.Property(e => e.Cantidad)
                .HasPrecision(8, 2)
                .HasColumnName("cantidad");
            entity.Property(e => e.Comprado).HasColumnName("comprado");
            entity.Property(e => e.IngredienteId).HasColumnName("ingrediente_id");
            entity.Property(e => e.ListaId).HasColumnName("lista_id");
            entity.Property(e => e.RecetaOrigenId).HasColumnName("receta_origen_id");
            entity.Property(e => e.UnidadId).HasColumnName("unidad_id");

            entity.HasOne(d => d.Ingrediente).WithMany(p => p.ListaCompraItems)
                .HasForeignKey(d => d.IngredienteId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("fk_item_ingrediente");

            entity.HasOne(d => d.Lista).WithMany(p => p.ListaCompraItems)
                .HasForeignKey(d => d.ListaId)
                .HasConstraintName("fk_item_lista");

            entity.HasOne(d => d.RecetaOrigen).WithMany(p => p.ListaCompraItems)
                .HasForeignKey(d => d.RecetaOrigenId)
                .OnDelete(DeleteBehavior.SetNull)
                .HasConstraintName("fk_item_receta");

            entity.HasOne(d => d.Unidad).WithMany(p => p.ListaCompraItems)
                .HasForeignKey(d => d.UnidadId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("fk_item_unidad");
        });

        modelBuilder.Entity<MeGusta>(entity =>
        {
            entity.HasKey(e => new { e.UsuarioId, e.RecetaId }).HasName("me_gusta_pkey");

            entity.ToTable("me_gusta");

            entity.HasIndex(e => e.Fecha, "idx_megusta_fecha");

            entity.HasIndex(e => e.RecetaId, "idx_megusta_receta");

            entity.Property(e => e.UsuarioId).HasColumnName("usuario_id");
            entity.Property(e => e.RecetaId).HasColumnName("receta_id");
            entity.Property(e => e.Fecha)
                .HasDefaultValueSql("CURRENT_TIMESTAMP")
                .HasColumnName("fecha");

            entity.HasOne(d => d.Receta).WithMany(p => p.MeGusta)
                .HasForeignKey(d => d.RecetaId)
                .HasConstraintName("fk_like_receta");

            entity.HasOne(d => d.Usuario).WithMany(p => p.MeGusta)
                .HasForeignKey(d => d.UsuarioId)
                .HasConstraintName("fk_like_usuario");
        });

        modelBuilder.Entity<Moderacion>(entity =>
        {
            entity.HasKey(e => e.ModeracionId).HasName("moderaciones_pkey");

            entity.ToTable("moderaciones");

            entity.HasIndex(e => e.ComentarioId, "idx_mod_comentario").HasFilter("(comentario_id IS NOT NULL)");

            entity.HasIndex(e => e.Fecha, "idx_mod_fecha");

            entity.HasIndex(e => e.RecetaId, "idx_mod_receta").HasFilter("(receta_id IS NOT NULL)");

            entity.Property(e => e.ModeracionId).HasColumnName("moderacion_id");
            entity.Property(e => e.Accion)
                .HasMaxLength(10)
                .HasColumnName("accion");
            entity.Property(e => e.AdminId).HasColumnName("admin_id");
            entity.Property(e => e.ComentarioId).HasColumnName("comentario_id");
            entity.Property(e => e.Fecha)
                .HasDefaultValueSql("CURRENT_TIMESTAMP")
                .HasColumnName("fecha");
            entity.Property(e => e.Motivo)
                .HasMaxLength(500)
                .HasColumnName("motivo");
            entity.Property(e => e.RecetaId).HasColumnName("receta_id");
            entity.Property(e => e.ReporteContenidoId).HasColumnName("reporte_contenido_id");

            entity.HasOne(d => d.Admin).WithMany(p => p.Moderaciones)
                .HasForeignKey(d => d.AdminId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("fk_mod_admin");

            entity.HasOne(d => d.Comentario).WithMany(p => p.Moderaciones)
                .HasForeignKey(d => d.ComentarioId)
                .OnDelete(DeleteBehavior.Cascade)
                .HasConstraintName("fk_mod_comentario");

            entity.HasOne(d => d.Receta).WithMany(p => p.Moderaciones)
                .HasForeignKey(d => d.RecetaId)
                .OnDelete(DeleteBehavior.Cascade)
                .HasConstraintName("fk_mod_receta");

            entity.HasOne(d => d.ReporteContenido).WithMany(p => p.Moderaciones)
                .HasForeignKey(d => d.ReporteContenidoId)
                .OnDelete(DeleteBehavior.SetNull)
                .HasConstraintName("fk_mod_reporte");
        });

        modelBuilder.Entity<NivelDificultad>(entity =>
        {
            entity.HasKey(e => e.DificultadId).HasName("niveles_dificultad_pkey");

            entity.ToTable("niveles_dificultad");

            entity.HasIndex(e => e.Nombre, "niveles_dificultad_nombre_key").IsUnique();

            entity.Property(e => e.DificultadId).HasColumnName("dificultad_id");
            entity.Property(e => e.Nombre)
                .HasMaxLength(20)
                .HasColumnName("nombre");
        });

        modelBuilder.Entity<Noticia>(entity =>
        {
            entity.HasKey(e => e.NoticiaId).HasName("noticias_pkey");

            entity.ToTable("noticias");

            entity.Property(e => e.NoticiaId).HasColumnName("noticia_id");
            entity.Property(e => e.Contenido).HasColumnName("contenido");
            entity.Property(e => e.FechaPublicacion)
                .HasDefaultValueSql("CURRENT_TIMESTAMP")
                .HasColumnName("fecha_publicacion");
            entity.Property(e => e.Imagen)
                .HasMaxLength(255)
                .HasColumnName("imagen");
            entity.Property(e => e.Publicada)
                .HasDefaultValue(true)
                .HasColumnName("publicada");
            entity.Property(e => e.Titulo)
                .HasMaxLength(150)
                .HasColumnName("titulo");
            entity.Property(e => e.UsuarioId).HasColumnName("usuario_id");

            entity.HasOne(d => d.Usuario).WithMany(p => p.Noticias)
                .HasForeignKey(d => d.UsuarioId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("fk_noticia_usuario");
        });

        modelBuilder.Entity<Receta>(entity =>
        {
            entity.HasKey(e => e.RecetaId).HasName("recetas_pkey");

            entity.ToTable("recetas");

            entity.HasIndex(e => e.CategoriaId, "idx_recetas_categoria");

            entity.HasIndex(e => e.Titulo, "idx_recetas_titulo");

            entity.HasIndex(e => e.ApiReferenciaId, "uq_recetas_api").IsUnique();

            entity.Property(e => e.RecetaId).HasColumnName("receta_id");
            entity.Property(e => e.ApiReferenciaId)
                .HasMaxLength(50)
                .HasColumnName("api_referencia_id");
            entity.Property(e => e.CategoriaId).HasColumnName("categoria_id");
            entity.Property(e => e.Descripcion).HasColumnName("descripcion");
            entity.Property(e => e.DificultadId).HasColumnName("dificultad_id");
            entity.Property(e => e.Estado)
                .HasMaxLength(10)
                .HasDefaultValueSql("'PUBLICADA'::character varying")
                .HasColumnName("estado");
            entity.Property(e => e.FechaActualizacion).HasColumnName("fecha_actualizacion");
            entity.Property(e => e.FechaCreacion)
                .HasDefaultValueSql("CURRENT_TIMESTAMP")
                .HasColumnName("fecha_creacion");
            entity.Property(e => e.Origen)
                .HasMaxLength(10)
                .HasDefaultValueSql("'LOCAL'::character varying")
                .HasColumnName("origen");
            entity.Property(e => e.PorcionesBase)
                .HasDefaultValue((short)1)
                .HasColumnName("porciones_base");
            entity.Property(e => e.TiempoPrepMin).HasColumnName("tiempo_prep_min");
            entity.Property(e => e.Titulo)
                .HasMaxLength(120)
                .HasColumnName("titulo");
            entity.Property(e => e.UsuarioId).HasColumnName("usuario_id");

            entity.HasOne(d => d.Categoria).WithMany(p => p.Recetas)
                .HasForeignKey(d => d.CategoriaId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("fk_recetas_categoria");

            entity.HasOne(d => d.Dificultad).WithMany(p => p.Recetas)
                .HasForeignKey(d => d.DificultadId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("fk_recetas_dificultad");

            entity.HasOne(d => d.Usuario).WithMany(p => p.Recetas)
                .HasForeignKey(d => d.UsuarioId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("fk_recetas_usuario");
        });

        modelBuilder.Entity<RecetaIngrediente>(entity =>
        {
            entity.HasKey(e => new { e.RecetaId, e.IngredienteId }).HasName("receta_ingredientes_pkey");

            entity.ToTable("receta_ingredientes");

            entity.Property(e => e.RecetaId).HasColumnName("receta_id");
            entity.Property(e => e.IngredienteId).HasColumnName("ingrediente_id");
            entity.Property(e => e.Cantidad)
                .HasPrecision(8, 2)
                .HasColumnName("cantidad");
            entity.Property(e => e.Nota)
                .HasMaxLength(100)
                .HasColumnName("nota");
            entity.Property(e => e.UnidadId).HasColumnName("unidad_id");

            entity.HasOne(d => d.Ingrediente).WithMany(p => p.RecetaIngredientes)
                .HasForeignKey(d => d.IngredienteId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("fk_ri_ingrediente");

            entity.HasOne(d => d.Receta).WithMany(p => p.RecetaIngredientes)
                .HasForeignKey(d => d.RecetaId)
                .HasConstraintName("fk_ri_receta");

            entity.HasOne(d => d.Unidad).WithMany(p => p.RecetaIngredientes)
                .HasForeignKey(d => d.UnidadId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("fk_ri_unidad");
        });

        modelBuilder.Entity<RecetaMultimedia>(entity =>
        {
            entity.HasKey(e => e.MultimediaId).HasName("receta_multimedia_pkey");

            entity.ToTable("receta_multimedia");

            entity.HasIndex(e => e.RecetaId, "uq_multimedia_portada")
                .IsUnique()
                .HasFilter("es_portada");

            entity.Property(e => e.MultimediaId).HasColumnName("multimedia_id");
            entity.Property(e => e.Descripcion)
                .HasMaxLength(150)
                .HasColumnName("descripcion");
            entity.Property(e => e.EsPortada).HasColumnName("es_portada");
            entity.Property(e => e.FechaSubida)
                .HasDefaultValueSql("CURRENT_TIMESTAMP")
                .HasColumnName("fecha_subida");
            entity.Property(e => e.RecetaId).HasColumnName("receta_id");
            entity.Property(e => e.Ruta)
                .HasMaxLength(255)
                .HasColumnName("ruta");
            entity.Property(e => e.Tipo)
                .HasMaxLength(10)
                .HasColumnName("tipo");

            entity.HasOne(d => d.Receta).WithMany(p => p.Multimedia)
                .HasForeignKey(d => d.RecetaId)
                .HasConstraintName("fk_multimedia_receta");
        });

        modelBuilder.Entity<RecetaPaso>(entity =>
        {
            entity.HasKey(e => new { e.RecetaId, e.NumeroPaso }).HasName("receta_pasos_pkey");

            entity.ToTable("receta_pasos");

            entity.Property(e => e.RecetaId).HasColumnName("receta_id");
            entity.Property(e => e.NumeroPaso).HasColumnName("numero_paso");
            entity.Property(e => e.Descripcion).HasColumnName("descripcion");

            entity.HasOne(d => d.Receta).WithMany(p => p.RecetaPasos)
                .HasForeignKey(d => d.RecetaId)
                .HasConstraintName("fk_pasos_receta");
        });

        modelBuilder.Entity<RedSocial>(entity =>
        {
            entity.HasKey(e => e.RedId).HasName("redes_sociales_pkey");

            entity.ToTable("redes_sociales");

            entity.HasIndex(e => e.Nombre, "redes_sociales_nombre_key").IsUnique();

            entity.Property(e => e.RedId).HasColumnName("red_id");
            entity.Property(e => e.Nombre)
                .HasMaxLength(30)
                .HasColumnName("nombre");
        });

        modelBuilder.Entity<ReporteContenido>(entity =>
        {
            entity.HasKey(e => e.ReporteContenidoId).HasName("reportes_contenido_pkey");

            entity.ToTable("reportes_contenido");

            entity.HasIndex(e => e.ComentarioId, "idx_rc_comentario").HasFilter("(comentario_id IS NOT NULL)");

            entity.HasIndex(e => new { e.Estado, e.Fecha }, "idx_rc_estado");

            entity.HasIndex(e => e.RecetaId, "idx_rc_receta").HasFilter("(receta_id IS NOT NULL)");

            entity.HasIndex(e => new { e.UsuarioId, e.ComentarioId }, "uq_rc_pendiente_comentario")
                .IsUnique()
                .HasFilter("(((estado)::text = 'PENDIENTE'::text) AND (comentario_id IS NOT NULL))");

            entity.HasIndex(e => new { e.UsuarioId, e.RecetaId }, "uq_rc_pendiente_receta")
                .IsUnique()
                .HasFilter("(((estado)::text = 'PENDIENTE'::text) AND (receta_id IS NOT NULL))");

            entity.Property(e => e.ReporteContenidoId).HasColumnName("reporte_contenido_id");
            entity.Property(e => e.AtendidoPor).HasColumnName("atendido_por");
            entity.Property(e => e.ComentarioId).HasColumnName("comentario_id");
            entity.Property(e => e.Descripcion)
                .HasMaxLength(500)
                .HasColumnName("descripcion");
            entity.Property(e => e.Estado)
                .HasMaxLength(10)
                .HasDefaultValueSql("'PENDIENTE'::character varying")
                .HasColumnName("estado");
            entity.Property(e => e.Fecha)
                .HasDefaultValueSql("CURRENT_TIMESTAMP")
                .HasColumnName("fecha");
            entity.Property(e => e.FechaAtencion).HasColumnName("fecha_atencion");
            entity.Property(e => e.Motivo)
                .HasMaxLength(20)
                .HasColumnName("motivo");
            entity.Property(e => e.RecetaId).HasColumnName("receta_id");
            entity.Property(e => e.Resolucion)
                .HasMaxLength(500)
                .HasColumnName("resolucion");
            entity.Property(e => e.UsuarioId).HasColumnName("usuario_id");

            entity.HasOne(d => d.AtendidoPorNavigation).WithMany(p => p.ReporteContenidoAtendidoPorNavigationes)
                .HasForeignKey(d => d.AtendidoPor)
                .HasConstraintName("fk_rc_atendido");

            entity.HasOne(d => d.Comentario).WithMany(p => p.ReportesContenido)
                .HasForeignKey(d => d.ComentarioId)
                .OnDelete(DeleteBehavior.Cascade)
                .HasConstraintName("fk_rc_comentario");

            entity.HasOne(d => d.Receta).WithMany(p => p.ReportesContenido)
                .HasForeignKey(d => d.RecetaId)
                .OnDelete(DeleteBehavior.Cascade)
                .HasConstraintName("fk_rc_receta");

            entity.HasOne(d => d.Usuario).WithMany(p => p.ReporteContenidoUsuarios)
                .HasForeignKey(d => d.UsuarioId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("fk_rc_usuario");
        });

        modelBuilder.Entity<ReporteGenerado>(entity =>
        {
            entity.HasKey(e => e.ReporteId).HasName("reportes_generados_pkey");

            entity.ToTable("reportes_generados");

            entity.Property(e => e.ReporteId).HasColumnName("reporte_id");
            entity.Property(e => e.FechaFin).HasColumnName("fecha_fin");
            entity.Property(e => e.FechaGeneracion)
                .HasDefaultValueSql("CURRENT_TIMESTAMP")
                .HasColumnName("fecha_generacion");
            entity.Property(e => e.FechaInicio).HasColumnName("fecha_inicio");
            entity.Property(e => e.Formato)
                .HasMaxLength(5)
                .HasColumnName("formato");
            entity.Property(e => e.TipoReporte)
                .HasMaxLength(30)
                .HasColumnName("tipo_reporte");
            entity.Property(e => e.UsuarioId).HasColumnName("usuario_id");

            entity.HasOne(d => d.Usuario).WithMany(p => p.ReportesGenerados)
                .HasForeignKey(d => d.UsuarioId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("fk_rep_usuario");
        });

        modelBuilder.Entity<Rol>(entity =>
        {
            entity.HasKey(e => e.RolId).HasName("roles_pkey");

            entity.ToTable("roles");

            entity.HasIndex(e => e.Nombre, "roles_nombre_key").IsUnique();

            entity.Property(e => e.RolId).HasColumnName("rol_id");
            entity.Property(e => e.Descripcion)
                .HasMaxLength(150)
                .HasColumnName("descripcion");
            entity.Property(e => e.Nombre)
                .HasMaxLength(30)
                .HasColumnName("nombre");
        });

        modelBuilder.Entity<UnidadMedida>(entity =>
        {
            entity.HasKey(e => e.UnidadId).HasName("unidades_medida_pkey");

            entity.ToTable("unidades_medida");

            entity.HasIndex(e => e.Abreviatura, "unidades_medida_abreviatura_key").IsUnique();

            entity.HasIndex(e => e.Nombre, "unidades_medida_nombre_key").IsUnique();

            entity.Property(e => e.UnidadId).HasColumnName("unidad_id");
            entity.Property(e => e.Abreviatura)
                .HasMaxLength(10)
                .HasColumnName("abreviatura");
            entity.Property(e => e.Escalable)
                .HasDefaultValue(true)
                .HasColumnName("escalable");
            entity.Property(e => e.Nombre)
                .HasMaxLength(30)
                .HasColumnName("nombre");
        });

        modelBuilder.Entity<Usuario>(entity =>
        {
            entity.HasKey(e => e.UsuarioId).HasName("usuarios_pkey");

            entity.ToTable("usuarios");

            entity.HasIndex(e => e.Correo, "usuarios_correo_key").IsUnique();

            entity.Property(e => e.UsuarioId).HasColumnName("usuario_id");
            entity.Property(e => e.Activo)
                .HasDefaultValue(true)
                .HasColumnName("activo");
            entity.Property(e => e.Apellidos)
                .HasMaxLength(80)
                .HasColumnName("apellidos");
            entity.Property(e => e.Correo)
                .HasMaxLength(120)
                .HasColumnName("correo");
            entity.Property(e => e.Descripcion)
                .HasMaxLength(300)
                .HasColumnName("descripcion");
            entity.Property(e => e.FechaRegistro)
                .HasDefaultValueSql("CURRENT_TIMESTAMP")
                .HasColumnName("fecha_registro");
            entity.Property(e => e.FotoPerfil)
                .HasMaxLength(255)
                .HasColumnName("foto_perfil");
            entity.Property(e => e.Nombre)
                .HasMaxLength(60)
                .HasColumnName("nombre");
            entity.Property(e => e.PasswordHash)
                .HasMaxLength(255)
                .HasColumnName("password_hash");
            entity.Property(e => e.RolId).HasColumnName("rol_id");
            entity.Property(e => e.UltimoAcceso).HasColumnName("ultimo_acceso");

            entity.HasOne(d => d.Rol).WithMany(p => p.Usuarios)
                .HasForeignKey(d => d.RolId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("fk_usuarios_rol");
        });

        modelBuilder.Entity<VisitaReceta>(entity =>
        {
            entity.HasKey(e => e.VisitaId).HasName("visitas_receta_pkey");

            entity.ToTable("visitas_receta");

            entity.HasIndex(e => e.BusquedaId, "idx_visitas_busqueda").HasFilter("(busqueda_id IS NOT NULL)");

            entity.HasIndex(e => e.Fecha, "idx_visitas_fecha");

            entity.HasIndex(e => e.RecetaId, "idx_visitas_receta");

            entity.Property(e => e.VisitaId).HasColumnName("visita_id");
            entity.Property(e => e.BusquedaId).HasColumnName("busqueda_id");
            entity.Property(e => e.Fecha)
                .HasDefaultValueSql("CURRENT_TIMESTAMP")
                .HasColumnName("fecha");
            entity.Property(e => e.RecetaId).HasColumnName("receta_id");
            entity.Property(e => e.UsuarioId).HasColumnName("usuario_id");

            entity.HasOne(d => d.Busqueda).WithMany(p => p.VisitasReceta)
                .HasForeignKey(d => d.BusquedaId)
                .OnDelete(DeleteBehavior.SetNull)
                .HasConstraintName("fk_visita_busqueda");

            entity.HasOne(d => d.Receta).WithMany(p => p.VisitasReceta)
                .HasForeignKey(d => d.RecetaId)
                .HasConstraintName("fk_visita_receta");

            entity.HasOne(d => d.Usuario).WithMany(p => p.VisitasReceta)
                .HasForeignKey(d => d.UsuarioId)
                .OnDelete(DeleteBehavior.SetNull)
                .HasConstraintName("fk_visita_usuario");
        });

        modelBuilder.Entity<VwIngredientesMasUsado>(entity =>
        {
            entity
                .HasNoKey()
                .ToView("vw_ingredientes_mas_usados");

            entity.Property(e => e.CantidadRecetas).HasColumnName("cantidad_recetas");
            entity.Property(e => e.Ingrediente)
                .HasMaxLength(80)
                .HasColumnName("ingrediente");
        });

        modelBuilder.Entity<VwReportesContenidoPendiente>(entity =>
        {
            entity
                .HasNoKey()
                .ToView("vw_reportes_contenido_pendientes");

            entity.Property(e => e.Contenido).HasColumnName("contenido");
            entity.Property(e => e.Descripcion)
                .HasMaxLength(500)
                .HasColumnName("descripcion");
            entity.Property(e => e.Fecha).HasColumnName("fecha");
            entity.Property(e => e.Motivo)
                .HasMaxLength(20)
                .HasColumnName("motivo");
            entity.Property(e => e.ReportadoPor).HasColumnName("reportado_por");
            entity.Property(e => e.ReporteContenidoId).HasColumnName("reporte_contenido_id");
            entity.Property(e => e.TipoContenido).HasColumnName("tipo_contenido");
        });

        modelBuilder.Entity<VwResumenReceta>(entity =>
        {
            entity
                .HasNoKey()
                .ToView("vw_resumen_recetas");

            entity.Property(e => e.Autor).HasColumnName("autor");
            entity.Property(e => e.CalificacionPromedio).HasColumnName("calificacion_promedio");
            entity.Property(e => e.Categoria)
                .HasMaxLength(60)
                .HasColumnName("categoria");
            entity.Property(e => e.RecetaId).HasColumnName("receta_id");
            entity.Property(e => e.Titulo)
                .HasMaxLength(120)
                .HasColumnName("titulo");
            entity.Property(e => e.TotalCompartidos).HasColumnName("total_compartidos");
            entity.Property(e => e.TotalFavoritos).HasColumnName("total_favoritos");
            entity.Property(e => e.TotalMeGusta).HasColumnName("total_me_gusta");
            entity.Property(e => e.TotalVisitas).HasColumnName("total_visitas");
        });

        modelBuilder.Entity<VwTerminosMasBuscado>(entity =>
        {
            entity
                .HasNoKey()
                .ToView("vw_terminos_mas_buscados");

            entity.Property(e => e.PromedioResultados).HasColumnName("promedio_resultados");
            entity.Property(e => e.Termino).HasColumnName("termino");
            entity.Property(e => e.VecesBuscado).HasColumnName("veces_buscado");
        });

        OnModelCreatingPartial(modelBuilder);
    }

    partial void OnModelCreatingPartial(ModelBuilder modelBuilder);
}
