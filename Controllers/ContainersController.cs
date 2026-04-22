using Microsoft.AspNetCore.Mvc;

namespace Harbour.Controllers;

/// <summary>
/// Controlador REST para gestionar Contenedores en el sistema de logística
/// Proporciona endpoints para CRUD, carga/descarga y consultas de peso/ítems
/// </summary>
[Route("api/[controller]")]
[ApiController]
public class ContainersController : ControllerBase
{
	private readonly IContainerApplicationService _containerService;
	private readonly ILogger<ContainersController> _logger;

	public ContainersController(
		IContainerApplicationService containerService,
		ILogger<ContainersController> logger)
	{
		_containerService = containerService ?? throw new ArgumentNullException(nameof(containerService));
		_logger = logger ?? throw new ArgumentNullException(nameof(logger));
	}

	/// <summary>
	/// Crea un nuevo contenedor basado en un tipo de especificación
	/// </summary>
	[HttpPost]
	[ProducesResponseType(typeof(ApiResponse<ContainerDetailDto>), StatusCodes.Status201Created)]
	[ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status400BadRequest)]
	public async Task<ActionResult<ApiResponse<ContainerDetailDto>>> Create([FromBody] CreateContainerDto request)
	{
		try
		{
			var result = await _containerService.CreateAsync(request);
			return CreatedAtAction(nameof(GetById), new { id = result.Id }, new ApiResponse<ContainerDetailDto>
			{
				Success = true,
				Message = "Contenedor creado exitosamente",
				Data = result
			});
		}
		catch (KeyNotFoundException ex)
		{
			_logger.LogWarning("Especificación de contenedor no encontrada: {Message}", ex.Message);
			return BadRequest(new ApiResponse<object>
			{
				Success = false,
				Error = ex.Message
			});
		}
		catch (ArgumentException ex)
		{
			_logger.LogWarning("Error de validación al crear contenedor: {Message}", ex.Message);
			return BadRequest(new ApiResponse<object>
			{
				Success = false,
				Error = ex.Message
			});
		}
		catch (Exception ex)
		{
			_logger.LogError(ex, "Error al crear contenedor");
			return StatusCode(500, new ApiResponse<object>
			{
				Success = false,
				Error = "Error interno del servidor"
			});
		}
	}

	/// <summary>
	/// Obtiene todos los contenedores
	/// </summary>
	[HttpGet]
	[ProducesResponseType(typeof(ApiResponse<List<ContainerDetailDto>>), StatusCodes.Status200OK)]
	public async Task<ActionResult<ApiResponse<List<ContainerDetailDto>>>> GetAll()
	{
		try
		{
			var containers = (await _containerService.GetAllAsync()).ToList();
			return Ok(new ApiResponse<List<ContainerDetailDto>>
			{
				Success = true,
				Message = $"Se encontraron {containers.Count} contenedores",
				Data = containers
			});
		}
		catch (Exception ex)
		{
			_logger.LogError(ex, "Error al obtener contenedores");
			return StatusCode(500, new ApiResponse<object>
			{
				Success = false,
				Error = "Error interno del servidor"
			});
		}
	}

	/// <summary>
	/// Obtiene un contenedor específico por ID con información detallada
	/// Incluye: número total de ítems, SelfWeight, TotalWeight (peso propio + carga), tamaño y tipo
	/// </summary>
	[HttpGet("{id}")]
	[ProducesResponseType(typeof(ApiResponse<ContainerDetailDto>), StatusCodes.Status200OK)]
	[ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status404NotFound)]
	public async Task<ActionResult<ApiResponse<ContainerDetailDto>>> GetById(string id)
	{
		try
		{
			var container = await _containerService.GetByIdAsync(id);
			return Ok(new ApiResponse<ContainerDetailDto>
			{
				Success = true,
				Data = container
			});
		}
		catch (KeyNotFoundException ex)
		{
			_logger.LogWarning("Contenedor no encontrado: {Id}", id);
			return NotFound(new ApiResponse<object>
			{
				Success = false,
				Error = ex.Message
			});
		}
		catch (Exception ex)
		{
			_logger.LogError(ex, "Error al obtener contenedor");
			return StatusCode(500, new ApiResponse<object>
			{
				Success = false,
				Error = "Error interno del servidor"
			});
		}
	}

	/// <summary>
	/// Actualiza un contenedor existente (ej. cambiar estado)
	/// </summary>
	[HttpPut("{id}")]
	[ProducesResponseType(typeof(ApiResponse<ContainerDetailDto>), StatusCodes.Status200OK)]
	[ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status404NotFound)]
	public async Task<ActionResult<ApiResponse<ContainerDetailDto>>> Update(string id, [FromBody] UpdateContainerDto request)
	{
		try
		{
			var result = await _containerService.UpdateAsync(id, request);
			return Ok(new ApiResponse<ContainerDetailDto>
			{
				Success = true,
				Message = "Contenedor actualizado exitosamente",
				Data = result
			});
		}
		catch (KeyNotFoundException ex)
		{
			_logger.LogWarning("Contenedor no encontrado: {Id}", id);
			return NotFound(new ApiResponse<object>
			{
				Success = false,
				Error = ex.Message
			});
		}
		catch (Exception ex)
		{
			_logger.LogError(ex, "Error al actualizar contenedor");
			return StatusCode(500, new ApiResponse<object>
			{
				Success = false,
				Error = "Error interno del servidor"
			});
		}
	}

	/// <summary>
	/// Elimina un contenedor por ID
	/// Regla: No se puede eliminar si está cargado en un barco
	/// </summary>
	[HttpDelete("{id}")]
	[ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status200OK)]
	[ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status404NotFound)]
	[ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status400BadRequest)]
	public async Task<ActionResult<ApiResponse<object>>> Delete(string id)
	{
		try
		{
			await _containerService.DeleteAsync(id);
			return Ok(new ApiResponse<object>
			{
				Success = true,
				Message = "Contenedor eliminado exitosamente"
			});
		}
		catch (KeyNotFoundException ex)
		{
			_logger.LogWarning("Contenedor no encontrado: {Id}", id);
			return NotFound(new ApiResponse<object>
			{
				Success = false,
				Error = ex.Message
			});
		}
		catch (InvalidOperationException ex)
		{
			_logger.LogWarning("No se puede eliminar contenedor cargado: {Id}", id);
			return BadRequest(new ApiResponse<object>
			{
				Success = false,
				Error = ex.Message
			});
		}
		catch (Exception ex)
		{
			_logger.LogError(ex, "Error al eliminar contenedor");
			return StatusCode(500, new ApiResponse<object>
			{
				Success = false,
				Error = "Error interno del servidor"
			});
		}
	}

	/// <summary>
	/// Carga un objeto específico dentro de un contenedor
	/// Valida: capacidad máxima, restricción de sellado, etc.
	/// </summary>
	[HttpPost("{id}/load/{itemId}")]
	[ProducesResponseType(typeof(ApiResponse<ContainerDetailDto>), StatusCodes.Status200OK)]
	[ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status400BadRequest)]
	[ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status404NotFound)]
	public async Task<ActionResult<ApiResponse<ContainerDetailDto>>> LoadItem(string id, string itemId)
	{
		try
		{
			var result = await _containerService.LoadItemAsync(id, itemId);
			return Ok(new ApiResponse<ContainerDetailDto>
			{
				Success = true,
				Message = "Objeto cargado exitosamente en el contenedor",
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
			_logger.LogError(ex, "Error al cargar objeto en contenedor");
			return StatusCode(500, new ApiResponse<object>
			{
				Success = false,
				Error = "Error interno del servidor"
			});
		}
	}

	/// <summary>
	/// Descarga un objeto específico del contenedor
	/// </summary>
	[HttpPost("{id}/unload/{itemId}")]
	[ProducesResponseType(typeof(ApiResponse<ContainerDetailDto>), StatusCodes.Status200OK)]
	[ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status400BadRequest)]
	[ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status404NotFound)]
	public async Task<ActionResult<ApiResponse<ContainerDetailDto>>> UnloadItem(string id, string itemId)
	{
		try
		{
			var result = await _containerService.UnloadItemAsync(id, itemId);
			return Ok(new ApiResponse<ContainerDetailDto>
			{
				Success = true,
				Message = "Objeto descargado exitosamente del contenedor",
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
			_logger.LogError(ex, "Error al descargar objeto del contenedor");
			return StatusCode(500, new ApiResponse<object>
			{
				Success = false,
				Error = "Error interno del servidor"
			});
		}
	}
}
