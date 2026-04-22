namespace Harbour.Repositories;

/// <summary>
/// Interfaz segregada para operaciones específicas de CompositeStorage (Pallets y Containers).
/// Amplía IStorageRepository<T> con operaciones de carga/descarga.
/// </summary>
public interface ICompositeStorageRepository<T> : IStorageRepository<T> where T : CompositeStorage
{
	/// <summary>
	/// Obtiene un CompositeStorage con toda su jerarquía de contenidos cargada (para calcular pesos totales)
	/// </summary>
	Task<T?> GetWithContentsAsync(string id);

	/// <summary>
	/// Carga un objeto dentro de otro validando reglas de peso/capacidad y restricción de sellado
	/// </summary>
	Task LoadItemAsync(string parentId, string childId);

	/// <summary>
	/// Descarga un objeto del contenedor padre
	/// </summary>
	Task UnloadItemAsync(string parentId, string childId);

	/// <summary>
	/// Obtiene el peso total del contenedor (peso propio + todos los contenidos)
	/// </summary>
	Task<decimal> GetTotalWeightAsync(string id);

	/// <summary>
	/// Obtiene el número de elementos contenidos (incluyendo anidados)
	/// </summary>
	Task<int> GetItemCountAsync(string id);
}

/// <summary>
/// Implementación genérica del repositorio para CompositeStorage
/// </summary>
public class CompositeStorageRepository<T> : ICompositeStorageRepository<T> where T : CompositeStorage
{
	protected readonly Infrastructure.Data.HarbourDbContext _context;
	protected readonly IStorageRepository _baseRepository;

	public CompositeStorageRepository(
		Infrastructure.Data.HarbourDbContext context,
		IStorageRepository baseRepository)
	{
		_context = context ?? throw new ArgumentNullException(nameof(context));
		_baseRepository = baseRepository ?? throw new ArgumentNullException(nameof(baseRepository));
	}

	public async Task<T?> GetByIdAsync(string id)
	{
		return await _context.Set<T>()
			.FirstOrDefaultAsync(x => x.Id == id);
	}

	public async Task<IEnumerable<T>> GetAllAsync()
	{
		return await _context.Set<T>().ToListAsync();
	}

	public async Task<T> AddAsync(T item)
	{
		_context.Set<T>().Add(item);
		await _context.SaveChangesAsync();
		return item;
	}

	public async Task<T> UpdateAsync(T item)
	{
		_context.Set<T>().Update(item);
		await _context.SaveChangesAsync();
		return item;
	}

	public async Task DeleteAsync(string id)
	{
		var item = await GetByIdAsync(id);
		if (item == null)
			throw new KeyNotFoundException($"El elemento con ID '{id}' no fue encontrado");

		// Validación de dominio: no se puede eliminar si está cargado
		item.ValidateNotLoaded();

		_context.Set<T>().Remove(item);
		await _context.SaveChangesAsync();
	}

	public async Task<bool> ExistsAsync(string id)
	{
		return await _context.Set<T>().AnyAsync(x => x.Id == id);
	}

	public async Task SaveChangesAsync()
	{
		await _context.SaveChangesAsync();
	}

	public async Task<T?> GetWithContentsAsync(string id)
	{
		var item = await GetByIdAsync(id);
		if (item == null)
			return null;

		// Cargar recursivamente toda la jerarquía de contenidos
		await LoadCompositeContentsRecursivelyAsync(item);

		return item;
	}

	public async Task LoadItemAsync(string parentId, string childId)
	{
		var parent = await GetWithContentsAsync(parentId)
			?? throw new KeyNotFoundException($"Contenedor con ID '{parentId}' no encontrado");

		var child = await _baseRepository.GetByIdAsync(childId)
			?? throw new KeyNotFoundException($"Elemento con ID '{childId}' no encontrado");

		// Las validaciones de dominio se aplican en el método AddItem
		parent.AddItem(child);

		await UpdateAsync(parent);
	}

	public async Task UnloadItemAsync(string parentId, string childId)
	{
		var parent = await GetWithContentsAsync(parentId)
			?? throw new KeyNotFoundException($"Contenedor con ID '{parentId}' no encontrado");

		bool removed = parent.RemoveItem(childId);
		if (!removed)
			throw new InvalidOperationException($"El elemento con ID '{childId}' no estaba en el contenedor");

		await UpdateAsync(parent);
	}

	public async Task<decimal> GetTotalWeightAsync(string id)
	{
		var item = await GetWithContentsAsync(id);
		return item?.GetTotalWeight() ?? 0;
	}

	public async Task<int> GetItemCountAsync(string id)
	{
		var item = await GetWithContentsAsync(id);
		return item?.GetItemCount() ?? 0;
	}

	/// <summary>
	/// Carga recursivamente todos los contenidos de un CompositeStorage
	/// </summary>
	private async Task LoadCompositeContentsRecursivelyAsync(CompositeStorage composite)
	{
		if (composite == null)
			return;

		// Obtener todos los descendientes
		var allDescendants = await GetAllDescendantsAsync(composite.Id);

		// Cargar las especificaciones de Pallet y Container
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
	/// Obtiene todos los descendientes recursivamente de un elemento (BFS)
	/// </summary>
	private async Task<List<StorageItem>> GetAllDescendantsAsync(string parentId)
	{
		var descendants = new List<StorageItem>();
		var queue = new Queue<string>();
		queue.Enqueue(parentId);

		while (queue.Count > 0)
		{
			var currentParentId = queue.Dequeue();

			var directChildren = await _context.Set<StorageItem>()
				.Where(x => x.ParentId == currentParentId)
				.ToListAsync();

			descendants.AddRange(directChildren);

			foreach (var child in directChildren)
			{
				queue.Enqueue(child.Id);
			}
		}

		return descendants;
	}
}
