using Harbour.Domain;
using Harbour.Domain;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Harbour.Infrastructure.Data;

/// <summary>
/// DbContext para la persistencia de entidades de logística usando Table-Per-Hierarchy (TPH)
/// </summary>
public class HarbourDbContext : DbContext
{
    public HarbourDbContext(DbContextOptions<HarbourDbContext> options) : base(options)
    {
    }

    /// <summary>
    /// DbSet para elementos almacenables (base para TPH)
    /// </summary>
    public DbSet<StorageItem> StorageItems { get; set; } = null!;

    /// <summary>
    /// DbSet para barcos
    /// </summary>
    public DbSet<Ship> Ships { get; set; } = null!;

    /// <summary>
    /// DbSet para tipos de estados de almacenamiento
    /// </summary>
    public DbSet<StorageStatusType> StorageStatusTypes { get; set; } = null!;

    /// <summary>
    /// DbSet para tipos de pallet con especificaciones
    /// </summary>
    public DbSet<PalletTypeSpec> PalletTypeSpecs { get; set; } = null!;

    /// <summary>
    /// DbSet para tipos de contenedor con especificaciones
    /// </summary>
    public DbSet<ContainerTypeSpec> ContainerTypeSpecs { get; set; } = null!;

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        // Configurar entidades de referencia (lookup tables)
        ConfigureLookupTables(modelBuilder);

        // Configurar Table-Per-Hierarchy para StorageItem
        ConfigureStorageItemTPH(modelBuilder);

        // Configurar entidad Ship
        ConfigureShip(modelBuilder);

        // Sembrar datos iniciales
        SeedLookupData(modelBuilder);
    }

    /// <summary>
    /// Configura las tablas de referencia (lookup tables)
    /// </summary>
    private static void ConfigureLookupTables(ModelBuilder modelBuilder)
    {
        // Configurar StorageStatusType
        modelBuilder.Entity<StorageStatusType>(entity =>
        {
            entity.ToTable("StorageStatusTypes");
            entity.HasKey(x => x.Id);
            entity.Property(x => x.Name).HasMaxLength(100).IsRequired();
            entity.Property(x => x.Description).HasMaxLength(500);
            entity.HasIndex(x => x.Name).IsUnique();
        });

        // Configurar PalletTypeSpec
        modelBuilder.Entity<PalletTypeSpec>(entity =>
        {
            entity.ToTable("PalletTypeSpecs");
            entity.HasKey(x => x.Id);
            entity.Property(x => x.Name).HasMaxLength(100).IsRequired();
            entity.Property(x => x.Description).HasMaxLength(500);
            entity.Property(x => x.SelfWeight).HasPrecision(10, 2).IsRequired();
            entity.Property(x => x.MaxCapacity).HasPrecision(10, 2).IsRequired();
            entity.Property(x => x.Dimensions).HasMaxLength(100);
            entity.HasIndex(x => x.Name).IsUnique();
        });

        // Configurar ContainerTypeSpec
        modelBuilder.Entity<ContainerTypeSpec>(entity =>
        {
            entity.ToTable("ContainerTypeSpecs");
            entity.HasKey(x => x.Id);
            entity.Property(x => x.Name).HasMaxLength(100).IsRequired();
            entity.Property(x => x.Description).HasMaxLength(500);
            entity.Property(x => x.SelfWeight).HasPrecision(10, 2).IsRequired();
            entity.Property(x => x.MaxCapacity).HasPrecision(10, 2).IsRequired();
            entity.Property(x => x.ExternalDimensions).HasMaxLength(100);
            entity.Property(x => x.InternalDimensions).HasMaxLength(100);
            entity.Property(x => x.VolumeM3).HasPrecision(10, 2);
            entity.HasIndex(x => x.Name).IsUnique();
        });
    }

    /// <summary>
    /// Configura el mapeo Table-Per-Hierarchy para StorageItem y sus derivados
    /// </summary>
    private static void ConfigureStorageItemTPH(ModelBuilder modelBuilder)
    {
        var storageItemConfig = modelBuilder.Entity<StorageItem>();

        // Configurar tabla principal
        storageItemConfig
            .ToTable("StorageItems")
            .HasDiscriminator<string>("StorageItemType")
            .HasValue<Box>("Box")
            .HasValue<Pallet>("Pallet")
            .HasValue<Container>("Container");

        // Configurar propiedades comunes
        storageItemConfig.HasKey(x => x.Id);
        
        storageItemConfig.Property(x => x.Id)
            .HasMaxLength(50)
            .IsRequired();

        storageItemConfig.Property(x => x.SelfWeight)
            .HasPrecision(10, 2)
            .IsRequired();

        storageItemConfig.Property(x => x.StatusId)
            .IsRequired();

        storageItemConfig.Property(x => x.ParentId)
            .HasMaxLength(50)
            .IsRequired(false);

        // Relación con StorageStatusType
        storageItemConfig
            .HasOne(x => x.Status)
            .WithMany()
            .HasForeignKey(x => x.StatusId)
            .OnDelete(DeleteBehavior.Restrict)
            .IsRequired();

        // Relación recursiva: StorageItem -> StorageItem (ParentId)
        storageItemConfig
            .HasOne<StorageItem>()
            .WithMany()
            .HasForeignKey(x => x.ParentId)
			.OnDelete(DeleteBehavior.Restrict)
			.IsRequired(false);

        // Configurar específicamente Box
        modelBuilder.Entity<Box>(entity =>
        {
            entity.Property(x => x.Destination)
                .HasMaxLength(255)
                .IsRequired();
        });

        // Configurar específicamente Pallet
        modelBuilder.Entity<Pallet>(entity =>
        {
            entity.Property(x => x.PalletTypeSpecId)
                .IsRequired();

            //entity.Property(x => x.PalletTypeSpec.MaxCapacity)
            //    .HasPrecision(10, 2)
            //    .IsRequired();

            entity.HasOne(x => x.PalletTypeSpec)
                .WithMany()
                .HasForeignKey(x => x.PalletTypeSpecId)
                .OnDelete(DeleteBehavior.Restrict)
                .IsRequired();
        });

        // Configurar específicamente Container
        modelBuilder.Entity<Container>(entity =>
        {
            entity.Property(x => x.ContainerTypeSpecId)
                .IsRequired();

            //entity.Property(x => x.ContainerTypeSpec.MaxCapacity)
            //    .HasPrecision(10, 2)
            //    .IsRequired();

            entity.HasOne(x => x.ContainerTypeSpec)
                .WithMany()
                .HasForeignKey(x => x.ContainerTypeSpecId)
                .OnDelete(DeleteBehavior.Restrict)
                .IsRequired();
        });
    }

	/// <summary>
	/// Configura el mapeo de la entidad Ship
	/// </summary>
	private static void ConfigureShip(ModelBuilder modelBuilder)
	{
		var shipConfig = modelBuilder.Entity<Ship>();

		shipConfig.ToTable("Ships");
		shipConfig.HasKey(x => x.ShipId);

		shipConfig.Property(x => x.ShipId)
			.HasMaxLength(50)
			.IsRequired();

		shipConfig.Property(x => x.MaxCapacity)
			.HasPrecision(10, 2)
			.IsRequired();

		shipConfig.Property(x => x.MinCapacity)
			.HasPrecision(10, 2)
			.IsRequired();

		shipConfig.Property(x => x.SpotShip)
			.IsRequired();

		// ✅ Relación correcta usando la propiedad pública
		shipConfig
			.HasMany(s => s.Cargo)
			.WithOne()
			.HasForeignKey("ShipId")
			.OnDelete(DeleteBehavior.Cascade);

		// ✅ Indicar que EF use el campo privado
		shipConfig
			.Navigation(s => s.Cargo)
			.UsePropertyAccessMode(PropertyAccessMode.Field);
	}

	/// <summary>
	/// Siembra datos iniciales en las tablas de referencia
	/// </summary>
	private static void SeedLookupData(ModelBuilder modelBuilder)
    {
        // Sembrar StorageStatusTypes
        modelBuilder.Entity<StorageStatusType>().HasData(
            new StorageStatusType { Id = 1, Name = "Received", Description = "Elemento recibido en almacén", IsActive = true },
            new StorageStatusType { Id = 2, Name = "Loaded", Description = "Elemento cargado en vehículo/contenedor", IsActive = true },
            new StorageStatusType { Id = 3, Name = "Shipped", Description = "Elemento enviado/despachado", IsActive = true }
        );

        // Sembrar PalletTypeSpecs
        modelBuilder.Entity<PalletTypeSpec>().HasData(
            new PalletTypeSpec 
            { 
                Id = 1, 
                Name = "American", 
                Description = "Pallet americano estándar",
                SelfWeight = 28m,
                MaxCapacity = 1200m,
                Dimensions = "40x48 inches",
                IsActive = true 
            },
            new PalletTypeSpec 
            { 
                Id = 2, 
                Name = "European", 
                Description = "Pallet europeo estándar",
                SelfWeight = 25m,
                MaxCapacity = 1500m,
                Dimensions = "80x120 cm",
                IsActive = true 
            }
        );

        // Sembrar ContainerTypeSpecs
        modelBuilder.Entity<ContainerTypeSpec>().HasData(
            new ContainerTypeSpec 
            { 
                Id = 1, 
                Name = "DryVan20", 
                Description = "Contenedor seco 20 pies",
                SelfWeight = 2330m,
                MaxCapacity = 21700m,
                ExternalDimensions = "20x8x8.6 feet",
                InternalDimensions = "19.4x7.7x7.9 feet",
                VolumeM3 = 33.2m,
                IsActive = true 
            },
            new ContainerTypeSpec 
            { 
                Id = 2, 
                Name = "DryVan40", 
                Description = "Contenedor seco 40 pies",
                SelfWeight = 3750m,
                MaxCapacity = 26730m,
                ExternalDimensions = "40x8x8.6 feet",
                InternalDimensions = "39.5x7.7x7.9 feet",
                VolumeM3 = 67.7m,
                IsActive = true 
            },
            new ContainerTypeSpec 
            { 
                Id = 3, 
                Name = "HighCube40", 
                Description = "Contenedor de altura alta 40 pies",
                SelfWeight = 3970m,
                MaxCapacity = 26500m,
                ExternalDimensions = "40x8x9.6 feet",
                InternalDimensions = "39.5x7.7x8.9 feet",
                VolumeM3 = 76.3m,
                IsActive = true 
            },
            new ContainerTypeSpec 
            { 
                Id = 4, 
                Name = "HighCube45", 
                Description = "Contenedor de altura alta 45 pies",
                SelfWeight = 4850m,
                MaxCapacity = 28350m,
                ExternalDimensions = "45x8x9.6 feet",
                InternalDimensions = "44.6x7.7x8.9 feet",
                VolumeM3 = 86.0m,
                IsActive = true 
            },
            new ContainerTypeSpec 
            { 
                Id = 5, 
                Name = "Reefer20", 
                Description = "Contenedor refrigerado 20 pies",
                SelfWeight = 2900m,
                MaxCapacity = 20800m,
                ExternalDimensions = "20x8x8.6 feet",
                InternalDimensions = "18.3x7.2x7.6 feet",
                VolumeM3 = 28.3m,
                IsActive = true 
            },
            new ContainerTypeSpec 
            { 
                Id = 6, 
                Name = "Reefer40", 
                Description = "Contenedor refrigerado 40 pies",
                SelfWeight = 4650m,
                MaxCapacity = 20350m,
                ExternalDimensions = "40x8x8.6 feet",
                InternalDimensions = "37.6x7.2x7.6 feet",
                VolumeM3 = 58.0m,
                IsActive = true 
            }
        );
    }
}

