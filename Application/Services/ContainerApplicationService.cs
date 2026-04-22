namespace Harbour.Application.Services;

/// <summary>
/// Servicio de aplicación para operaciones de Container
/// Implementa lógica de negocio para CRUD, carga/descarga con DTOs ricos
/// </summary>
public interface IContainerApplicationService
{
	Task<ContainerDetailDto> CreateAsync(CreateContainerDto request);
	Task<ContainerDetailDto> GetByIdAsync(string id);
	Task<IEnumerable<ContainerDetailDto>> GetAllAsync();
	Task<ContainerDetailDto> UpdateAsync(string id, UpdateContainerDto request);
	Task DeleteAsync(string id);
	Task<ContainerDetailDto> LoadItemAsync(string containerId, string itemId);
	Task<ContainerDetailDto> UnloadItemAsync(string containerId, string itemId);
}

public class ContainerApplicationService : IContainerApplicationService
{
	private readonly ICompositeStorageRepository<Container> _containerRepository;
	private readonly IStorageRepository _storageRepository;
	private readonly IMapper _mapper;

	public ContainerApplicationService(
		ICompositeStorageRepository<Container> containerRepository,
		IStorageRepository storageRepository,
		IMapper mapper)
	{
		_containerRepository = containerRepository ?? throw new ArgumentNullException(nameof(containerRepository));
		_storageRepository = storageRepository ?? throw new ArgumentNullException(nameof(storageRepository));
		_mapper = mapper ?? throw new ArgumentNullException(nameof(mapper));
	}

	public async Task<ContainerDetailDto> CreateAsync(CreateContainerDto request)
	{
		var spec = await _storageRepository.GetContainerTypeSpecByIdAsync(request.ContainerTypeSpecId)
			?? throw new KeyNotFoundException($"ContainerTypeSpec con ID '{request.ContainerTypeSpecId}' no encontrado");

		var container = new Container(spec);
		var created = await _containerRepository.AddAsync(container);
		return MapToDetailDtoAsync(created);
	}

	public async Task<ContainerDetailDto> GetByIdAsync(string id)
	{
		var container = await _containerRepository.GetWithContentsAsync(id)
			?? throw new KeyNotFoundException($"Container con ID '{id}' no encontrado");

		return MapToDetailDtoAsync(container);
	}

	public async Task<IEnumerable<ContainerDetailDto>> GetAllAsync()
	{
		var containers = await _containerRepository.GetAllAsync();
		var result = new List<ContainerDetailDto>();

		foreach (var container in containers)
		{
			var detailed = await _containerRepository.GetWithContentsAsync(container.Id);
			if (detailed != null)
			{
				result.Add(MapToDetailDtoAsync(detailed));
			}
		}

		return result;
	}

	public async Task<ContainerDetailDto> UpdateAsync(string id, UpdateContainerDto request)
	{
		var container = await _containerRepository.GetByIdAsync(id)
			?? throw new KeyNotFoundException($"Container con ID '{id}' no encontrado");

		if (request.StatusId.HasValue)
		{
			container.StatusId = request.StatusId.Value;
		}

		var updated = await _containerRepository.UpdateAsync(container);
		return MapToDetailDtoAsync(updated);
	}

	public async Task DeleteAsync(string id)
	{
		// Regla 4: Validar que no está cargado
		await _containerRepository.DeleteAsync(id);
	}

	public async Task<ContainerDetailDto> LoadItemAsync(string containerId, string itemId)
	{
		await _containerRepository.LoadItemAsync(containerId, itemId);
		var updated = await _containerRepository.GetWithContentsAsync(containerId);
		return MapToDetailDtoAsync(updated!);
	}

	public async Task<ContainerDetailDto> UnloadItemAsync(string containerId, string itemId)
	{
		await _containerRepository.UnloadItemAsync(containerId, itemId);
		var updated = await _containerRepository.GetWithContentsAsync(containerId);
		return MapToDetailDtoAsync(updated!);
	}

	private ContainerDetailDto MapToDetailDtoAsync(Container container)
	{
		var dto = new ContainerDetailDto
		{
			Id = container.Id,
			ContainerTypeName = container.ContainerTypeSpec?.Name,
			ItemCount = container.GetItemCount(),
			SelfWeightKg = container.SelfWeight,
			TotalWeightKg = container.GetTotalWeight(),
			SizeInFeet = container.GetContainerSizeInFeet(),
			IsRefrigerated = container.IsRefrigerated,
			TypeSpecification = container.ContainerTypeSpec != null ? _mapper.Map<ContainerTypeSpecDto>(container.ContainerTypeSpec) : null
		};

		return dto;
	}
}
