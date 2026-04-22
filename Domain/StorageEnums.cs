namespace Harbour.Domain;

/// Estados posibles de un elemento en la cadena de suministro
public enum StorageStatus
{
    /// Elemento recibido en almacén
    Received = 0,

    /// Elemento cargado en vehículo/contenedor
    Loaded = 1,

    /// Elemento enviado/despachado
    Shipped = 2
}

/// Tipos de pallets con especificaciones estándar
public enum PalletType
{
    /// Pallet americano: Tara=28kg, Capacidad máxima=1200kg
    American = 0,

    /// Pallet europeo: Tara=25kg, Capacidad máxima=1500kg
    European = 1
}

/// Tipos de contenedores marítimos estándar ISO
public enum ContainerType
{
    /// Contenedor seco 20 pies: Tara=2330kg, Capacidad=21700kg
    DryVan20 = 0,

    /// Contenedor seco 40 pies: Tara=3750kg, Capacidad=26730kg
    DryVan40 = 1,

    /// Contenedor High Cube 40 pies: Tara=3970kg, Capacidad=26500kg
    HighCube40 = 2,

    /// Contenedor High Cube 45 pies: Tara=4850kg, Capacidad=28350kg
    HighCube45 = 3,

    /// Contenedor refrigerado 20 pies: Tara=2900kg, Capacidad=20800kg
    Reefer20 = 4,

    /// Contenedor refrigerado 40 pies: Tara=4650kg, Capacidad=20350kg
    Reefer40 = 5
}
