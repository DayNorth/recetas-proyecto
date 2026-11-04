using Microsoft.EntityFrameworkCore;
using Recetas.Api.Aplicacion.Dtos;
using Recetas.Api.Aplicacion.Puertos;
using Recetas.Api.Dominio;
using Recetas.Api.Infraestructura.Persistencia;
using Recetas.Api.Infraestructura.Persistencia.Entidades;

namespace Recetas.Api.Infraestructura.Repositorios;

public class RepositorioDeCatalogos(RecetasDbContext laBaseDeDatos) : IRepositorioDeCatalogos
{

    public async Task<IReadOnlyList<CategoriaDto>> ListeLasCategoriasActivas(CancellationToken cancelacion) =>
        await laBaseDeDatos.Categorias.AsNoTracking().Where(categoria => categoria.Activo).OrderBy(categoria => categoria.Nombre)
            .Select(categoria => new CategoriaDto(categoria.CategoriaId, categoria.Nombre, categoria.Descripcion)).ToListAsync(cancelacion);

    public async Task<IReadOnlyList<DificultadDto>> ListeLasDificultades(CancellationToken cancelacion) =>
        await laBaseDeDatos.NivelesDificultad.AsNoTracking().OrderBy(nivel => nivel.DificultadId)
            .Select(nivel => new DificultadDto(nivel.DificultadId, nivel.Nombre)).ToListAsync(cancelacion);

    public async Task<IReadOnlyList<UnidadDto>> ListeLasUnidades(CancellationToken cancelacion) =>
        await laBaseDeDatos.UnidadesMedida.AsNoTracking().OrderBy(unidad => unidad.UnidadId)
            .Select(unidad => new UnidadDto(unidad.UnidadId, unidad.Nombre, unidad.Abreviatura, unidad.Escalable)).ToListAsync(cancelacion);

    public async Task<IReadOnlyList<RedSocialDto>> ListeLasRedesSociales(CancellationToken cancelacion) =>
        await laBaseDeDatos.RedesSociales.AsNoTracking().OrderBy(red => red.RedId)
            .Select(red => new RedSocialDto(red.RedId, red.Nombre)).ToListAsync(cancelacion);

    public async Task<IReadOnlyList<IngredienteDto>> BusqueLosIngredientesPorPrefijo(string elPrefijo, int elMaximo,
        CancellationToken cancelacion) =>
        await laBaseDeDatos.Ingredientes.AsNoTracking()
            .Where(ingrediente => ingrediente.Nombre.ToLower().StartsWith(elPrefijo))
            .OrderBy(ingrediente => ingrediente.Nombre).Take(elMaximo)
            .Select(ingrediente => new IngredienteDto(ingrediente.IngredienteId, ingrediente.Nombre)).ToListAsync(cancelacion);

    public Task<RedSocialDto?> BusqueLaRedSocial(short laRedId, CancellationToken cancelacion) =>
        laBaseDeDatos.RedesSociales.AsNoTracking().Where(red => red.RedId == laRedId)
            .Select(red => new RedSocialDto(red.RedId, red.Nombre)).FirstOrDefaultAsync(cancelacion);

    public async Task<bool?> ConsulteSiLaCategoriaEstaActiva(short laCategoriaId, CancellationToken cancelacion)
    {
        var laCategoria = await laBaseDeDatos.Categorias.Where(categoria => categoria.CategoriaId == laCategoriaId)
            .Select(categoria => new { categoria.Activo }).FirstOrDefaultAsync(cancelacion);
        return laCategoria?.Activo;
    }

    public Task<bool> ExisteLaDificultad(short laDificultadId, CancellationToken cancelacion) =>
        laBaseDeDatos.NivelesDificultad.AnyAsync(nivel => nivel.DificultadId == laDificultadId, cancelacion);

    public Task<int> CuenteLasUnidadesExistentes(IReadOnlyCollection<short> losIds, CancellationToken cancelacion)
    {
        var losIdsBuscados = losIds.ToList();
        return laBaseDeDatos.UnidadesMedida.CountAsync(unidad => losIdsBuscados.Contains(unidad.UnidadId), cancelacion);
    }

    public Task<int> CuenteLosIngredientesExistentes(IReadOnlyCollection<int> losIds, CancellationToken cancelacion)
    {
        var losIdsBuscados = losIds.ToList();
        return laBaseDeDatos.Ingredientes.CountAsync(ingrediente => losIdsBuscados.Contains(ingrediente.IngredienteId), cancelacion);
    }

    public async Task<IReadOnlyList<string>> ListeLosNombresDeIngredientes(IReadOnlyCollection<int> losIds, CancellationToken cancelacion)
    {
        var losIdsBuscados = losIds.ToList();
        return await laBaseDeDatos.Ingredientes.Where(ingrediente => losIdsBuscados.Contains(ingrediente.IngredienteId))
            .OrderBy(ingrediente => ingrediente.Nombre).Select(ingrediente => ingrediente.Nombre).ToListAsync(cancelacion);
    }

    public Task<string?> BusqueElNombreDeLaCategoria(short laCategoriaId, CancellationToken cancelacion) =>
        laBaseDeDatos.Categorias.Where(categoria => categoria.CategoriaId == laCategoriaId)
            .Select(categoria => categoria.Nombre).FirstOrDefaultAsync(cancelacion);

    public Task<string?> BusqueElNombreDeLaDificultad(short laDificultadId, CancellationToken cancelacion) =>
        laBaseDeDatos.NivelesDificultad.Where(nivel => nivel.DificultadId == laDificultadId)
            .Select(nivel => nivel.Nombre).FirstOrDefaultAsync(cancelacion);

    public async Task<IReadOnlyDictionary<string, int>> AgregueLosIngredientesFaltantes(IReadOnlyCollection<string> losNombres,
        CancellationToken cancelacion)
    {
        var lasClaves = losNombres.Select(Texto.CalculeLaClaveDelNombre).ToList();
        var losExistentes = await laBaseDeDatos.Ingredientes
            .Where(ingrediente => lasClaves.Contains(ingrediente.Nombre.ToLower())).ToListAsync(cancelacion);
        var losIngredientesPorClave = losExistentes.ToDictionary(ingrediente => Texto.CalculeLaClaveDelNombre(ingrediente.Nombre));

        foreach (var elNombre in losNombres.Where(nombre => !losIngredientesPorClave.ContainsKey(Texto.CalculeLaClaveDelNombre(nombre))))
        {
            var elIngredienteNuevo = new Ingrediente { Nombre = elNombre };
            laBaseDeDatos.Ingredientes.Add(elIngredienteNuevo);
            losIngredientesPorClave[Texto.CalculeLaClaveDelNombre(elNombre)] = elIngredienteNuevo;
        }
        await laBaseDeDatos.SaveChangesAsync(cancelacion);

        return losIngredientesPorClave.ToDictionary(par => par.Key, par => par.Value.IngredienteId);
    }

    public async Task<IReadOnlyList<CategoriaAdminDto>> ListeLasCategoriasParaAdministracion(CancellationToken cancelacion) =>
        await laBaseDeDatos.Categorias.AsNoTracking().OrderBy(categoria => categoria.Nombre)
            .Select(categoria => new CategoriaAdminDto(categoria.CategoriaId, categoria.Nombre, categoria.Descripcion, categoria.Activo,
                categoria.Recetas.Count)).ToListAsync(cancelacion);

    public Task<CategoriaAdminDto?> BusqueLaCategoriaParaAdministracion(int laCategoriaId, CancellationToken cancelacion) =>
        laBaseDeDatos.Categorias.AsNoTracking().Where(categoria => categoria.CategoriaId == laCategoriaId)
            .Select(categoria => new CategoriaAdminDto(categoria.CategoriaId, categoria.Nombre, categoria.Descripcion, categoria.Activo,
                categoria.Recetas.Count)).FirstOrDefaultAsync(cancelacion);

    public Task<bool> ExisteOtraCategoriaConElNombre(string elNombre, int? laCategoriaExcluidaId, CancellationToken cancelacion)
    {
        var elNombreEnMinusculas = elNombre.ToLower();
        return laBaseDeDatos.Categorias.AnyAsync(
            categoria => categoria.Nombre.ToLower() == elNombreEnMinusculas && categoria.CategoriaId != laCategoriaExcluidaId, cancelacion);
    }

    public async Task<short> AgregueLaCategoria(string elNombre, string? laDescripcion, CancellationToken cancelacion)
    {
        var laCategoria = new Categoria { Nombre = elNombre, Descripcion = laDescripcion, Activo = true };
        laBaseDeDatos.Categorias.Add(laCategoria);
        await laBaseDeDatos.SaveChangesAsync(cancelacion);
        return laCategoria.CategoriaId;
    }

    public async Task<bool> ActualiceLaCategoria(int laCategoriaId, string elNombre, string? laDescripcion, CancellationToken cancelacion)
    {
        var laCategoria = await laBaseDeDatos.Categorias.FirstOrDefaultAsync(categoria => categoria.CategoriaId == laCategoriaId, cancelacion);
        if (laCategoria is null) return false;
        laCategoria.Nombre = elNombre;
        laCategoria.Descripcion = laDescripcion;
        await laBaseDeDatos.SaveChangesAsync(cancelacion);
        return true;
    }

    public async Task<bool> CambieElEstadoDeLaCategoria(int laCategoriaId, bool estaActiva, CancellationToken cancelacion) =>
        await laBaseDeDatos.Categorias.Where(categoria => categoria.CategoriaId == laCategoriaId)
            .ExecuteUpdateAsync(cambios => cambios.SetProperty(categoria => categoria.Activo, estaActiva), cancelacion) > 0;
}
