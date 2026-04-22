namespace Harbour.Domain;

/// Interfaz que define el contrato para elementos almacenables
public interface IStorage
{
    /// Identificador único del elemento
    string Id { get; }

    /// Peso propio del elemento (Tara)
    decimal SelfWeight { get; }

    /// Identificador del elemento contenedor padre
    string? ParentId { get; set; }

    /// Identificador foráneo del estado en la tabla StorageStatusTypes
    int StatusId { get; set; }

    /// Propiedad de navegación a StorageStatusType
    StorageStatusType? Status { get; set; }

    /// Calcula el peso total incluyendo contenido
    decimal GetTotalWeight();
}
