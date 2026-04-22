using System.ComponentModel.DataAnnotations.Schema;

namespace Harbour.Domain;

/// Clase base para elementos contenedores en el patrón Composite
/// Implementa la capacidad de contener otros elementos (polimórficos)
public abstract class CompositeStorage : StorageItem
{
    private readonly List<StorageItem> _contents = new();

	/// Colección de elementos contenidos (solo lectura desde el exterior)
	public IReadOnlyCollection<StorageItem> Contents => _contents.AsReadOnly();

	/// Constructor protegido para inicializar un contenedor
	protected CompositeStorage(
	string id,
	decimal selfWeight,
	int statusId = 1)
	: base(id, selfWeight, statusId)
	{
	}

	/// Obtiene la especificación de almacenamiento (debe implementarse en clases derivadas)
	protected abstract IStorageSpecification? GetStorageSpecification();

	/// Calcula el peso total: peso propio + peso de todos los elementos contenidos
	public override decimal GetTotalWeight()
    {
        return SelfWeight + _contents.Sum(item => item.GetTotalWeight());
    }

    /// Añade un elemento al contenedor con validación de capacidad y restricción de sellado
    public void AddItem(StorageItem item)
    {
        if (item == null)
            throw new ArgumentNullException(nameof(item), "El elemento no puede ser null");

        if (item.Id == Id)
            throw new InvalidOperationException("Un contenedor no puede contenerse a sí mismo");

        // Regla 1: Validar que el contenedor actual no esté sellado (cargado en otro contenedor)
        if (ParentId != null)
            throw new InvalidOperationException(
                $"El contenedor con ID '{Id}' ya está sellado (cargado en otro contenedor). " +
                "No se pueden añadir más elementos en su interior.");

        var spec = GetStorageSpecification();
        if (spec == null)
            throw new InvalidOperationException("La especificación del contenedor no ha sido inicializada");

        decimal currentWeight = GetTotalWeight();
        decimal itemWeight = item.GetTotalWeight();
        decimal projectedWeight = currentWeight + itemWeight;

        // Regla 2: Validar capacidad máxima
        if (projectedWeight > spec.MaxCapacity)
        {
            throw new InvalidOperationException(
                $"No se puede añadir el elemento. Peso actual: {currentWeight}kg, " +
                $"Peso del elemento: {itemWeight}kg, Total proyectado: {projectedWeight}kg, " +
                $"Capacidad máxima: {spec.MaxCapacity}kg");
        }

        item.ParentId = Id;
        _contents.Add(item);
    }

    /// Elimina un elemento del contenedor por su ID
    public bool RemoveItem(string itemId)
    {
        var item = _contents.FirstOrDefault(i => i.Id == itemId);
        if (item != null)
        {
            item.ParentId = null;
            return _contents.Remove(item);
        }
        return false;
    }

    /// Obtiene un elemento contenido por su ID
    public StorageItem? GetItem(string itemId)
    {
        return _contents.FirstOrDefault(i => i.Id == itemId);
    }

    /// Verifica si el contenedor está vacío
    public bool IsEmpty => _contents.Count == 0;

    /// Obtiene el peso disponible (capacidad restante)
    public decimal AvailableCapacity => (GetStorageSpecification()?.MaxCapacity ?? 0) - GetTotalWeight();

	/// Obtiene el número total de elementos contenidos (solo conteo directo, no recursivo)
	public override int GetItemCount() => _contents.Count;
}
