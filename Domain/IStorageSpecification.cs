namespace Harbour.Domain;

/// Define la interfaz común para especificaciones de almacenamiento (Pallets y Contenedores)
public interface IStorageSpecification
{
    /// Peso propio del elemento de almacenamiento en kilogramos (Tara)
    decimal SelfWeight { get; }

    /// Capacidad máxima de carga en kilogramos
    decimal MaxCapacity { get; }
}
