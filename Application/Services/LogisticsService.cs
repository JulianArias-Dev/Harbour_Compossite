using Harbour.Domain;
using Harbour.Repositories;

namespace Harbour.Application.Services;

/// <summary>
/// Implementación del servicio de logística
/// Orquesta operaciones de carga y almacenamiento, aplicando reglas de negocio
/// </summary>
public class LogisticsService : ILogisticsService
{
    private readonly IStorageRepository _storageRepository;

    public LogisticsService(IStorageRepository storageRepository)
    {
        _storageRepository = storageRepository ?? throw new ArgumentNullException(nameof(storageRepository));
    }

    /// <inheritdoc/>
    public async Task<Box> CreateBoxAsync(decimal selfWeight, string destination)
    {
        var box = new Box(selfWeight, destination);
        return await _storageRepository.AddAsync(box);
    }

	/// <inheritdoc/>
	public async Task<Pallet> CreatePalletAsync(int palletTypeSpecId)
	{
		var spec = await _storageRepository
			.GetPalletTypeSpecByIdAsync(palletTypeSpecId);

		if (spec == null)
			throw new Exception("PalletTypeSpec no encontrado");

		var pallet = new Pallet(spec);

		return await _storageRepository.AddAsync(pallet);
	}

	/// <inheritdoc/>
	public async Task<Container> CreateContainerAsync(int containerTypeSpecId)
    {
        var container = new Container(containerTypeSpecId);
        return await _storageRepository.AddAsync(container);
    }

    /// <inheritdoc/>
    public async Task<Ship> CreateShipAsync(decimal maxCapacity, decimal minCapacity, bool spotShip = false)
    {
        var ship = new Ship(maxCapacity, minCapacity, spotShip);
        // Nota: Ship no se persiste directamente, se crea en memoria
        // Si se necesita persistencia, agregar DbSet<Ship> en el contexto
        return ship;
    }

    /// <inheritdoc/>
    public async Task<StorageItem> LoadItemToContainerAsync(string childItemId, string parentItemId)
    {
        // Obtener el elemento hijo
        var childItem = await _storageRepository.GetByIdAsync(childItemId)
            ?? throw new InvalidOperationException(
                $"El elemento hijo con ID '{childItemId}' no existe.");

        // Obtener el elemento padre
        var parentItem = await _storageRepository.GetByIdAsync(parentItemId)
            ?? throw new InvalidOperationException(
                $"El contenedor padre con ID '{parentItemId}' no existe.");

        // Validar que el padre sea un CompositeStorage
        if (parentItem is not CompositeStorage compositeParent)
        {
            throw new InvalidOperationException(
                $"El elemento con ID '{parentItemId}' no es un contenedor válido. " +
                $"Solo Pallet y Container pueden contener otros elementos.");
        }

        // Aplicar la regla de negocio: AddItem valida la capacidad
        try
        {
            compositeParent.AddItem(childItem);
        }
        catch (InvalidOperationException ex)
        {
            throw new InvalidOperationException(
                $"No se puede cargar el elemento en el contenedor. Motivo: {ex.Message}", ex);
        }

        // Guardar los cambios en la base de datos
        await _storageRepository.UpdateAsync(parentItem);

        return childItem;
    }

    /// <inheritdoc/>
    public async Task<StorageItem> LoadItemToShipAsync(string itemId, string shipId)
    {
        // Obtener el elemento
        var item = await _storageRepository.GetByIdAsync(itemId)
            ?? throw new InvalidOperationException(
                $"El elemento con ID '{itemId}' no existe.");

        // Nota: Los barcos se manejan en memoria en esta versión
        // En una versión futura, se podría persistir la relación Item-Ship
        // mediante una tabla de relación explícita

        return item;
    }

    /// <inheritdoc/>
    public async Task<decimal> GetTotalWeightAsync(string itemId)
    {
        var item = await _storageRepository.GetWithContentsAsync(itemId)
            ?? throw new InvalidOperationException(
                $"El elemento con ID '{itemId}' no existe.");

        return item.GetTotalWeight();
    }

    /// <inheritdoc/>
    public async Task<StorageItem?> GetStorageItemAsync(string itemId)
    {
        return await _storageRepository.GetWithContentsAsync(itemId);
    }

    /// <inheritdoc/>
    public async Task<IEnumerable<T>> GetItemsByTypeAsync<T>() where T : StorageItem
    {
        return await _storageRepository.GetByTypeAsync<T>();
    }

    /// <inheritdoc/>
    public async Task<Ship?> GetShipAsync(string shipId)
    {
        // Nota: Esta es una implementación simplificada
        // En producción, se retriaría de la base de datos
        return null;
    }
}
