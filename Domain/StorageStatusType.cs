namespace Harbour.Domain;

/// Entidad de tabla independiente que define los estados posibles de un elemento
public class StorageStatusType
{
    /// Identificador único del estado
    public int Id { get; set; }

    /// Nombre del estado (ej: "Received", "Loaded", "Shipped")
    public string Name { get; set; } = string.Empty;

    /// Descripción del estado
    public string Description { get; set; } = string.Empty;

    /// Indica si el estado está activo
    public bool IsActive { get; set; } = true;
}
