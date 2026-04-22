using Microsoft.AspNetCore.Mvc;

namespace Harbour.Controllers;

/// <summary>
/// Controlador REST para gestionar Pallets en el sistema de logística
/// Proporciona endpoints para CRUD, carga/descarga y consultas de peso/ítems
/// </summary>
[Route("api/[controller]")]
[ApiController]
public class PalletsController : ControllerBase
{
	private readonly IPalletApplicationService _palletService;
	private readonly ILogger<PalletsController> _logger;

	public PalletsController(
		IPalletApplicationService palletService,
		ILogger<PalletsController> logger)
	{
		_palletService = palletService ?? throw new ArgumentNullException(nameof(palletService));
		_logger = logger ?? throw new ArgumentNullException(nameof(logger));
	}

	/// <summary>
	/// Crea un nuevo pallet basado en un tipo de especificación
	/// </summary>
	[HttpPost]
	[ProducesResponseType(typeof(ApiResponse<PalletDetailDto>), StatusCodes.Status201Created)]
	[ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status400BadRequest)]
	public async Task<ActionResult<ApiResponse<PalletDetailDto>>> Create([FromBody] CreatePalletDto request)
	{
		try
		{
			var result = await _palletService.CreateAsync(request);
			return CreatedAtAction(nameof(GetById), new { id = result.Id }, new ApiResponse<PalletDetailDto>
			{
				Success = true,
				Message = "Pallet creado exitosamente",
				Data = result
			});
		}
		catch (KeyNotFoundException ex)
		{
			_logger.LogWarning("Especificación de pallet no encontrada: {Message}", ex.Message);
			return BadRequest(new ApiResponse<object>
			{
				Success = false,
				Error = ex.Message
			});
		}
		catch (ArgumentException ex)
		{
			_logger.LogWarning("Error de validación al crear pallet: {Message}", ex.Message);
			return BadRequest(new ApiResponse<object>
			{
				Success = false,
				Error = ex.Message
			});
		}
		catch (Exception ex)
		{
			_logger.LogError(ex, "Error al crear pallet");
			return StatusCode(500, new ApiResponse<object>
			{
				Success = false,
				Error = "Error interno del servidor"
			});
		}
	}

	/// <summary>
	/// Obtiene todos los pallets
	/// </summary>
	[HttpGet]
	[ProducesResponseType(typeof(ApiResponse<List<PalletDetailDto>>), StatusCodes.Status200OK)]
	public async Task<ActionResult<ApiResponse<List<PalletDetailDto>>>> GetAll()
	{
		try
		{
			var pallets = (await _palletService.GetAllAsync()).ToList();
			return Ok(new ApiResponse<List<PalletDetailDto>>
			{
				Success = true,
				Message = $"Se encontraron {pallets.Count} pallets",
				Data = pallets
			});
		}
		catch (Exception ex)
		{
			_logger.LogError(ex, "Error al obtener pallets");
			return StatusCode(500, new ApiResponse<object>
			{
				Success = false,
				Error = "Error interno del servidor"
			});
		}
	}

	/// <summary>
	/// Obtiene un pallet específico por ID con información detallada
	/// Incluye: número total de ítems, SelfWeight y TotalWeight (peso propio + carga)
	/// </summary>
	[HttpGet("{id}")]
	[ProducesResponseType(typeof(ApiResponse<PalletDetailDto>), StatusCodes.Status200OK)]
	[ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status404NotFound)]
	public async Task<ActionResult<ApiResponse<PalletDetailDto>>> GetById(string id)
	{
		try
		{
			var pallet = await _palletService.GetByIdAsync(id);
			return Ok(new ApiResponse<PalletDetailDto>
			{
				Success = true,
				Data = pallet
			});
		}
		catch (KeyNotFoundException ex)
		{
			_logger.LogWarning("Pallet no encontrado: {Id}", id);
			return NotFound(new ApiResponse<object>
			{
				Success = false,
				Error = ex.Message
			});
		}
		catch (Exception ex)
		{
			_logger.LogError(ex, "Error al obtener pallet");
			return StatusCode(500, new ApiResponse<object>
			{
				Success = false,
				Error = "Error interno del servidor"
			});
		}
	}

	/// <summary>
	/// Actualiza un pallet existente (ej. cambiar estado)
	/// </summary>
	[HttpPut("{id}")]
	[ProducesResponseType(typeof(ApiResponse<PalletDetailDto>), StatusCodes.Status200OK)]
	[ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status404NotFound)]
	public async Task<ActionResult<ApiResponse<PalletDetailDto>>> Update(string id, [FromBody] UpdatePalletDto request)
	{
		try
		{
			var result = await _palletService.UpdateAsync(id, request);
			return Ok(new ApiResponse<PalletDetailDto>
			{
				Success = true,
				Message = "Pallet actualizado exitosamente",
				Data = result
			});
		}
		catch (KeyNotFoundException ex)
		{
			_logger.LogWarning("Pallet no encontrado: {Id}", id);
			return NotFound(new ApiResponse<object>
			{
				Success = false,
				Error = ex.Message
			});
		}
		catch (Exception ex)
		{
			_logger.LogError(ex, "Error al actualizar pallet");
			return StatusCode(500, new ApiResponse<object>
			{
				Success = false,
				Error = "Error interno del servidor"
			});
		}
	}

	/// <summary>
	/// Elimina un pallet por ID
	/// Regla: No se puede eliminar si está cargado en un contenedor
	/// </summary>
	[HttpDelete("{id}")]
	[ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status200OK)]
	[ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status404NotFound)]
	[ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status400BadRequest)]
	public async Task<ActionResult<ApiResponse<object>>> Delete(string id)
	{
		try
		{
			await _palletService.DeleteAsync(id);
			return Ok(new ApiResponse<object>
			{
				Success = true,
				Message = "Pallet eliminado exitosamente"
			});
		}
		catch (KeyNotFoundException ex)
		{
			_logger.LogWarning("Pallet no encontrado: {Id}", id);
			return NotFound(new ApiResponse<object>
			{
				Success = false,
				Error = ex.Message
			});
		}
		catch (InvalidOperationException ex)
		{
			_logger.LogWarning("No se puede eliminar pallet cargado: {Id}", id);
			return BadRequest(new ApiResponse<object>
			{
				Success = false,
				Error = ex.Message
			});
		}
		catch (Exception ex)
		{
			_logger.LogError(ex, "Error al eliminar pallet");
			return StatusCode(500, new ApiResponse<object>
			{
				Success = false,
				Error = "Error interno del servidor"
			});
		}
	}

	/// <summary>
	/// Carga un objeto específico dentro de un pallet
	/// Valida: capacidad máxima, restricción de sellado, etc.
	/// </summary>
	[HttpPost("{id}/load/{itemId}")]
	[ProducesResponseType(typeof(ApiResponse<PalletDetailDto>), StatusCodes.Status200OK)]
	[ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status400BadRequest)]
	[ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status404NotFound)]
	public async Task<ActionResult<ApiResponse<PalletDetailDto>>> LoadItem(string id, string itemId)
	{
		try
		{
			var result = await _palletService.LoadItemAsync(id, itemId);
			return Ok(new ApiResponse<PalletDetailDto>
			{
				Success = true,
				Message = "Objeto cargado exitosamente en el pallet",
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
			_logger.LogWarning("Error al cargar objeto: {Message}", ex.Message);
			return BadRequest(new ApiResponse<object>
			{
				Success = false,
				Error = ex.Message
			});
		}
		catch (Exception ex)
		{
			_logger.LogError(ex, "Error al cargar objeto en pallet");
			return StatusCode(500, new ApiResponse<object>
			{
				Success = false,
				Error = "Error interno del servidor"
			});
		}
	}

	/// <summary>
	/// Descarga un objeto específico del pallet
	/// </summary>
	[HttpPost("{id}/unload/{itemId}")]
	[ProducesResponseType(typeof(ApiResponse<PalletDetailDto>), StatusCodes.Status200OK)]
	[ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status400BadRequest)]
	[ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status404NotFound)]
	public async Task<ActionResult<ApiResponse<PalletDetailDto>>> UnloadItem(string id, string itemId)
	{
		try
		{
			var result = await _palletService.UnloadItemAsync(id, itemId);
			return Ok(new ApiResponse<PalletDetailDto>
			{
				Success = true,
				Message = "Objeto descargado exitosamente del pallet",
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
			_logger.LogWarning("Error al descargar objeto: {Message}", ex.Message);
			return BadRequest(new ApiResponse<object>
			{
				Success = false,
				Error = ex.Message
			});
		}
		catch (Exception ex)
		{
			_logger.LogError(ex, "Error al descargar objeto del pallet");
			return StatusCode(500, new ApiResponse<object>
			{
				Success = false,
				Error = "Error interno del servidor"
			});
		}
	}
}
