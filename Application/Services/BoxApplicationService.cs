namespace Harbour.Application.Services;

/// <summary>
/// Servicio de aplicación para operaciones de Box
/// Implementa lógica de negocio para CRUD y operaciones compuestas
/// </summary>
public interface IBoxApplicationService
{
	Task<BoxDto> CreateAsync(CreateBoxDto request);
	Task<BoxDto> GetByIdAsync(string id);
	Task<IEnumerable<BoxDto>> GetAllAsync();
	Task<BoxDto> UpdateAsync(string id, UpdateBoxDto request);
	Task DeleteAsync(string id);
}

public class BoxApplicationService : IBoxApplicationService
{
	private readonly IStorageRepository _storageRepository;
	private readonly IMapper _mapper;

	public BoxApplicationService(
		IStorageRepository storageRepository,
		IMapper mapper)
	{
		_storageRepository = storageRepository ?? throw new ArgumentNullException(nameof(storageRepository));
		_mapper = mapper ?? throw new ArgumentNullException(nameof(mapper));
	}

	public async Task<BoxDto> CreateAsync(CreateBoxDto request)
	{
		var box = new Box(request.SelfWeight, request.Destination);
		var created = await _storageRepository.AddAsync(box);
		return _mapper.Map<BoxDto>(created);
	}

	public async Task<BoxDto> GetByIdAsync(string id)
	{
		var box = await _storageRepository.GetByIdAsync(id);
		if (box == null)
			throw new KeyNotFoundException($"Box con ID '{id}' no encontrado");

		return _mapper.Map<BoxDto>(box);
	}

	public async Task<IEnumerable<BoxDto>> GetAllAsync()
	{
		var boxes = await _storageRepository.GetByTypeAsync<Box>();
		return boxes.Select(b => _mapper.Map<BoxDto>(b));
	}

	public async Task<BoxDto> UpdateAsync(string id, UpdateBoxDto request)
	{
		var box = await _storageRepository.GetByIdAsync(id) as Box
			?? throw new KeyNotFoundException($"Box con ID '{id}' no encontrado");

		if (!string.IsNullOrWhiteSpace(request.Destination))
		{
			box.Destination = request.Destination;
		}

		var updated = await _storageRepository.UpdateAsync(box);
		return _mapper.Map<BoxDto>(updated);
	}

	public async Task DeleteAsync(string id)
	{
		var box = await _storageRepository.GetByIdAsync(id) as Box
			?? throw new KeyNotFoundException($"Box con ID '{id}' no encontrado");

		// Regla 4: Validar que no está cargado
		box.ValidateNotLoaded();

		await _storageRepository.DeleteAsync(id);
	}
}
