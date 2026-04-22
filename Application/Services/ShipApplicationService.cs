namespace Harbour.Application.Services;

/// <summary>
/// Servicio de aplicación para operaciones de Ship
/// Implementa lógica de negocio para CRUD, carga/descarga, zarpe y anclaje
/// </summary>
public interface IShipApplicationService
{
	Task<ShipDetailDto> CreateAsync(CreateShipDto request);
	Task<ShipDetailDto> GetByIdAsync(string id);
	Task<IEnumerable<ShipDetailDto>> GetAllAsync();
	Task<ShipDetailDto> UpdateAsync(string id, UpdateShipDto request);
	Task DeleteAsync(string id);
	Task<ShipDetailDto> LoadCargoAsync(string shipId, string itemId);
	Task<ShipDetailDto> UnloadCargoAsync(string shipId, string itemId);
	Task<ShipDetailDto> SailAsync(string shipId);
	Task<ShipDetailDto> AnchorAsync(string shipId);
}

public class ShipApplicationService : IShipApplicationService
{
	private readonly IShipRepository _shipRepository;
	private readonly IMapper _mapper;

	public ShipApplicationService(
		IShipRepository shipRepository,
		IMapper mapper)
	{
		_shipRepository = shipRepository ?? throw new ArgumentNullException(nameof(shipRepository));
		_mapper = mapper ?? throw new ArgumentNullException(nameof(mapper));
	}

	public async Task<ShipDetailDto> CreateAsync(CreateShipDto request)
	{
		var ship = new Ship(request.MaxCapacity, request.MinCapacity, request.SpotShip);
		var created = await _shipRepository.AddAsync(ship);
		return MapToDetailDto(created);
	}

	public async Task<ShipDetailDto> GetByIdAsync(string id)
	{
		var ship = await _shipRepository.GetByIdAsync(id)
			?? throw new KeyNotFoundException($"Ship con ID '{id}' no encontrado");

		return MapToDetailDto(ship);
	}

	public async Task<IEnumerable<ShipDetailDto>> GetAllAsync()
	{
		var ships = await _shipRepository.GetAllAsync();
		return ships.Select(MapToDetailDto);
	}

	public async Task<ShipDetailDto> UpdateAsync(string id, UpdateShipDto request)
	{
		var ship = await _shipRepository.GetByIdAsync(id)
			?? throw new KeyNotFoundException($"Ship con ID '{id}' no encontrado");

		if (request.SpotShip.HasValue)
		{
			ship.SpotShip = request.SpotShip.Value;
		}

		var updated = await _shipRepository.UpdateAsync(ship);
		return MapToDetailDto(updated);
	}

	public async Task DeleteAsync(string id)
	{
		await _shipRepository.DeleteAsync(id);
	}

	public async Task<ShipDetailDto> LoadCargoAsync(string shipId, string itemId)
	{
		await _shipRepository.LoadCargoAsync(shipId, itemId);
		var updated = await _shipRepository.GetByIdAsync(shipId);
		return MapToDetailDto(updated!);
	}

	public async Task<ShipDetailDto> UnloadCargoAsync(string shipId, string itemId)
	{
		await _shipRepository.UnloadCargoAsync(shipId, itemId);
		var updated = await _shipRepository.GetByIdAsync(shipId);
		return MapToDetailDto(updated!);
	}

	public async Task<ShipDetailDto> SailAsync(string shipId)
	{
		await _shipRepository.SailAsync(shipId);
		var updated = await _shipRepository.GetByIdAsync(shipId);
		return MapToDetailDto(updated!);
	}

	public async Task<ShipDetailDto> AnchorAsync(string shipId)
	{
		await _shipRepository.AnchorAsync(shipId);
		var updated = await _shipRepository.GetByIdAsync(shipId);
		return MapToDetailDto(updated!);
	}

	private ShipDetailDto MapToDetailDto(Ship ship)
	{
		var totalWeight = ship.GetTotalCargoWeight();
		var dto = new ShipDetailDto
		{
			ShipId = ship.ShipId,
			SpotShip = ship.SpotShip,
			MaxCapacity = ship.MaxCapacity,
			MinCapacity = ship.MinCapacity,
			Status = ship.Status.ToString(),
			CargoItemCount = ship.GetCargoCount(),
			TotalCargoWeightKg = totalWeight,
			AvailableCapacityKg = ship.AvailableCapacity,
			CapacityUtilization = ship.GetCapacityUtilization(),
			CanSail = totalWeight >= ship.MinCapacity
		};

		return dto;
	}
}
