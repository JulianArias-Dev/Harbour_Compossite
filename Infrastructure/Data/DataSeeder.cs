using Harbour.Domain;
using Harbour.Domain;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.EntityFrameworkCore;

namespace Harbour.Infrastructure.Data;

/// <summary>
/// Servicio hosted que genera datos de prueba al iniciar la aplicación
/// Crea 300+ objetos distribuidos entre Box, Pallet, Container y dos barcos
/// </summary>
public class DataSeeder : IHostedService
{
    private readonly IServiceProvider _serviceProvider;
    private readonly Random _random = new();

    public DataSeeder(IServiceProvider serviceProvider)
    {
        _serviceProvider = serviceProvider;
    }

    public async Task StartAsync(CancellationToken cancellationToken)
    {
        using (var scope = _serviceProvider.CreateScope())
        {
            var dbContext = scope.ServiceProvider.GetRequiredService<HarbourDbContext>();

            // Verificar si ya hay datos sembrados
            if (dbContext.StorageItems.Any() || dbContext.Ships.Any())
            {
                return;
            }

            await SeedDataAsync(dbContext, cancellationToken);
        }
    }

    public Task StopAsync(CancellationToken cancellationToken)
    {
        return Task.CompletedTask;
    }

    private async Task SeedDataAsync(HarbourDbContext dbContext, CancellationToken cancellationToken)
    {
        // Obtener especificaciones de pallets y contenedores
        var palletSpecs = await dbContext.PalletTypeSpecs.ToListAsync(cancellationToken);
        var containerSpecs = await dbContext.ContainerTypeSpecs.ToListAsync(cancellationToken);

        if (!palletSpecs.Any() || !containerSpecs.Any())
        {
            return;
        }

        var destinations = new[] 
        { 
            "Miami", "Houston", "Long Beach", "New York", "Singapore", 
            "Rotterdam", "Hamburg", "Shanghai", "Dubai", "Tokyo",
            "Sydney", "Los Angeles", "Busan", "Hong Kong", "Mumbai"
        };

        var boxWeights = new[] { 5m, 10m, 15m, 20m, 25m, 30m, 35m, 40m, 45m, 50m };

        // Crear aproximadamente 300 cajas
        var boxes = new List<Box>();
        for (int i = 0; i < 200; i++)
        {
            var weight = boxWeights[_random.Next(boxWeights.Length)];
            var destination = destinations[_random.Next(destinations.Length)];
            boxes.Add(new Box(weight, destination));
        }

        // Crear pallets y llenarlos con cajas
        var pallets = new List<Pallet>();
        var boxIndex = 0;

        while (boxIndex < boxes.Count)
        {
            var palletSpec = palletSpecs[_random.Next(palletSpecs.Count)];
            var pallet = new Pallet(palletSpec);
            
            // Llenar el pallet con cajas respetando capacidad
            while (boxIndex < boxes.Count)
            {
                var box = boxes[boxIndex];
                try
                {
                    pallet.AddItem(box);
                    boxIndex++;
                }
                catch (InvalidOperationException)
                {
                    // Pallet lleno, crear uno nuevo
                    break;
                }
            }

            pallets.Add(pallet);
        }

        // Crear contenedores y llenarlos con pallets
        var containers = new List<Container>();
        var palletIndex = 0;

        while (palletIndex < pallets.Count)
        {
            var containerSpec = containerSpecs[_random.Next(containerSpecs.Count)];
            var container = new Container(containerSpec);
            
            // Llenar el contenedor con pallets respetando capacidad
            while (palletIndex < pallets.Count)
            {
                var pallet = pallets[palletIndex];
                try
                {
                    container.AddItem(pallet);
                    palletIndex++;
                }
                catch (InvalidOperationException)
                {
                    // Contenedor lleno, crear uno nuevo
                    break;
                }
            }

            containers.Add(container);
        }

        // Agregar todos los elementos a la base de datos
        dbContext.StorageItems.AddRange(boxes.Cast<StorageItem>());
        dbContext.StorageItems.AddRange(pallets.Cast<StorageItem>());
        dbContext.StorageItems.AddRange(containers.Cast<StorageItem>());

        // Crear dos barcos con requisitos específicos
        var ship1 = new Ship(maxCapacity: 50000m, minCapacity: 30000m);
        var ship2 = new Ship(maxCapacity: 50000m, minCapacity: 30000m);

        // Barco 1: Llenar hasta que supere MinCapacity
        var ship1Weight = 0m;
        var containerIndexForShip1 = 0;

        while (containerIndexForShip1 < containers.Count && ship1Weight < ship1.MinCapacity)
        {
            try
            {
                var container = containers[containerIndexForShip1];
                ship1.LoadCargo(container);
                ship1Weight = ship1.GetTotalCargoWeight();
                containerIndexForShip1++;
            }
            catch (InvalidOperationException)
            {
                // No cabe más, pasar al siguiente
                containerIndexForShip1++;
            }
        }

        // Barco 2: Llenar parcialmente (por debajo de MinCapacity)
        var ship2Weight = 0m;
        var containerIndexForShip2 = containerIndexForShip1;

        while (containerIndexForShip2 < containers.Count && ship2Weight + containers[containerIndexForShip2].GetTotalWeight() < ship2.MinCapacity * 0.8m)
        {
            try
            {
                var container = containers[containerIndexForShip2];
                ship2.LoadCargo(container);
                ship2Weight = ship2.GetTotalCargoWeight();
                containerIndexForShip2++;
            }
            catch (InvalidOperationException)
            {
                // No cabe más
                containerIndexForShip2++;
            }
        }

        // Agregar barcos a la base de datos
        dbContext.Ships.Add(ship1);
        dbContext.Ships.Add(ship2);

        // Guardar todos los cambios
        await dbContext.SaveChangesAsync(cancellationToken);
    }
}
