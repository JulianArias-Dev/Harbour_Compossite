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
    /// Carga un elemento en un contenedor
    /// </summary>
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
    [HttpGet("{id}")]
    [ProducesResponseType(typeof(StorageItemResponseDto), StatusCodes.Status200OK)]
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

            var response = new StorageItemResponseDto
            {
                Id = item.Id,
                Type = item.GetType().Name,
                SelfWeight = item.SelfWeight,
                TotalWeight = item.GetTotalWeight(),
                ParentId = item.ParentId,
                StatusId = item.StatusId
            };

            // Si es un contenedor composite, mapear sus contenidos
            if (item is CompositeStorage composite)
            {
                response.Contents = composite.Contents.Select(content => new StorageItemContentDto
                {
                    Id = content.Id,
                    Type = content.GetType().Name,
                    SelfWeight = content.SelfWeight,
                    TotalWeight = content.GetTotalWeight(),
                    ParentId = content.ParentId,
                    StatusId = content.StatusId
                }).ToList();

                // Agregar detalles específicos según el tipo
                if (item is Pallet pallet)
                {
                    response.Details = new
                    {
                        PalletTypeName = pallet.PalletTypeSpec?.Name,
                        TypeSpecification = pallet.PalletTypeSpec
                    };
                }
                else if (item is Container container)
                {
                    response.Details = new
                    {
                        ContainerTypeName = container.ContainerTypeSpec?.Name,
                        SizeInFeet = container.GetContainerSizeInFeet(),
                        IsRefrigerated = container.IsRefrigerated,
                        TypeSpecification = container.ContainerTypeSpec
                    };
                }
            }
            else if (item is Box box)
            {
                response.Details = new
                {
                    Destination = box.Destination
                };
            }

            return Ok(response);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error inesperado al obtener elemento");
            return StatusCode(500, new ErrorResponseDto(
                "Error interno del servidor",
                "INTERNAL_ERROR"));
        }
    }
    
}
