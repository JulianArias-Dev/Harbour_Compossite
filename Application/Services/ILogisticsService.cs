namespace Harbour.Application.Services;

/// <summary>
/// Interfaz para el servicio de logística que orquesta operaciones de carga y almacenamiento
/// </summary>
public interface ILogisticsService
{
        
    // Operaciones de carga y asignación

    /// <summary>
    /// Asigna un elemento hijo a un contenedor padre, validando reglas de negocio
    /// </summary>
    Task<StorageItem> LoadItemToContainerAsync(string childItemId, string parentItemId);

    /// <summary>
    /// Carga un elemento de almacenamiento en un barco
    /// </summary>
    Task<StorageItem> LoadItemToShipAsync(string itemId, string shipId);

    // Operaciones de consulta

    /// <summary>
    /// Obtiene el peso total de un elemento (incluyendo su contenido si es contenedor)
    /// </summary>
    Task<decimal> GetTotalWeightAsync(string itemId);

    /// <summary>
    /// Obtiene un elemento de almacenamiento completo con su jerarquía
    /// </summary>
    Task<StorageItem?> GetStorageItemAsync(string itemId);

    /// <summary>
    /// Obtiene todos los elementos de un tipo específico
    /// </summary>
    Task<IEnumerable<T>> GetItemsByTypeAsync<T>() where T : StorageItem;

}
