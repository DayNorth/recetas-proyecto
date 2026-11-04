namespace Recetas.Api.Aplicacion.Dtos;

public record ListaResumenDto(int ListaId, string Nombre, string Estado, DateTime FechaCreacion, int TotalItems, int Comprados);
public record ListaItemDto(int ItemId, int IngredienteId, string Ingrediente, short UnidadId, string Unidad, decimal Cantidad,
    int? RecetaOrigenId, string? RecetaOrigen, bool Comprado);
public record ListaDetalleDto(int ListaId, string Nombre, string Estado, DateTime FechaCreacion, IReadOnlyList<ListaItemDto> Items);
public record ItemActualizadoDto(ListaItemDto Item, string EstadoLista);
