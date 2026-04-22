namespace Harbour.Domain;

/// Representa un contenedor marítimo ISO estándar (elemento compuesto en el patrón Composite)
/// Especificaciones basadas en normas ISO 668 e ISO 1496
public class Container : CompositeStorage
{
    /// Identificador foráneo del tipo de contenedor en la tabla ContainerTypeSpecs
    public int ContainerTypeSpecId { get; private set; }

    /// Propiedad de navegación a ContainerTypeSpec con especificaciones
    public ContainerTypeSpec? ContainerTypeSpec { get; set; }

	/// Constructor para crear un nuevo contenedor
	public Container(int containerTypeSpecId, int statusId = 1)
	: base(Guid.NewGuid().ToString(), 0, statusId)
	{
		ContainerTypeSpecId = containerTypeSpecId;
	}

	/// Constructor parametrizado para ser usado desde servicios con objeto ContainerTypeSpec
	public Container(ContainerTypeSpec containerTypeSpec, int statusId = 1)
	: base(Guid.NewGuid().ToString(), containerTypeSpec.SelfWeight, statusId)
	{
		ContainerTypeSpecId = containerTypeSpec.Id;
		ContainerTypeSpec = containerTypeSpec;
	}

	/// Obtiene la especificación de almacenamiento del contenedor
	protected override IStorageSpecification? GetStorageSpecification()
	{
		return ContainerTypeSpec;
	}

	/// Obtiene el tamaño del contenedor en pies basado en el nombre del tipo
	public int GetContainerSizeInFeet()
    {
        var typeName = ContainerTypeSpec?.Name ?? string.Empty;
        return typeName switch
        {
            var t when t.Contains("20") => 20,
            var t when t.Contains("40") => 40,
            var t when t.Contains("45") => 45,
            _ => 0
        };
    }

    /// Indica si el contenedor es de tipo refrigerado basado en el nombre del tipo
    public bool IsRefrigerated => ContainerTypeSpec?.Name?.Contains("Reefer", StringComparison.OrdinalIgnoreCase) ?? false;

    /// Representación en string del contenedor
    public override string ToString()
    {
        return $"Container(Id={Id}, Type={ContainerTypeSpec?.Name}, Weight={GetTotalWeight()}kg/{ContainerTypeSpec?.MaxCapacity}kg, " +
               $"Items={Contents.Count}, StatusId={StatusId}, Refrigerated={IsRefrigerated})";
    }
}

