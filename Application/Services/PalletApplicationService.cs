namespace Harbour.Application.Services;

/// <summary>
/// Servicio de aplicación para operaciones de Pallet
/// Implementa lógica de negocio para CRUD, carga/descarga con DTOs ricos
/// </summary>
public interface IPalletApplicationService
{
	Task<PalletDetailDto> CreateAsync(CreatePalletDto request);
	Task<PalletDetailDto> GetByIdAsync(string id);
	Task<IEnumerable<PalletDetailDto>> GetAllAsync();
	Task<PalletDetailDto> UpdateAsync(string id, UpdatePalletDto request);
	Task DeleteAsync(string id);
	Task<PalletDetailDto> LoadItemAsync(string palletId, string itemId);
	Task<PalletDetailDto> UnloadItemAsync(string palletId, string itemId);
}

public class PalletApplicationService : IPalletApplicationService
{
	private readonly ICompositeStorageRepository<Pallet> _palletRepository;
	private readonly IStorageRepository _storageRepository;
	private readonly IMapper _mapper;

	public PalletApplicationService(
		ICompositeStorageRepository<Pallet> palletRepository,
		IStorageRepository storageRepository,
		IMapper mapper)
	{
		_palletRepository = palletRepository ?? throw new ArgumentNullException(nameof(palletRepository));
		_storageRepository = storageRepository ?? throw new ArgumentNullException(nameof(storageRepository));
		_mapper = mapper ?? throw new ArgumentNullException(nameof(mapper));
	}

	public async Task<PalletDetailDto> CreateAsync(CreatePalletDto request)
	{
		var spec = await _storageRepository.GetPalletTypeSpecByIdAsync(request.PalletTypeSpecId)
			?? throw new KeyNotFoundException($"PalletTypeSpec con ID '{request.PalletTypeSpecId}' no encontrado");

		var pallet = new Pallet(spec);
		var created = await _palletRepository.AddAsync(pallet);
		return MapToDetailDtoAsync(created);
	}

	public async Task<PalletDetailDto> GetByIdAsync(string id)
	{
		var pallet = await _palletRepository.GetWithContentsAsync(id)
			?? throw new KeyNotFoundException($"Pallet con ID '{id}' no encontrado");

		return MapToDetailDtoAsync(pallet);
	}

	public async Task<IEnumerable<PalletDetailDto>> GetAllAsync()
	{
		var pallets = await _palletRepository.GetAllAsync();
		var result = new List<PalletDetailDto>();

		foreach (var pallet in pallets)
		{
			var detailed = await _palletRepository.GetWithContentsAsync(pallet.Id);
			if (detailed != null)
			{
				result.Add(MapToDetailDtoAsync(detailed));
			}
		}

		return result;
	}

	public async Task<PalletDetailDto> UpdateAsync(string id, UpdatePalletDto request)
	{
		var pallet = await _palletRepository.GetByIdAsync(id)
			?? throw new KeyNotFoundException($"Pallet con ID '{id}' no encontrado");

		if (request.StatusId.HasValue)
		{
			pallet.StatusId = request.StatusId.Value;
		}

		var updated = await _palletRepository.UpdateAsync(pallet);
		return MapToDetailDtoAsync(updated);
	}

	public async Task DeleteAsync(string id)
	{
		// Regla 4: Validar que no está cargado
		await _palletRepository.DeleteAsync(id);
	}

	public async Task<PalletDetailDto> LoadItemAsync(string palletId, string itemId)
	{
		await _palletRepository.LoadItemAsync(palletId, itemId);
		var updated = await _palletRepository.GetWithContentsAsync(palletId);
		return MapToDetailDtoAsync(updated!);
	}

	public async Task<PalletDetailDto> UnloadItemAsync(string palletId, string itemId)
	{
		await _palletRepository.UnloadItemAsync(palletId, itemId);
		var updated = await _palletRepository.GetWithContentsAsync(palletId);
		return  MapToDetailDtoAsync(updated!);
	}

	private PalletDetailDto MapToDetailDtoAsync(Pallet pallet)
	{
		var dto = new PalletDetailDto
		{
			Id = pallet.Id,
			PalletTypeName = pallet.PalletTypeSpec?.Name,
			ItemCount = pallet.GetItemCount(),
			SelfWeightKg = pallet.SelfWeight,
			TotalWeightKg = pallet.GetTotalWeight(),
			TypeSpecification = pallet.PalletTypeSpec != null ? _mapper.Map<PalletTypeSpecDto>(pallet.PalletTypeSpec) : null
		};

		return dto;
	}
}
