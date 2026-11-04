using Recetas.Api.Aplicacion.Dtos;

namespace Recetas.Api.Aplicacion.Puertos;

public interface IRepositorioDeCatalogos
{
    Task<IReadOnlyList<CategoriaDto>> ListeLasCategoriasActivas(CancellationToken cancelacion);

    Task<IReadOnlyList<DificultadDto>> ListeLasDificultades(CancellationToken cancelacion);

    Task<IReadOnlyList<UnidadDto>> ListeLasUnidades(CancellationToken cancelacion);

    Task<IReadOnlyList<RedSocialDto>> ListeLasRedesSociales(CancellationToken cancelacion);

    Task<IReadOnlyList<IngredienteDto>> BusqueLosIngredientesPorPrefijo(string elPrefijo, int elMaximo,
        CancellationToken cancelacion);

    Task<RedSocialDto?> BusqueLaRedSocial(short laRedId, CancellationToken cancelacion);

    Task<bool?> ConsulteSiLaCategoriaEstaActiva(short laCategoriaId, CancellationToken cancelacion);

    Task<bool> ExisteLaDificultad(short laDificultadId, CancellationToken cancelacion);

    Task<int> CuenteLasUnidadesExistentes(IReadOnlyCollection<short> losIds, CancellationToken cancelacion);

    Task<int> CuenteLosIngredientesExistentes(IReadOnlyCollection<int> losIds, CancellationToken cancelacion);

    Task<IReadOnlyList<string>> ListeLosNombresDeIngredientes(IReadOnlyCollection<int> losIds, CancellationToken cancelacion);

    Task<string?> BusqueElNombreDeLaCategoria(short laCategoriaId, CancellationToken cancelacion);

    Task<string?> BusqueElNombreDeLaDificultad(short laDificultadId, CancellationToken cancelacion);

    Task<IReadOnlyDictionary<string, int>> AgregueLosIngredientesFaltantes(IReadOnlyCollection<string> losNombres,
        CancellationToken cancelacion);

    Task<IReadOnlyList<CategoriaAdminDto>> ListeLasCategoriasParaAdministracion(CancellationToken cancelacion);

    Task<CategoriaAdminDto?> BusqueLaCategoriaParaAdministracion(int laCategoriaId, CancellationToken cancelacion);

    Task<bool> ExisteOtraCategoriaConElNombre(string elNombre, int? laCategoriaExcluidaId, CancellationToken cancelacion);

    Task<short> AgregueLaCategoria(string elNombre, string? laDescripcion, CancellationToken cancelacion);

    Task<bool> ActualiceLaCategoria(int laCategoriaId, string elNombre, string? laDescripcion, CancellationToken cancelacion);

    Task<bool> CambieElEstadoDeLaCategoria(int laCategoriaId, bool estaActiva, CancellationToken cancelacion);
}
