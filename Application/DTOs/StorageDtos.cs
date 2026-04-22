namespace Harbour.Application.DTOs;

/// <summary>
/// DTO para Caja individual con información básica
/// </summary>
public class BoxDto
{
	public string Id { get; set; } = string.Empty;
	public decimal SelfWeight { get; set; }
	public string? ParentId { get; set; }
	public int StatusId { get; set; }
	public string Destination { get; set; } = string.Empty;
}

/// <summary>
/// DTO rico para respuestas de Pallet con información de peso y cantidad de ítems
/// </summary>
public class PalletDetailDto
{
	public string Id { get; set; } = string.Empty;

	/// Nombre del tipo de pallet
	public string? PalletTypeName { get; set; }

	/// Número total de elementos contenidos directamente
	public int ItemCount { get; set; }

	/// Peso propio del pallet (tara)
	public decimal SelfWeightKg { get; set; }

	/// Peso total: peso propio + peso de todos los elementos contenidos
	public decimal TotalWeightKg { get; set; }

	/// Identificador del contenedor padre (null si es elemento raíz)
	public string? ContainerId { get; set; }

	/// Especificaciones del pallet
	public PalletTypeSpecDto? TypeSpecification { get; set; }
}

/// <summary>
/// DTO rico para respuestas de Container con información de peso y cantidad de ítems
/// </summary>
public class ContainerDetailDto
{
	public string Id { get; set; } = string.Empty;

	/// Nombre del tipo de contenedor
	public string? ContainerTypeName { get; set; }

	/// Número total de elementos contenidos directamente
	public int ItemCount { get; set; }

	/// Peso propio del contenedor (tara)
	public decimal SelfWeightKg { get; set; }

	/// Peso total: peso propio + peso de todos los elementos contenidos
	public decimal TotalWeightKg { get; set; }

	/// Identificador del elemento contenedor padre (null si es elemento raíz)
	public string? ParentId { get; set; }

	/// Tamaño en pies (20, 40, 45, etc.)
	public int SizeInFeet { get; set; }

	/// Indica si es refrigerado (Reefer)
	public bool IsRefrigerated { get; set; }

	/// Especificaciones del contenedor
	public ContainerTypeSpecDto? TypeSpecification { get; set; }
}

/// <summary>
/// DTO para especificación de tipo de pallet
/// </summary>
public class PalletTypeSpecDto
{
	public int Id { get; set; }
	public string Name { get; set; } = string.Empty;
	public string? Description { get; set; }
	public decimal SelfWeight { get; set; }
	public decimal MaxCapacity { get; set; }
	public string? Dimensions { get; set; }
}

/// <summary>
/// DTO para especificación de tipo de contenedor
/// </summary>
public class ContainerTypeSpecDto
{
	public int Id { get; set; }
	public string Name { get; set; } = string.Empty;
	public string? Description { get; set; }
	public decimal SelfWeight { get; set; }
	public decimal MaxCapacity { get; set; }
	public string? ExternalDimensions { get; set; }
	public string? InternalDimensions { get; set; }
	public decimal? VolumeM3 { get; set; }
}

/// <summary>
/// DTO rico para respuestas de Ship con información de carga y estado
/// </summary>
public class ShipDetailDto
{
	public string ShipId { get; set; } = string.Empty;

	public bool SpotShip { get; set; }

	public decimal MaxCapacity { get; set; }

	public decimal MinCapacity { get; set; }

	public string Status { get; set; } = string.Empty;

	/// Número de ítems cargados
	public int CargoItemCount { get; set; }

	/// Peso total de la carga actual
	public decimal TotalCargoWeightKg { get; set; }

	/// Peso disponible (capacidad restante)
	public decimal AvailableCapacityKg { get; set; }

	/// Porcentaje de utilización
	public decimal CapacityUtilization { get; set; }

	/// Indica si alcanza la capacidad mínima para zarpar
	public bool CanSail { get; set; }
}

/// <summary>
/// DTO para actualizar un Box
/// </summary>
public class UpdateBoxDto
{
	public string? Destination { get; set; }
}

/// <summary>
/// DTO para actualizar un Pallet
/// </summary>
public class UpdatePalletDto
{
	public int? StatusId { get; set; }
}

/// <summary>
/// DTO para actualizar un Container
/// </summary>
public class UpdateContainerDto
{
	public int? StatusId { get; set; }
}

/// <summary>
/// DTO para actualizar un Ship
/// </summary>
public class UpdateShipDto
{
	public bool? SpotShip { get; set; }
}

/// <summary>
/// DTO de respuesta genérica envolvedor
/// </summary>
public class ApiResponse<T>
{
	public bool Success { get; set; }
	public string? Message { get; set; }
	public T? Data { get; set; }
	public string? Error { get; set; }
}

/// <summary>
/// DTO de respuesta para listados paginados
/// </summary>
public class PaginatedResponse<T>
{
	public List<T> Items { get; set; } = new();
	public int Total { get; set; }
	public int Page { get; set; }
	public int PageSize { get; set; }
}
