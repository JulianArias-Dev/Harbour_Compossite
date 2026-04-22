using Harbour.Application.DTOs;
using Harbour.Application.Services;
using Harbour.Domain;
using Microsoft.AspNetCore.Mvc;

namespace Harbour.Controllers;

/// <summary>
/// Controlador para operaciones de logística y gestión de almacenamiento
/// </summary>
[ApiController]
[Route("api/[controller]")]
[Produces("application/json")]
public class LogisticsController : ControllerBase
{
    private readonly ILogisticsService _logisticsService;
    private readonly ILogger<LogisticsController> _logger;

    public LogisticsController(ILogisticsService logisticsService, ILogger<LogisticsController> logger)
    {
        _logisticsService = logisticsService ?? throw new ArgumentNullException(nameof(logisticsService));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    /// <summary>
    /// Crea una nueva caja
    /// </summary>
    /// <param name="dto">Datos para crear la caja</param>
    /// <returns>Caja creada</returns>
    /// <response code="201">Caja creada exitosamente</response>
    /// <response code="400">Error de validación</response>
    /// <response code="500">Error interno del servidor</response>
    [HttpPost("boxes")]
    [ProducesResponseType(typeof(StorageItemWeightResponseDto), StatusCodes.Status201Created)]
    [ProducesResponseType(typeof(ErrorResponseDto), StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> CreateBox([FromBody] CreateBoxDto dto)
    {
        try
        {
            _logger.LogInformation("Creando caja: {@BoxDto}", dto);

            var box = await _logisticsService.CreateBoxAsync(dto.SelfWeight, dto.Destination);

            var response = new StorageItemWeightResponseDto(
                box.Id,
                box.GetType().Name,
                box.SelfWeight,
                box.GetTotalWeight());

            return CreatedAtAction(nameof(GetStorageItem), new { id = box.Id }, response);
        }
        catch (ArgumentException ex)
        {
            _logger.LogWarning("Error de validación al crear caja: {Message}", ex.Message);
            return BadRequest(new ErrorResponseDto(
                "Error de validación al crear la caja",
                "VALIDATION_ERROR",
                new Dictionary<string, string> { { "details", ex.Message } }));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error inesperado al crear caja");
            return StatusCode(500, new ErrorResponseDto(
                "Error interno del servidor",
                "INTERNAL_ERROR"));
        }
    }

    /// <summary>
    /// Crea un nuevo pallet
    /// </summary>
    /// <param name="dto">Datos para crear el pallet</param>
    /// <returns>Pallet creado</returns>
    /// <response code="201">Pallet creado exitosamente</response>
    /// <response code="400">Error de validación</response>
    /// <response code="500">Error interno del servidor</response>
    [HttpPost("pallets")]
    [ProducesResponseType(typeof(StorageItemWeightResponseDto), StatusCodes.Status201Created)]
    [ProducesResponseType(typeof(ErrorResponseDto), StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> CreatePallet([FromBody] CreatePalletDto dto)
    {
        try
        {
            _logger.LogInformation("Creando pallet: {@PalletDto}", dto);

            var pallet = await _logisticsService.CreatePalletAsync(dto.PalletTypeSpecId);

            var response = new StorageItemWeightResponseDto(
                pallet.Id,
                pallet.GetType().Name,
                pallet.SelfWeight,
                pallet.GetTotalWeight(),
                pallet.Contents.Count);

            return CreatedAtAction(nameof(GetStorageItem), new { id = pallet.Id }, response);
        }
        catch (ArgumentException ex)
        {
            _logger.LogWarning("Error de validación al crear pallet: {Message}", ex.Message);
            return BadRequest(new ErrorResponseDto(
                "Error de validación al crear el pallet",
                "VALIDATION_ERROR",
                new Dictionary<string, string> { { "details", ex.Message } }));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error inesperado al crear pallet");
            return StatusCode(500, new ErrorResponseDto(
                "Error interno del servidor",
                "INTERNAL_ERROR"));
        }
    }

    /// <summary>
    /// Crea un nuevo contenedor
    /// </summary>
    /// <param name="dto">Datos para crear el contenedor</param>
    /// <returns>Contenedor creado</returns>
    /// <response code="201">Contenedor creado exitosamente</response>
    /// <response code="400">Error de validación</response>
    /// <response code="500">Error interno del servidor</response>
    [HttpPost("containers")]
    [ProducesResponseType(typeof(StorageItemWeightResponseDto), StatusCodes.Status201Created)]
    [ProducesResponseType(typeof(ErrorResponseDto), StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> CreateContainer([FromBody] CreateContainerDto dto)
    {
        try
        {
            _logger.LogInformation("Creando contenedor: {@ContainerDto}", dto);

            var container = await _logisticsService.CreateContainerAsync(dto.ContainerTypeSpecId);

			var response = new StorageItemWeightResponseDto(
                container.Id,
                container.GetType().Name,
                container.SelfWeight,
                container.GetTotalWeight(),
                container.Contents.Count);

            return CreatedAtAction(nameof(GetStorageItem), new { id = container.Id }, response);
        }
        catch (ArgumentException ex)
        {
            _logger.LogWarning("Error de validación al crear contenedor: {Message}", ex.Message);
            return BadRequest(new ErrorResponseDto(
                "Error de validación al crear el contenedor",
                "VALIDATION_ERROR",
                new Dictionary<string, string> { { "details", ex.Message } }));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error inesperado al crear contenedor");
            return StatusCode(500, new ErrorResponseDto(
                "Error interno del servidor",
                "INTERNAL_ERROR"));
        }
    }

    /// <summary>
    /// Carga un elemento en un contenedor
    /// </summary>
    /// <param name="dto">Datos de la carga (ID del elemento hijo e ID del contenedor padre)</param>
    /// <returns>Resultado de la operación</returns>
    /// <response code="200">Elemento cargado exitosamente</response>
    /// <response code="400">Error de validación (peso excedido o elemento no encontrado)</response>
    /// <response code="500">Error interno del servidor</response>
    [HttpPut("load-item")]
    [ProducesResponseType(typeof(SuccessResponseDto), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ErrorResponseDto), StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> LoadItemToContainer([FromBody] LoadItemDto dto)
    {
        try
        {
            _logger.LogInformation("Cargando elemento {@LoadItemDto}", dto);

            var item = await _logisticsService.LoadItemToContainerAsync(dto.ChildItemId, dto.ParentItemId);

            var response = new SuccessResponseDto(
                "Elemento cargado exitosamente en el contenedor",
                new { itemId = item.Id, parentId = item.ParentId });

            return Ok(response);
        }
        catch (InvalidOperationException ex)
        {
            _logger.LogWarning("Error de negocio al cargar elemento: {Message}", ex.Message);
            return BadRequest(new ErrorResponseDto(
                ex.Message,
                "BUSINESS_RULE_VIOLATION",
                new Dictionary<string, string> 
                { 
                    { "childItemId", dto.ChildItemId },
                    { "parentItemId", dto.ParentItemId }
                }));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error inesperado al cargar elemento");
            return StatusCode(500, new ErrorResponseDto(
                "Error interno del servidor",
                "INTERNAL_ERROR"));
        }
    }

    /// <summary>
    /// Obtiene el peso total de un elemento
    /// </summary>
    /// <param name="id">ID del elemento</param>
    /// <returns>Información de peso del elemento</returns>
    /// <response code="200">Elemento encontrado</response>
    /// <response code="404">Elemento no encontrado</response>
    /// <response code="500">Error interno del servidor</response>
    [HttpGet("weight/{id}")]
    [ProducesResponseType(typeof(StorageItemWeightResponseDto), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ErrorResponseDto), StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetItemWeight(string id)
    {
        try
        {
            _logger.LogInformation("Obteniendo peso del elemento: {Id}", id);

            var item = await _logisticsService.GetStorageItemAsync(id);
            if (item == null)
            {
                return NotFound(new ErrorResponseDto(
                    $"El elemento con ID '{id}' no existe",
                    "NOT_FOUND"));
            }

            var itemsCount = item is CompositeStorage composite ? composite.Contents.Count : (int?)null;

            var response = new StorageItemWeightResponseDto(
                item.Id,
                item.GetType().Name,
                item.SelfWeight,
                item.GetTotalWeight(),
                itemsCount);

            return Ok(response);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error inesperado al obtener peso del elemento");
            return StatusCode(500, new ErrorResponseDto(
                "Error interno del servidor",
                "INTERNAL_ERROR"));
        }
    }

    /// <summary>
    /// Obtiene un elemento de almacenamiento completo
    /// </summary>
    /// <param name="id">ID del elemento</param>
    /// <returns>Información del elemento</returns>
    /// <response code="200">Elemento encontrado</response>
    /// <response code="404">Elemento no encontrado</response>
    [HttpGet("{id}")]
    [ProducesResponseType(typeof(object), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ErrorResponseDto), StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetStorageItem(string id)
    {
        try
        {
            _logger.LogInformation("Obteniendo elemento: {Id}", id);

            var item = await _logisticsService.GetStorageItemAsync(id);
            if (item == null)
            {
                return NotFound(new ErrorResponseDto(
                    $"El elemento con ID '{id}' no existe",
                    "NOT_FOUND"));
            }

            return Ok(item);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error inesperado al obtener elemento");
            return StatusCode(500, new ErrorResponseDto(
                "Error interno del servidor",
                "INTERNAL_ERROR"));
        }
    }

    /// <summary>
    /// Crea un nuevo barco
    /// </summary>
    /// <param name="dto">Datos para crear el barco</param>
    /// <returns>Barco creado</returns>
    /// <response code="201">Barco creado exitosamente</response>
    /// <response code="400">Error de validación</response>
    [HttpPost("ships")]
    [ProducesResponseType(typeof(SuccessResponseDto), StatusCodes.Status201Created)]
    [ProducesResponseType(typeof(ErrorResponseDto), StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> CreateShip([FromBody] CreateShipDto dto)
    {
        try
        {
            _logger.LogInformation("Creando barco: {@ShipDto}", dto);

            var ship = await _logisticsService.CreateShipAsync(dto.MaxCapacity, dto.MinCapacity, dto.SpotShip);

            var response = new SuccessResponseDto(
                "Barco creado exitosamente",
                new { shipId = ship.ShipId, maxCapacity = ship.MaxCapacity });

            return CreatedAtAction(nameof(GetShip), new { id = ship.ShipId }, response);
        }
        catch (ArgumentException ex)
        {
            _logger.LogWarning("Error de validación al crear barco: {Message}", ex.Message);
            return BadRequest(new ErrorResponseDto(
                "Error de validación al crear el barco",
                "VALIDATION_ERROR",
                new Dictionary<string, string> { { "details", ex.Message } }));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error inesperado al crear barco");
            return StatusCode(500, new ErrorResponseDto(
                "Error interno del servidor",
                "INTERNAL_ERROR"));
        }
    }

    /// <summary>
    /// Obtiene información de un barco
    /// </summary>
    /// <param name="id">ID del barco</param>
    /// <returns>Información del barco</returns>
    /// <response code="200">Barco encontrado</response>
    /// <response code="404">Barco no encontrado</response>
    [HttpGet("ships/{id}")]
    [ProducesResponseType(typeof(object), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ErrorResponseDto), StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetShip(string id)
    {
        try
        {
            _logger.LogInformation("Obteniendo barco: {Id}", id);

            var ship = await _logisticsService.GetShipAsync(id);
            if (ship == null)
            {
                return NotFound(new ErrorResponseDto(
                    $"El barco con ID '{id}' no existe",
                    "NOT_FOUND"));
            }

            return Ok(ship);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error inesperado al obtener barco");
            return StatusCode(500, new ErrorResponseDto(
                "Error interno del servidor",
                "INTERNAL_ERROR"));
        }
    }
}
