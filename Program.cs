using Harbour.Application.Services;
using Harbour.Application.Mapping;
using Harbour.Infrastructure.Data;
using Harbour.Repositories;
using Microsoft.EntityFrameworkCore;
using AutoMapper;

var builder = WebApplication.CreateBuilder(args);

// Configurar la cadena de conexión
var connectionString = builder.Configuration.GetConnectionString("DefaultConnection")
    ?? "Server=localhost,1433;Database=HarbourDb;User Id=sa;Password=Unicesar;Encrypt=False;";

// Registrar DbContext
builder.Services.AddDbContext<HarbourDbContext>(options =>
    options.UseSqlServer(connectionString,
        sqlServerOptions => sqlServerOptions.MigrationsAssembly("Harbour")));

// Registrar repositorios segregados por ISP
builder.Services.AddScoped<IStorageRepository, StorageRepository>();
builder.Services.AddScoped(typeof(ICompositeStorageRepository<>), typeof(CompositeStorageRepository<>));
builder.Services.AddScoped<IShipRepository, ShipRepository>();

// Registrar servicios de aplicación
builder.Services.AddScoped<ILogisticsService, LogisticsService>();
builder.Services.AddScoped<IBoxApplicationService, BoxApplicationService>();
builder.Services.AddScoped<IPalletApplicationService, PalletApplicationService>();
builder.Services.AddScoped<IContainerApplicationService, ContainerApplicationService>();
builder.Services.AddScoped<IShipApplicationService, ShipApplicationService>();

// Registrar AutoMapper
builder.Services.AddAutoMapper(typeof(MappingProfile));

// Registrar DataSeeder como IHostedService
builder.Services.AddHostedService<DataSeeder>();

// Agregar controladores
builder.Services.AddControllers();

// Configurar OpenAPI
builder.Services.AddOpenApi();

// Configurar Swagger
builder.Services.AddSwaggerGen(options =>
{
    options.SwaggerDoc("v1", new Microsoft.OpenApi.Models.OpenApiInfo
    {
        Title = "Harbour Logistics API",
        Version = "v1",
        Description = "API para gestión de logística y almacenamiento con patrón Composite y segregación de interfaces (ISP)"
    });
});

var app = builder.Build();

// Configurar el pipeline HTTP
if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
    
    // Habilitar Swagger en desarrollo
    app.UseSwagger();
    app.UseSwaggerUI(options =>
    {
        options.SwaggerEndpoint("/swagger/v1/swagger.json", "Harbour Logistics API v1");
    });

    // Aplicar migraciones automáticamente en desarrollo
    using (var scope = app.Services.CreateScope())
    {
        var dbContext = scope.ServiceProvider.GetRequiredService<HarbourDbContext>();
        dbContext.Database.Migrate();
    }
}

app.UseHttpsRedirection();

app.UseAuthorization();

app.MapControllers();

app.MapGet("/", () => Results.Redirect("/swagger"));

app.Run();