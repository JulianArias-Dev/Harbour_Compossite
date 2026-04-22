using Harbour.Domain;
using Harbour.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace Harbour.Repositories;

/// Interfaz genérica para el repositorio de elementos almacenables
public interface IStorageRepository
{
    /// Obtiene un elemento de almacenamiento por su ID, incluyendo toda la jerarquía
    Task<StorageItem?> GetByIdAsync(string id);

    /// Obtiene un elemento con todos sus contenidos (para CompositeStorage)
    Task<StorageItem?> GetWithContentsAsync(string id);

    /// Obtiene todos los elementos de almacenamiento
    Task<IEnumerable<StorageItem>> GetAllAsync();

    /// Añade un nuevo elemento de almacenamiento
    Task<T> AddAsync<T>(T item) where T : StorageItem;

    /// Actualiza un elemento de almacenamiento existente
    Task<T> UpdateAsync<T>(T item) where T : StorageItem;

    /// Elimina un elemento de almacenamiento por su ID
    Task<bool> DeleteAsync(string id);

    /// Guarda todos los cambios en la base de datos
    Task<int> SaveChangesAsync();

    /// Obtiene todos los elementos de tipo T
    Task<IEnumerable<T>> GetByTypeAsync<T>() where T : StorageItem;

	/// Obtiene las especificaciones de un tipo de pallet por su ID
	Task<PalletTypeSpec?> GetPalletTypeSpecByIdAsync(int id);

	/// Obtiene las especificaciones de un tipo de contenedor por su ID
	Task<ContainerTypeSpec?> GetContainerTypeSpecByIdAsync(int id);
}

/// <summary>
/// Implementación del repositorio de elementos almacenables usando EF Core
/// </summary>
public class StorageRepository : IStorageRepository
{
    private readonly HarbourDbContext _context;

    public StorageRepository(HarbourDbContext context)
    {
        _context = context ?? throw new ArgumentNullException(nameof(context));
    }

    /// <inheritdoc/>
    public async Task<StorageItem?> GetByIdAsync(string id)
    {
        var item = await _context.StorageItems
            .FirstOrDefaultAsync(x => x.Id == id);

        if (item is Pallet pallet)
        {
            await _context.Entry(pallet).Reference(x => x.PalletTypeSpec).LoadAsync();
        }
        else if (item is Container container)
        {
            await _context.Entry(container).Reference(x => x.ContainerTypeSpec).LoadAsync();
        }

        return item;
    }

    /// <inheritdoc/>
    public async Task<StorageItem?> GetWithContentsAsync(string id)
    {
        // Obtener el elemento principal
        var item = await _context.StorageItems
            .FirstOrDefaultAsync(x => x.Id == id);

        if (item == null)
            return null;

        // Cargar la navegación según el tipo
        if (item is Pallet pallet)
        {
            await _context.Entry(pallet).Reference(x => x.PalletTypeSpec).LoadAsync();
        }
        else if (item is Container container)
        {
            await _context.Entry(container).Reference(x => x.ContainerTypeSpec).LoadAsync();
        }

        if (item is CompositeStorage)
        {
            // Cargar recursivamente toda la jerarquía de contenidos
            await LoadCompositeContentsRecursivelyAsync(item as CompositeStorage);
        }

        return item;
    }

    /// <summary>
    /// Carga recursivamente todos los contenidos de un CompositeStorage
    /// </summary>
    private async Task LoadCompositeContentsRecursivelyAsync(CompositeStorage composite)
    {
        if (composite == null)
            return;

        // Obtener todos los descendientes usando una query recursiva (CTE equivalente)
        var allDescendants = await GetAllDescendantsAsync(composite.Id);
        
        // Cargar las navegaciones de especificaciones
        foreach (var descendant in allDescendants)
        {
            if (descendant is Pallet descendantPallet)
            {
                await _context.Entry(descendantPallet).Reference(x => x.PalletTypeSpec).LoadAsync();
            }
            else if (descendant is Container descendantContainer)
            {
                await _context.Entry(descendantContainer).Reference(x => x.ContainerTypeSpec).LoadAsync();
            }
        }
    }

    /// <summary>
    /// Obtiene todos los descendientes (recursivamente) de un elemento, similar a un CTE recursivo en SQL
    /// </summary>
    private async Task<List<StorageItem>> GetAllDescendantsAsync(string parentId)
    {
        var descendants = new List<StorageItem>();
        var queue = new Queue<string>();
        queue.Enqueue(parentId);

        while (queue.Count > 0)
        {
            var currentParentId = queue.Dequeue();
            
            // Obtener los hijos directos del elemento actual
            var directChildren = await _context.StorageItems
                .Where(x => x.ParentId == currentParentId)
                .ToListAsync();

            descendants.AddRange(directChildren);

            // Agregar a la cola los IDs de los hijos para procesarlos recursivamente
            foreach (var child in directChildren)
            {
                queue.Enqueue(child.Id);
            }
        }

        return descendants;
    }

    /// <inheritdoc/>
    public async Task<IEnumerable<StorageItem>> GetAllAsync()
    {
        var items = await _context.StorageItems.ToListAsync();

        foreach (var item in items)
        {
            if (item is Pallet pallet)
            {
                await _context.Entry(pallet).Reference(x => x.PalletTypeSpec).LoadAsync();
            }
            else if (item is Container container)
            {
                await _context.Entry(container).Reference(x => x.ContainerTypeSpec).LoadAsync();
            }
        }

        return items;
    }

    /// <inheritdoc/>
    public async Task<T> AddAsync<T>(T item) where T : StorageItem
    {
        _context.StorageItems.Add(item);
        await _context.SaveChangesAsync();
        return item;
    }

    /// <inheritdoc/>
    public async Task<T> UpdateAsync<T>(T item) where T : StorageItem
    {
        _context.StorageItems.Update(item);
        await _context.SaveChangesAsync();
        return item;
    }

    /// <inheritdoc/>
    public async Task<bool> DeleteAsync(string id)
    {
        var item = await _context.StorageItems.FindAsync(id);
        if (item == null)
            return false;

        _context.StorageItems.Remove(item);
        await _context.SaveChangesAsync();
        return true;
    }

    /// <inheritdoc/>
    public async Task<int> SaveChangesAsync()
    {
        return await _context.SaveChangesAsync();
    }

    /// <inheritdoc/>
    public async Task<IEnumerable<T>> GetByTypeAsync<T>() where T : StorageItem
    {
        var items = await _context.StorageItems
            .OfType<T>()
            .ToListAsync();

        // Load navigation properties based on type
        foreach (var item in items)
        {
            if (item is Pallet pallet)
            {
                await _context.Entry(pallet).Reference(x => x.PalletTypeSpec).LoadAsync();
            }
            else if (item is Container container)
            {
                await _context.Entry(container).Reference(x => x.ContainerTypeSpec).LoadAsync();
            }
        }

        return items;
    }

	public async Task<PalletTypeSpec?> GetPalletTypeSpecByIdAsync(int id)
	{
		return await _context.PalletTypeSpecs
			.FirstOrDefaultAsync(x => x.Id == id);
	}

	public async Task<ContainerTypeSpec?> GetContainerTypeSpecByIdAsync(int id)
	{
		return await _context.ContainerTypeSpecs
			.FirstOrDefaultAsync(x => x.Id == id);
	}
}
