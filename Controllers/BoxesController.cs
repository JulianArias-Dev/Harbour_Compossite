using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace Harbour.Controllers;

/// <summary>
/// Controlador REST para gestionar Cajas (Boxes) en el sistema de logística
/// Proporciona endpoints para CRUD de cajas individuales
/// </summary>
[Route("api/[controller]")]
[ApiController]
public class BoxesController : ControllerBase
{
	private readonly IBoxApplicationService _boxService;
	private readonly ILogger<BoxesController> _logger;

	public BoxesController(
		IBoxApplicationService boxService,
		ILogger<BoxesController> logger)
	{
		_boxService = boxService ?? throw new ArgumentNullException(nameof(boxService));
		_logger = logger ?? throw new ArgumentNullException(nameof(logger));
	}

	/// <summary>
	/// Crea una nueva caja
	/// </summary>
	[HttpPost]
	[ProducesResponseType(typeof(ApiResponse<BoxDto>), StatusCodes.Status201Created)]
	[ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status400BadRequest)]
	public async Task<ActionResult<ApiResponse<BoxDto>>> Create([FromBody] CreateBoxDto request)
	{
		try
		{
			var result = await _boxService.CreateAsync(request);
			return CreatedAtAction(nameof(GetById), new { id = result.Id }, new ApiResponse<BoxDto>
			{
				Success = true,
				Message = "Caja creada exitosamente",
				Data = result
			});
		}
		catch (ArgumentException ex)
		{
			_logger.LogWarning("Error de validación al crear caja: {Message}", ex.Message);
			return BadRequest(new ApiResponse<object>
			{
				Success = false,
				Error = ex.Message
			});
		}
		catch (Exception ex)
		{
			_logger.LogError(ex, "Error al crear caja");
			return StatusCode(500, new ApiResponse<object>
			{
				Success = false,
				Error = "Error interno del servidor"
			});
		}
	}

	/// <summary>
	/// Obtiene todas las cajas
	/// </summary>
	[HttpGet]
	[ProducesResponseType(typeof(ApiResponse<List<BoxDto>>), StatusCodes.Status200OK)]
	public async Task<ActionResult<ApiResponse<List<BoxDto>>>> GetAll()
	{
		try
		{
			var boxes = (await _boxService.GetAllAsync()).ToList();
			return Ok(new ApiResponse<List<BoxDto>>
			{
				Success = true,
				Message = $"Se encontraron {boxes.Count} cajas",
				Data = boxes
			});
		}
		catch (Exception ex)
		{
			_logger.LogError(ex, "Error al obtener cajas");
			return StatusCode(500, new ApiResponse<object>
			{
				Success = false,
				Error = "Error interno del servidor"
			});
		}
	}

	/// <summary>
	/// Obtiene una caja específica por ID
	/// </summary>
	[HttpGet("{id}")]
	[ProducesResponseType(typeof(ApiResponse<BoxDto>), StatusCodes.Status200OK)]
	[ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status404NotFound)]
	public async Task<ActionResult<ApiResponse<BoxDto>>> GetById(string id)
	{
		try
		{
			var box = await _boxService.GetByIdAsync(id);
			return Ok(new ApiResponse<BoxDto>
			{
				Success = true,
				Data = box
			});
		}
		catch (KeyNotFoundException ex)
		{
			_logger.LogWarning("Caja no encontrada: {Id}", id);
			return NotFound(new ApiResponse<object>
			{
				Success = false,
				Error = ex.Message
			});
		}
		catch (Exception ex)
		{
			_logger.LogError(ex, "Error al obtener caja");
			return StatusCode(500, new ApiResponse<object>
			{
				Success = false,
				Error = "Error interno del servidor"
			});
		}
	}

	/// <summary>
	/// Actualiza una caja existente (ej. cambiar destino)
	/// </summary>
	[HttpPut("{id}")]
	[ProducesResponseType(typeof(ApiResponse<BoxDto>), StatusCodes.Status200OK)]
	[ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status404NotFound)]
	public async Task<ActionResult<ApiResponse<BoxDto>>> Update(string id, [FromBody] UpdateBoxDto request)
	{
		try
		{
			var result = await _boxService.UpdateAsync(id, request);
			return Ok(new ApiResponse<BoxDto>
			{
				Success = true,
				Message = "Caja actualizada exitosamente",
				Data = result
			});
		}
		catch (KeyNotFoundException ex)
		{
			_logger.LogWarning("Caja no encontrada: {Id}", id);
			return NotFound(new ApiResponse<object>
			{
				Success = false,
				Error = ex.Message
			});
		}
		catch (Exception ex)
		{
			_logger.LogError(ex, "Error al actualizar caja");
			return StatusCode(500, new ApiResponse<object>
			{
				Success = false,
				Error = "Error interno del servidor"
			});
		}
	}

	/// <summary>
	/// Elimina una caja por ID
	/// Regla: No se puede eliminar si está cargada en un contenedor
	/// </summary>
	[HttpDelete("{id}")]
	[ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status200OK)]
	[ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status404NotFound)]
	[ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status400BadRequest)]
	public async Task<ActionResult<ApiResponse<object>>> Delete(string id)
	{
		try
		{
			await _boxService.DeleteAsync(id);
			return Ok(new ApiResponse<object>
			{
				Success = true,
				Message = "Caja eliminada exitosamente"
			});
		}
		catch (KeyNotFoundException ex)
		{
			_logger.LogWarning("Caja no encontrada: {Id}", id);
			return NotFound(new ApiResponse<object>
			{
				Success = false,
				Error = ex.Message
			});
		}
		catch (InvalidOperationException ex)
		{
			_logger.LogWarning("No se puede eliminar caja cargada: {Id}, {Message}", id, ex.Message);
			return BadRequest(new ApiResponse<object>
			{
				Success = false,
				Error = ex.Message
			});
		}
		catch (Exception ex)
		{
			_logger.LogError(ex, "Error al eliminar caja");
			return StatusCode(500, new ApiResponse<object>
			{
				Success = false,
				Error = "Error interno del servidor"
			});
		}
	}
}
