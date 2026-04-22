namespace Harbour.Domain;

/// Entidad de tabla independiente que define los tipos de contenedor marítimo estándar ISO con sus especificaciones
/// Especificaciones basadas en normas ISO 668 e ISO 1496
public class ContainerTypeSpec : IStorageSpecification
{
    /// Identificador único del tipo de contenedor
    public int Id { get; set; }

    /// Nombre del tipo de contenedor (ej: "DryVan20", "HighCube40")
    public string Name { get; set; } = string.Empty;

    /// Descripción del tipo de contenedor
    public string Description { get; set; } = string.Empty;

    /// Peso propio del contenedor en kilogramos (Tara)
    /// DryVan20: 2330kg, DryVan40: 3750kg, HighCube40: 3970kg, etc.
    public decimal SelfWeight { get; set; }

    /// Capacidad máxima de carga en kilogramos
    /// DryVan20: 21700kg, DryVan40: 26730kg, etc.
    public decimal MaxCapacity { get; set; }

    /// Dimensiones exteriores del contenedor (ej: "20x8x8.6 feet", "40x8x8.6 feet")
    public string? ExternalDimensions { get; set; }

    /// Dimensiones interiores del contenedor
    public string? InternalDimensions { get; set; }

    /// Volumen del contenedor en metros cúbicos
    public decimal? VolumeM3 { get; set; }

    /// Indica si el tipo de contenedor está activo
    public bool IsActive { get; set; } = true;
}
