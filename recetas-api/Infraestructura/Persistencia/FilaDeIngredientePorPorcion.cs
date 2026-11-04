using System.ComponentModel.DataAnnotations.Schema;

namespace Recetas.Api.Infraestructura.Persistencia;

public class FilaDeIngredientePorPorcion
{
    [Column("ingrediente")] public string Ingrediente { get; set; } = "";
    [Column("cantidad")] public decimal Cantidad { get; set; }
    [Column("unidad")] public string Unidad { get; set; } = "";
    [Column("nota")] public string? Nota { get; set; }
    [Column("ingrediente_id")] public int IngredienteId { get; set; }
    [Column("unidad_id")] public short UnidadId { get; set; }
}
