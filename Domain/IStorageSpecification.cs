namespace Harbour.Domain;

/// <summary>
/// Define la interfaz común para especificaciones de almacenamiento (Pallets y Contenedores)
/// </summary>
public interface IStorageSpecification
{
    /// <summary>
    /// Peso propio del elemento de almacenamiento en kilogramos (Tara)
    /// </summary>
    decimal SelfWeight { get; }

    /// <summary>
    /// Capacidad máxima de carga en kilogramos
    /// </summary>
    decimal MaxCapacity { get; }
}
