namespace Harbour.Application.DTOs;

/// <summary>
/// DTO para solicitudes de creación de cajas
/// </summary>
public record CreateBoxDto(
    decimal SelfWeight,
    string Destination);

/// <summary>
/// DTO para solicitudes de creación de pallets
/// </summary>
public record CreatePalletDto(
    int PalletTypeSpecId);

/// <summary>
/// DTO para solicitudes de creación de contenedores
/// </summary>
public record CreateContainerDto(
    int ContainerTypeSpecId);

/// <summary>
/// DTO para solicitudes de carga de un elemento en un contenedor
/// </summary>
public record LoadItemDto(
    string ChildItemId,
    string ParentItemId);

/// <summary>
/// DTO para solicitudes de creación/actualización de barcos
/// </summary>
public record CreateShipDto(
    decimal MaxCapacity,
    decimal MinCapacity,
    bool SpotShip = false);

/// <summary>
/// DTO para a adicionar carga a un barco
/// </summary>
public record LoadCargoDto(
    string ItemId);

/// <summary>
/// DTO para respuesta de peso de un elemento
/// </summary>
public record StorageItemWeightResponseDto(
    string Id,
    string Type,
    decimal SelfWeight,
    decimal TotalWeight,
    int? ItemsCount = null);

/// <summary>
/// DTO para respuesta de peso total de un barco
/// </summary>
public record ShipWeightResponseDto(
    string ShipId,
    decimal CurrentWeight,
    decimal MaxCapacity,
    decimal MinCapacity,
    decimal AvailableCapacity,
    int ItemsCount,
    decimal CapacityUtilizationPercent,
    bool IsViable);

/// <summary>
/// DTO para respuesta de error de validación
/// </summary>
public record ErrorResponseDto(
    string Message,
    string ErrorCode,
    Dictionary<string, string>? Details = null);

/// <summary>
/// DTO para respuesta de operación exitosa
/// </summary>
public record SuccessResponseDto(
    string Message,
    object? Data = null);
