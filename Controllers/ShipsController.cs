using Microsoft.AspNetCore.Mvc;

namespace Harbour.Controllers;

/// <summary>
/// Controlador REST para gestionar Barcos (Ships) en el sistema de logística
/// Proporciona endpoints para CRUD, carga/descarga, zarpe y anclaje
/// </summary>
[Route("api/[controller]")]
[ApiController]
public class ShipsController : ControllerBase
{
	private readonly IShipApplicationService _shipService;
	private readonly ILogger<ShipsController> _logger;

	public ShipsController(
		IShipApplicationService shipService,
		ILogger<ShipsController> logger)
	{
		_shipService = shipService ?? throw new ArgumentNullException(nameof(shipService));
		_logger = logger ?? throw new ArgumentNullException(nameof(logger));
	}

	/// <summary>
	/// Crea un nuevo barco con especificaciones de capacidad
	/// </summary>
	/// <remarks>
	/// Ejemplo de request:
	/// ```
	/// {
	///     "maxCapacity": 50000,
	///     "minCapacity": 30000,
	///     "spotShip": false
	/// }
	/// ```
	/// </remarks>
	[HttpPost]
	[ProducesResponseType(typeof(ApiResponse<ShipDetailDto>), StatusCodes.Status201Created)]
	[ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status400BadRequest)]
	public async Task<ActionResult<ApiResponse<ShipDetailDto>>> Create([FromBody] CreateShipDto request)
	{
		try
		{
			var result = await _shipService.CreateAsync(request);
			return CreatedAtAction(nameof(GetById), new { id = result.ShipId }, new ApiResponse<ShipDetailDto>
			{
				Success = true,
				Message = "Barco creado exitosamente",
				Data = result
			});
		}
		catch (ArgumentException ex)
		{
			_logger.LogWarning("Error de validación al crear barco: {Message}", ex.Message);
			return BadRequest(new ApiResponse<object>
			{
				Success = false,
				Error = ex.Message
			});
		}
		catch (Exception ex)
		{
			_logger.LogError(ex, "Error al crear barco");
			return StatusCode(500, new ApiResponse<object>
			{
				Success = false,
				Error = "Error interno del servidor"
			});
		}
	}

	/// <summary>
	/// Obtiene todos los barcos
	/// </summary>
	[HttpGet]
	[ProducesResponseType(typeof(ApiResponse<List<ShipDetailDto>>), StatusCodes.Status200OK)]
	public async Task<ActionResult<ApiResponse<List<ShipDetailDto>>>> GetAll()
	{
		try
		{
			var ships = (await _shipService.GetAllAsync()).ToList();
			return Ok(new ApiResponse<List<ShipDetailDto>>
			{
				Success = true,
				Message = $"Se encontraron {ships.Count} barcos",
				Data = ships
			});
		}
		catch (Exception ex)
		{
			_logger.LogError(ex, "Error al obtener barcos");
			return StatusCode(500, new ApiResponse<object>
			{
				Success = false,
				Error = "Error interno del servidor"
			});
		}
	}

	/// <summary>
	/// Obtiene un barco específico por ID con información detallada
	/// Incluye: peso total de carga, cantidad de ítems, capacidad disponible, estado
	/// </summary>
	[HttpGet("{id}")]
	[ProducesResponseType(typeof(ApiResponse<ShipDetailDto>), StatusCodes.Status200OK)]
	[ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status404NotFound)]
	public async Task<ActionResult<ApiResponse<ShipDetailDto>>> GetById(string id)
	{
		try
		{
			var ship = await _shipService.GetByIdAsync(id);
			return Ok(new ApiResponse<ShipDetailDto>
			{
				Success = true,
				Data = ship
			});
		}
		catch (KeyNotFoundException ex)
		{
			_logger.LogWarning("Barco no encontrado: {Id}", id);
			return NotFound(new ApiResponse<object>
			{
				Success = false,
				Error = ex.Message
			});
		}
		catch (Exception ex)
		{
			_logger.LogError(ex, "Error al obtener barco");
			return StatusCode(500, new ApiResponse<object>
			{
				Success = false,
				Error = "Error interno del servidor"
			});
		}
	}

	/// <summary>
	/// Actualiza un barco existente (ej. cambiar tipo a SpotShip)
	/// </summary>
	[HttpPut("{id}")]
	[ProducesResponseType(typeof(ApiResponse<ShipDetailDto>), StatusCodes.Status200OK)]
	[ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status404NotFound)]
	public async Task<ActionResult<ApiResponse<ShipDetailDto>>> Update(string id, [FromBody] UpdateShipDto request)
	{
		try
		{
			var result = await _shipService.UpdateAsync(id, request);
			return Ok(new ApiResponse<ShipDetailDto>
			{
				Success = true,
				Message = "Barco actualizado exitosamente",
				Data = result
			});
		}
		catch (KeyNotFoundException ex)
		{
			_logger.LogWarning("Barco no encontrado: {Id}", id);
			return NotFound(new ApiResponse<object>
			{
				Success = false,
				Error = ex.Message
			});
		}
		catch (Exception ex)
		{
			_logger.LogError(ex, "Error al actualizar barco");
			return StatusCode(500, new ApiResponse<object>
			{
				Success = false,
				Error = "Error interno del servidor"
			});
		}
	}

	/// <summary>
	/// Elimina un barco por ID
	/// </summary>
	[HttpDelete("{id}")]
	[ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status200OK)]
	[ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status404NotFound)]
	public async Task<ActionResult<ApiResponse<object>>> Delete(string id)
	{
		try
		{
			await _shipService.DeleteAsync(id);
			return Ok(new ApiResponse<object>
			{
				Success = true,
				Message = "Barco eliminado exitosamente"
			});
		}
		catch (KeyNotFoundException ex)
		{
			_logger.LogWarning("Barco no encontrado: {Id}", id);
			return NotFound(new ApiResponse<object>
			{
				Success = false,
				Error = ex.Message
			});
		}
		catch (Exception ex)
		{
			_logger.LogError(ex, "Error al eliminar barco");
			return StatusCode(500, new ApiResponse<object>
			{
				Success = false,
				Error = "Error interno del servidor"
			});
		}
	}

	/// <summary>
	/// Carga un contenedor/pallet/caja en el barco
	/// Valida que no exceda la capacidad máxima
	/// </summary>
	[HttpPost("{id}/load/{itemId}")]
	[ProducesResponseType(typeof(ApiResponse<ShipDetailDto>), StatusCodes.Status200OK)]
	[ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status400BadRequest)]
	[ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status404NotFound)]
	public async Task<ActionResult<ApiResponse<ShipDetailDto>>> LoadCargo(string id, string itemId)
	{
		try
		{
			var result = await _shipService.LoadCargoAsync(id, itemId);
			return Ok(new ApiResponse<ShipDetailDto>
			{
				Success = true,
				Message = "Carga cargada exitosamente en el barco",
				Data = result
			});
		}
		catch (KeyNotFoundException ex)
		{
			_logger.LogWarning("Recurso no encontrado: {Message}", ex.Message);
			return NotFound(new ApiResponse<object>
			{
				Success = false,
				Error = ex.Message
			});
		}
		catch (InvalidOperationException ex)
		{
			_logger.LogWarning("Error al cargar cargo: {Message}", ex.Message);
			return BadRequest(new ApiResponse<object>
			{
				Success = false,
				Error = ex.Message
			});
		}
		catch (Exception ex)
		{
			_logger.LogError(ex, "Error al cargar cargo en barco");
			return StatusCode(500, new ApiResponse<object>
			{
				Success = false,
				Error = "Error interno del servidor"
			});
		}
	}

	/// <summary>
	/// Descarga un contenedor/pallet/caja del barco
	/// </summary>
	[HttpPost("{id}/unload/{itemId}")]
	[ProducesResponseType(typeof(ApiResponse<ShipDetailDto>), StatusCodes.Status200OK)]
	[ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status400BadRequest)]
	[ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status404NotFound)]
	public async Task<ActionResult<ApiResponse<ShipDetailDto>>> UnloadCargo(string id, string itemId)
	{
		try
		{
			var result = await _shipService.UnloadCargoAsync(id, itemId);
			return Ok(new ApiResponse<ShipDetailDto>
			{
				Success = true,
				Message = "Carga descargada exitosamente del barco",
				Data = result
			});
		}
		catch (KeyNotFoundException ex)
		{
			_logger.LogWarning("Recurso no encontrado: {Message}", ex.Message);
			return NotFound(new ApiResponse<object>
			{
				Success = false,
				Error = ex.Message
			});
		}
		catch (InvalidOperationException ex)
		{
			_logger.LogWarning("Error al descargar cargo: {Message}", ex.Message);
			return BadRequest(new ApiResponse<object>
			{
				Success = false,
				Error = ex.Message
			});
		}
		catch (Exception ex)
		{
			_logger.LogError(ex, "Error al descargar cargo del barco");
			return StatusCode(500, new ApiResponse<object>
			{
				Success = false,
				Error = "Error interno del servidor"
			});
		}
	}

	/// <summary>
	/// Zarpa el barco (transición a estado "En Tránsito")
	/// Regla: El barco debe tener carga >= capacidad mínima
	/// </summary>
	[HttpPost("{id}/sail")]
	[ProducesResponseType(typeof(ApiResponse<ShipDetailDto>), StatusCodes.Status200OK)]
	[ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status400BadRequest)]
	[ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status404NotFound)]
	public async Task<ActionResult<ApiResponse<ShipDetailDto>>> Sail(string id)
	{
		try
		{
			var result = await _shipService.SailAsync(id);
			return Ok(new ApiResponse<ShipDetailDto>
			{
				Success = true,
				Message = "Barco ha zarpado exitosamente",
				Data = result
			});
		}
		catch (KeyNotFoundException ex)
		{
			_logger.LogWarning("Barco no encontrado: {Message}", ex.Message);
			return NotFound(new ApiResponse<object>
			{
				Success = false,
				Error = ex.Message
			});
		}
		catch (InvalidOperationException ex)
		{
			_logger.LogWarning("No se puede zarpar: {Message}", ex.Message);
			return BadRequest(new ApiResponse<object>
			{
				Success = false,
				Error = ex.Message
			});
		}
		catch (Exception ex)
		{
			_logger.LogError(ex, "Error al zarpar barco");
			return StatusCode(500, new ApiResponse<object>
			{
				Success = false,
				Error = "Error interno del servidor"
			});
		}
	}

	/// <summary>
	/// Ancla el barco (transición a estado "Disponible")
	/// Permite volver a recibir carga
	/// </summary>
	[HttpPost("{id}/anchor")]
	[ProducesResponseType(typeof(ApiResponse<ShipDetailDto>), StatusCodes.Status200OK)]
	[ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status400BadRequest)]
	[ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status404NotFound)]
	public async Task<ActionResult<ApiResponse<ShipDetailDto>>> Anchor(string id)
	{
		try
		{
			var result = await _shipService.AnchorAsync(id);
			return Ok(new ApiResponse<ShipDetailDto>
			{
				Success = true,
				Message = "Barco ha sido anclado exitosamente",
				Data = result
			});
		}
		catch (KeyNotFoundException ex)
		{
			_logger.LogWarning("Barco no encontrado: {Message}", ex.Message);
			return NotFound(new ApiResponse<object>
			{
				Success = false,
				Error = ex.Message
			});
		}
		catch (InvalidOperationException ex)
		{
			_logger.LogWarning("No se puede anclar: {Message}", ex.Message);
			return BadRequest(new ApiResponse<object>
			{
				Success = false,
				Error = ex.Message
			});
		}
		catch (Exception ex)
		{
			_logger.LogError(ex, "Error al anclar barco");
			return StatusCode(500, new ApiResponse<object>
			{
				Success = false,
				Error = "Error interno del servidor"
			});
		}
	}
}
