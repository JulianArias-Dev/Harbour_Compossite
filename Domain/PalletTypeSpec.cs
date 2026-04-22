namespace Harbour.Domain;

/// Entidad de tabla independiente que define los tipos de pallet estándar con sus especificaciones
/// Especificaciones según norma ISO 6780
public class PalletTypeSpec : IStorageSpecification
{
    /// Identificador único del tipo de pallet
    public int Id { get; set; }

    /// Nombre del tipo de pallet (ej: "American", "European")
    public string Name { get; set; } = string.Empty;

    /// Descripción del tipo de pallet
    public string Description { get; set; } = string.Empty;

    /// Peso propio del pallet en kilogramos (Tara)
    /// American: 28kg, European: 25kg
    public decimal SelfWeight { get; set; }

    /// Capacidad máxima de carga en kilogramos 
    public decimal MaxCapacity { get; set; }

    /// Dimensiones del pallet (ej: "40x48 inches", "80x120 cm")
    public string? Dimensions { get; set; }

    /// Indica si el tipo de pallet está activo
    public bool IsActive { get; set; } = true;
}
