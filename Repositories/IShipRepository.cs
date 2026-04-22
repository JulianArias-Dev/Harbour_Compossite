namespace Harbour.Repositories
{
	public interface IShipRepository
	{
		Task<Ship?> GetByIdAsync(string id);
		Task<IEnumerable<Ship>> GetAllAsync();

		Task<Ship> AddAsync(Ship ship);
		Task<Ship> UpdateAsync(Ship ship);
		Task DeleteAsync(string id);

		Task LoadCargoAsync(string shipId, string itemId);
		Task UnloadCargoAsync(string shipId, string itemId);

		Task SailAsync(string shipId);
		Task AnchorAsync(string shipId);
	}

	public class ShipRepository : IShipRepository
	{
		protected readonly Infrastructure.Data.HarbourDbContext _context;
		public ShipRepository(Infrastructure.Data.HarbourDbContext context)
		{
			_context = context ?? throw new ArgumentNullException(nameof(context));
		}
		public async Task AnchorAsync(string shipId)
		{
			var ship = await GetByIdAsync(shipId)
				?? throw new KeyNotFoundException($"Ship con ID '{shipId}' no encontrado");

			ship.Anchor(); // lógica de dominio
			await _context.SaveChangesAsync();
		}

		public async Task SailAsync(string shipId)
		{
			var ship = await GetByIdAsync(shipId)
				?? throw new KeyNotFoundException($"Ship con ID '{shipId}' no encontrado");

			ship.Sail();
			await _context.SaveChangesAsync();
		}

		public async Task LoadCargoAsync(string shipId, string itemId)
		{
			var ship = await GetByIdAsync(shipId)
				?? throw new KeyNotFoundException($"Ship con ID '{shipId}' no encontrado");

			var item = await _context.StorageItems.FindAsync(itemId)
				?? throw new KeyNotFoundException($"Item '{itemId}' no encontrado");

			ship.LoadCargo(item);
			await _context.SaveChangesAsync();
		}

		public async Task UnloadCargoAsync(string shipId, string itemId)
		{
			var ship = await GetByIdAsync(shipId)
				?? throw new KeyNotFoundException($"Ship con ID '{shipId}' no encontrado");

			ship.UnloadCargo(itemId);
			await _context.SaveChangesAsync();
		}
		public async Task<Ship?> GetByIdAsync(string id)
		{
			return await _context.Ships
				.Include(s => s.Cargo)
				.FirstOrDefaultAsync(s => s.ShipId == id);
		}
		public async Task<IEnumerable<Ship>> GetAllAsync()
		{
			return await _context.Ships
				.Include(s => s.Cargo)
				.ToListAsync();
		}
		public async Task<Ship> AddAsync(Ship ship)
		{
			var entry = await _context.Ships.AddAsync(ship);
			await _context.SaveChangesAsync();
			return entry.Entity;
		}
		public async Task<Ship> UpdateAsync(Ship ship)
		{
			var entry = _context.Ships.Update(ship);
			await _context.SaveChangesAsync();
			return entry.Entity;
		}
		public async Task DeleteAsync(string id)
		{
			var ship = await GetByIdAsync(id);
			if (ship != null)
			{
				_context.Ships.Remove(ship);
				await _context.SaveChangesAsync();
			}
			else
			{
				throw new KeyNotFoundException($"Ship con ID '{id}' no encontrado");
			}
		}
		// Implementación de LoadCargoAsync, UnloadCargoAsync, SailAsync y AnchorAsync se omite por simplicidad
	}


}
