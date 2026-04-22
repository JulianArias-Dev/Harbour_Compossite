namespace Harbour.Domain;

/// Clase base abstracta para elementos almacenables que implementa el patrón Composite
/// Utiliza Table-Per-Hierarchy para persistencia en base de datos
public abstract class StorageItem : IStorage
{
   /// Identificador único del elemento
    public string Id { get; private set; }

    /// Peso propio del elemento (Tara)
    public decimal SelfWeight { get; protected set; }

    /// Identificador del elemento contenedor padre (null si es elemento raíz)
    public string? ParentId { get; set; }

    /// Identificador foráneo del estado en la tabla StorageStatusTypes
    public int StatusId { get; set; }

    /// Propiedad de navegación a StorageStatusType
    public StorageStatusType? Status { get; set; }

	public string? ShipId { get; set; }

	/// Constructor protegido para inicializar un elemento almacenable
	protected StorageItem(string id, decimal selfWeight, int statusId = 1)
    {
        // Permitir ID vacío solo para materialización de EF Core (será sobrescrito después)
        if (string.IsNullOrWhiteSpace(id) && id != string.Empty)
            throw new ArgumentException("El ID no puede estar vacío", nameof(id));

        if (selfWeight < 0)
            throw new ArgumentException("El peso propio no puede ser negativo", nameof(selfWeight));

        Id = id ?? string.Empty;
        SelfWeight = selfWeight;
        StatusId = statusId;
        ParentId = null;
    }

    /// Calcula el peso total del elemento (abstracto, debe implementarse en clases derivadas)
    public abstract decimal GetTotalWeight();

    /// Valida que el elemento no esté cargado (ParentId == null) para operaciones de eliminación
    public void ValidateNotLoaded()
    {
        if (ParentId != null)
            throw new InvalidOperationException(
                $"El elemento con ID '{Id}' ya está cargado en otro contenedor. " +
                "Debe ser descargado antes de ser eliminado.");
    }

    /// Obtiene el número de elementos contenidos (solo para CompositeStorage)
    public virtual int GetItemCount() => 0;
}
