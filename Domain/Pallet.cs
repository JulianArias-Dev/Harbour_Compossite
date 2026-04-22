namespace Harbour.Domain;

/// Representa un pallet estándar (elemento compuesto en el patrón Composite)
/// Especificaciones según norma ISO 6780
public class Pallet : CompositeStorage
{
    /// Identificador foráneo del tipo de pallet en la tabla PalletTypeSpecs
    public int PalletTypeSpecId { get; private set; }

    /// Propiedad de navegación a PalletTypeSpec con especificaciones
    public PalletTypeSpec? PalletTypeSpec { get; set; }

	// Constructor para EF Core (materialización desde DB)
	protected Pallet() : base(string.Empty, 0, 1) { }

	/// Constructor parametrizado para ser usado desde servicios con objeto PalletTypeSpec
	public Pallet(PalletTypeSpec palletTypeSpec, int statusId = 1)
	: base(Guid.NewGuid().ToString(), palletTypeSpec.SelfWeight, statusId)
	{
		PalletTypeSpec = palletTypeSpec;
		PalletTypeSpecId = palletTypeSpec.Id;
	}

	/// Obtiene la especificación de almacenamiento del pallet
	protected override IStorageSpecification? GetStorageSpecification()
	{
		return PalletTypeSpec;
	}

	/// Representación en string del pallet
	public override string ToString()
    {
        return $"Pallet(Id={Id}, Type={PalletTypeSpec?.Name}, Weight={GetTotalWeight()}kg/{PalletTypeSpec?.MaxCapacity}kg, " +
               $"Items={Contents.Count}, StatusId={StatusId})";
    }
}

