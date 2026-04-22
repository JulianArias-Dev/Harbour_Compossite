namespace Harbour.Domain;

/// Representa una caja individual (elemento hoja en el patrón Composite)
public class Box : StorageItem
{
    /// Destino final de la caja
    public string Destination { get; set; }

    public Box(
        decimal selfWeight,
        string destination,
        int statusId = 1)
        : base(Guid.NewGuid().ToString(), selfWeight, statusId)
    {
        if (string.IsNullOrWhiteSpace(destination))
            throw new ArgumentException("El destino no puede estar vacío", nameof(destination));

        Destination = destination;
    }

    /// El peso total de una caja es solo su peso propio (es un elemento hoja)
    public override decimal GetTotalWeight() => SelfWeight;

    /// Representación en string de la caja
    public override string ToString()
    {
        return $"Box(Id={Id}, Weight={SelfWeight}kg, Destination={Destination}, StatusId={StatusId})";
    }
}

