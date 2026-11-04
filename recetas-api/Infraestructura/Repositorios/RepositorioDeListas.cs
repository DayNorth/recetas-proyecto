using Microsoft.EntityFrameworkCore;
using Recetas.Api.Aplicacion.Comun;
using Recetas.Api.Aplicacion.Dtos;
using Recetas.Api.Aplicacion.Puertos;
using Recetas.Api.Dominio;
using Recetas.Api.Infraestructura.Persistencia;
using Recetas.Api.Infraestructura.Persistencia.Entidades;

namespace Recetas.Api.Infraestructura.Repositorios;

public class RepositorioDeListas(RecetasDbContext laBaseDeDatos) : IRepositorioDeListas
{
    private IQueryable<ListaCompra> FiltreLasListasDelUsuario(int elUsuarioId) =>
        laBaseDeDatos.ListasCompra.Where(lista => lista.UsuarioId == elUsuarioId);

    private static ListaItemDto ConviertaAlDto(ListaCompraItem elItem) =>
        new(elItem.ItemId, elItem.IngredienteId, elItem.Ingrediente.Nombre, elItem.UnidadId, elItem.Unidad.Abreviatura, elItem.Cantidad,
            elItem.RecetaOrigenId, elItem.RecetaOrigen != null ? elItem.RecetaOrigen.Titulo : null, elItem.Comprado);

    public async Task<int> AgregueLaLista(int elUsuarioId, string elNombre, CancellationToken cancelacion)
    {
        var laLista = new ListaCompra { UsuarioId = elUsuarioId, Nombre = elNombre, Estado = EstadosDeLaLista.ACTIVA };
        laBaseDeDatos.ListasCompra.Add(laLista);
        await laBaseDeDatos.SaveChangesAsync(cancelacion);
        return laLista.ListaId;
    }

    public async Task<int> AgregueLosItemsDesdeLaReceta(int laListaId, int laRecetaId, int lasPorciones, CancellationToken cancelacion) =>
        await laBaseDeDatos.Database.ExecuteSqlInterpolatedAsync($@"
                INSERT INTO lista_compra_items (lista_id, ingrediente_id, unidad_id, cantidad, receta_origen_id)
                SELECT {laListaId}, ingrediente_id, unidad_id, cantidad, {laRecetaId}
                FROM sp_ingredientes_por_porciones({laRecetaId}, {lasPorciones})", cancelacion);

    public Task<Paginado<ListaResumenDto>> ListeLasListas(int elUsuarioId, string? elEstado, SolicitudDePagina laPagina,
        CancellationToken cancelacion)
    {
        var laConsulta = FiltreLasListasDelUsuario(elUsuarioId).AsNoTracking();
        if (elEstado is not null) laConsulta = laConsulta.Where(lista => lista.Estado == elEstado);
        return laConsulta.OrderByDescending(lista => lista.FechaCreacion).ThenByDescending(lista => lista.ListaId)
            .Select(lista => new ListaResumenDto(lista.ListaId, lista.Nombre, lista.Estado, lista.FechaCreacion,
                lista.ListaCompraItems.Count, lista.ListaCompraItems.Count(elItem => elItem.Comprado)))
            .PagineLaConsulta(laPagina, cancelacion);
    }

    public async Task<ListaDetalleDto?> BusqueElDetalleDeLaLista(int elUsuarioId, int laListaId, CancellationToken cancelacion)
    {
        var laLista = await FiltreLasListasDelUsuario(elUsuarioId).AsNoTracking().AsSplitQuery()
            .Include(lista => lista.ListaCompraItems).ThenInclude(elItem => elItem.Ingrediente)
            .Include(lista => lista.ListaCompraItems).ThenInclude(elItem => elItem.Unidad)
            .Include(lista => lista.ListaCompraItems).ThenInclude(elItem => elItem.RecetaOrigen)
            .FirstOrDefaultAsync(lista => lista.ListaId == laListaId, cancelacion);
        return laLista is null
            ? null
            : new ListaDetalleDto(laLista.ListaId, laLista.Nombre, laLista.Estado, laLista.FechaCreacion,
                laLista.ListaCompraItems.OrderBy(elItem => elItem.Ingrediente.Nombre).Select(ConviertaAlDto).ToList());
    }

    public Task<bool> ExisteLaListaDelUsuario(int elUsuarioId, int laListaId, CancellationToken cancelacion) =>
        FiltreLasListasDelUsuario(elUsuarioId).AnyAsync(lista => lista.ListaId == laListaId, cancelacion);

    public async Task BloqueeLaLista(int laListaId, CancellationToken cancelacion) =>
        await laBaseDeDatos.Database.ExecuteSqlInterpolatedAsync($"SELECT 1 FROM listas_compras WHERE lista_id = {laListaId} FOR UPDATE", cancelacion);

    public async Task<bool> MarqueElItem(int laListaId, int elItemId, bool estaComprado, CancellationToken cancelacion) =>
        await laBaseDeDatos.ListaCompraItems.Where(elItem => elItem.ItemId == elItemId && elItem.ListaId == laListaId)
            .ExecuteUpdateAsync(cambios => cambios.SetProperty(elItem => elItem.Comprado, estaComprado), cancelacion) > 0;

    public Task<bool> HayItemsPendientes(int laListaId, CancellationToken cancelacion) =>
        laBaseDeDatos.ListaCompraItems.AnyAsync(elItem => elItem.ListaId == laListaId && !elItem.Comprado, cancelacion);

    public Task EstablezcaElEstadoDeLaLista(int laListaId, string elEstado, CancellationToken cancelacion) =>
        laBaseDeDatos.ListasCompra.Where(lista => lista.ListaId == laListaId)
            .ExecuteUpdateAsync(cambios => cambios.SetProperty(lista => lista.Estado, elEstado), cancelacion);

    public async Task<ListaItemDto> BusqueElItem(int elItemId, CancellationToken cancelacion) =>
        ConviertaAlDto(await laBaseDeDatos.ListaCompraItems.AsNoTracking().Where(elItem => elItem.ItemId == elItemId)
            .Include(elItem => elItem.Ingrediente).Include(elItem => elItem.Unidad).Include(elItem => elItem.RecetaOrigen)
            .SingleAsync(cancelacion));

    public async Task<bool> BorreLaLista(int elUsuarioId, int laListaId, CancellationToken cancelacion) =>
        await FiltreLasListasDelUsuario(elUsuarioId).Where(lista => lista.ListaId == laListaId).ExecuteDeleteAsync(cancelacion) > 0;
}
