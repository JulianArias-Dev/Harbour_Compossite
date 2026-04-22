namespace Harbour.Domain;

/// <summary>
/// Estados posibles de un elemento en la cadena de suministro
/// </summary>
public enum StorageStatus
{
    /// Elemento recibido en almacén
    Received = 0,

    /// Elemento cargado en vehículo/contenedor
    Loaded = 1,

    /// Elemento enviado/despachado
    Shipped = 2
}

/// <summary>
/// Tipos de pallets con especificaciones estándar
/// </summary>
public enum PalletType
{
    /// <summary>
    /// Pallet americano: Tara=28kg, Capacidad máxima=1200kg
    /// </summary>
    American = 0,

    /// <summary>
    /// Pallet europeo: Tara=25kg, Capacidad máxima=1500kg
    /// </summary>
    European = 1
}

/// <summary>
/// Tipos de contenedores marítimos estándar ISO
/// </summary>
public enum ContainerType
{
    /// <summary>
    /// Contenedor seco 20 pies: Tara=2330kg, Capacidad=21700kg
    /// </summary>
    DryVan20 = 0,

    /// <summary>
    /// Contenedor seco 40 pies: Tara=3750kg, Capacidad=26730kg
    /// </summary>
    DryVan40 = 1,

    /// <summary>
    /// Contenedor High Cube 40 pies: Tara=3970kg, Capacidad=26500kg
    /// </summary>
    HighCube40 = 2,

    /// <summary>
    /// Contenedor High Cube 45 pies: Tara=4850kg, Capacidad=28350kg
    /// </summary>
    HighCube45 = 3,

    /// <summary>
    /// Contenedor refrigerado 20 pies: Tara=2900kg, Capacidad=20800kg
    /// </summary>
    Reefer20 = 4,

    /// <summary>
    /// Contenedor refrigerado 40 pies: Tara=4650kg, Capacidad=20350kg
    /// </summary>
    Reefer40 = 5
}
