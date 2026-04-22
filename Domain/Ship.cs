namespace Harbour.Domain;

/// Estados posibles de un barco en el ciclo de vida de la simulación
public enum ShipStatus
{
	/// El barco está disponible para recibir carga
	Available = 0,

	/// El barco está en tránsito (ha zarpado)
	Sailing = 1
}

/// Representa un barco/navío (cliente del patrón Composite)
/// Gestiona la carga total incluyendo restricciones de capacidad mínima y máxima.
/// Implementa reglas de negocio: zarpe solo si alcanza capacidad mínima, anclaje para disponibilidad.
public class Ship
{
	private readonly List<StorageItem> _cargo = new();

	/// Identificador único del barco
	public string ShipId { get; private set; }

	/// Indica si es un viaje especial/charter
	public bool SpotShip { get; set; }

	/// Capacidad máxima permitida en kilogramos
	public decimal MaxCapacity { get; private set; }

	/// Capacidad mínima requerida en kilogramos para viabilidad económica
	public decimal MinCapacity { get; private set; }

	/// Estado del barco: disponible (0) o en tránsito (1)
	public ShipStatus Status { get; private set; }

	/// Colección de elementos de carga (solo lectura desde el exterior)
	public IReadOnlyCollection<StorageItem> Cargo => _cargo.AsReadOnly();

	/// Constructor para crear un nuevo barco
	public Ship(
		decimal maxCapacity,
		decimal minCapacity,
		bool spotShip = false)
	{
		if (maxCapacity <= 0)
			throw new ArgumentException("La capacidad máxima debe ser mayor que cero", nameof(maxCapacity));

		if (minCapacity < 0)
			throw new ArgumentException("La capacidad mínima no puede ser negativa", nameof(minCapacity));

		if (minCapacity > maxCapacity)
			throw new ArgumentException(
				"La capacidad mínima no puede superar la capacidad máxima",
				nameof(minCapacity));

		ShipId = Guid.NewGuid().ToString();
		MaxCapacity = maxCapacity;
		MinCapacity = minCapacity;
		SpotShip = spotShip;
		Status = ShipStatus.Available;
	}

	/// Calcula el peso total de la carga
	public decimal GetTotalCargoWeight() => _cargo.Sum(item => item.GetTotalWeight());

	/// Obtiene el número de ítems cargados (conteo plano, no recursivo)
	public int GetCargoCount() => _cargo.Count;

	/// Añade un elemento de carga al barco con validación de capacidad máxima
	public void LoadCargo(StorageItem item)
	{
		if (item == null)
			throw new ArgumentNullException(nameof(item), "El elemento de carga no puede ser null");

		decimal currentWeight = GetTotalCargoWeight();
		decimal itemWeight = item.GetTotalWeight();
		decimal projectedWeight = currentWeight + itemWeight;

		// Regla 2: Validar capacidad máxima del barco
		if (projectedWeight > MaxCapacity)
		{
			throw new InvalidOperationException(
				$"No se puede cargar el elemento. Carga actual: {currentWeight}kg, " +
				$"Peso del elemento: {itemWeight}kg, Total proyectado: {projectedWeight}kg, " +
				$"Capacidad máxima: {MaxCapacity}kg");
		}

		_cargo.Add(item);
	}

	/// Elimina un elemento de carga del barco
	public bool UnloadCargo(string itemId)
	{
		var item = _cargo.FirstOrDefault(i => i.Id == itemId);
		if (item != null)
		{
			return _cargo.Remove(item);
		}
		return false;
	}

	/// Zarpa el barco (transición a estado "En Tránsito")
	/// Regla 3: El barco NO puede zarpar si la carga actual es menor que la capacidad mínima.
	public void Sail()
	{
		if (Status == ShipStatus.Sailing)
			throw new InvalidOperationException("El barco ya está en tránsito");

		decimal currentWeight = GetTotalCargoWeight();

		if (currentWeight < MinCapacity)
		{
			throw new InvalidOperationException(
				$"El barco no puede zarpar. Carga actual: {currentWeight}kg, " +
				$"Carga mínima requerida: {MinCapacity}kg");
		}

		Status = ShipStatus.Sailing;
	}

	/// Ancla el barco (transición a estado "Disponible")
	/// Permite volver a recibir carga o realizar otras operaciones.
	public void Anchor()
	{
		if (Status == ShipStatus.Available)
			throw new InvalidOperationException("El barco ya está anclado/disponible");

		Status = ShipStatus.Available;
	}

	/// Obtiene la capacidad disponible (espacios restantes)
	public decimal AvailableCapacity => MaxCapacity - GetTotalCargoWeight();

	/// Verifica si la carga cumple con los requisitos mínimos
	public bool MeetsMinimumCapacity() => GetTotalCargoWeight() >= MinCapacity;

	/// Verifica si la carga cumple con los requisitos máximos
	public bool WithinMaxCapacity() => GetTotalCargoWeight() <= MaxCapacity;

	/// Verifica si la carga es viable (cumple tanto mínimo como máximo)
	public bool IsViable() => MeetsMinimumCapacity() && WithinMaxCapacity();

	/// Obtiene el porcentaje de utilización de capacidad
	public decimal GetCapacityUtilization()
	{
		if (MaxCapacity <= 0)
			return 0;

		return (GetTotalCargoWeight() / MaxCapacity) * 100m;
	}

	/// Representación en string del barco
	public override string ToString()
	{
		var totalWeight = GetTotalCargoWeight();
		var utilization = GetCapacityUtilization();

		return $"Ship(Id={ShipId}, Cargo={_cargo.Count} items, Weight={totalWeight}kg/" +
			   $"{MaxCapacity}kg ({utilization:F1}%), Status={Status}, Viable={IsViable()}, SpotShip={SpotShip})";
	}
}
