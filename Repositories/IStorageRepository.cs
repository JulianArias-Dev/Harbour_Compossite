namespace Harbour.Repositories;

/// <summary>
/// Interfaz segregada genérica para operaciones CRUD de elementos almacenables.
/// Implementa ISP: responsabilidades específicas sin métodos no utilizados.
/// </summary>
public interface IStorageRepository<T> where T : StorageItem
{
	/// Obtiene un elemento por su ID
	Task<T?> GetByIdAsync(string id);

	/// Obtiene todos los elementos de tipo T
	Task<IEnumerable<T>> GetAllAsync();

	/// Añade un nuevo elemento
	Task<T> AddAsync(T item);

	/// Actualiza un elemento existente
	Task<T> UpdateAsync(T item);

	/// Elimina un elemento por su ID, validando que no esté cargado
	Task DeleteAsync(string id);

	/// Verifica si un elemento existe
	Task<bool> ExistsAsync(string id);

	/// Guarda cambios pendientes en la base de datos
	Task SaveChangesAsync();
}
