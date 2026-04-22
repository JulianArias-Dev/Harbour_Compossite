# Harbour Logistics API - .NET 9

API de gestión de logística basada en el patrón **Composite** con arquitectura de tres capas (Controllers > Services > Repositories).

## ??? Arquitectura

### Capa de Dominio (Domain)
- **StorageItem**: Clase base abstracta para todos los elementos almacenables
  - **Box**: Elemento hoja (caja individual)
  - **CompositeStorage**: Clase base para contenedores
    - **Pallet**: Contenedor estándar (American/European)
    - **Container**: Contenedor marítimo (6 tipos ISO)
- **Ship**: Cliente del composite que gestiona carga
- Enumeraciones: `StorageStatus`, `PalletType`, `ContainerType`
- Interfaz: `IStorage`

### Capa de Aplicación (Application)
- **DTOs**: `CreateBoxDto`, `CreatePalletDto`, `CreateContainerDto`, `LoadItemDto`, `CreateShipDto`, etc.
- **Services**: `ILogisticsService` e implementación `LogisticsService`
  - Métodos: `CreateBoxAsync()`, `CreatePalletAsync()`, `CreateContainerAsync()`, `LoadItemToContainerAsync()`, etc.

### Capa de Datos (Infrastructure)
- **DbContext**: `HarbourDbContext` con **Table-Per-Hierarchy (TPH)**
  - Configuración con Fluent API
  - Relación recursiva: `ParentId` para jerarquías
- **Repository**: `IStorageRepository` y `StorageRepository`
  - Métodos genéricos: `GetByIdAsync<T>()`, `GetByTypeAsync<T>()`, etc.

### Capa de Presentación (Controllers)
- **LogisticsController**: Expone endpoints REST
  - POST: Crear elementos
  - PUT: Cargar elementos en contenedores
  - GET: Obtener información

## ?? Requisitos

- .NET 9.0 SDK
- SQL Server (local o remoto)
- Visual Studio 2022 o VS Code

## ?? NuGet Packages

```xml
<PackageReference Include="Microsoft.EntityFrameworkCore" Version="9.0.0" />
<PackageReference Include="Microsoft.EntityFrameworkCore.SqlServer" Version="9.0.0" />
<PackageReference Include="Microsoft.EntityFrameworkCore.Tools" Version="9.0.0" />
<PackageReference Include="Swashbuckle.AspNetCore" Version="6.4.10" />
```

## ?? Configuración

### 1. Ajustar Conexión de Base de Datos

Editar `appsettings.json`:
```json
{
  "ConnectionStrings": {
    "DefaultConnection": "Server=.;Database=HarbourDb;Trusted_Connection=True;Encrypt=False;"
  }
}
```

### 2. Crear Base de Datos

```bash
dotnet ef database update
```

o en Package Manager Console:
```powershell
Update-Database
```

## ?? Uso de API

### Ejemplo 1: Crear una Caja
```bash
curl -X POST "https://localhost:7xxx/api/logistics/boxes" \
  -H "Content-Type: application/json" \
  -d '{
    "id": "BOX-001",
    "selfWeight": 2.5,
    "destination": "Madrid"
  }'
```

### Ejemplo 2: Crear Pallet y Cargar Caja
```bash
# 1. Crear pallet europeo
curl -X POST "https://localhost:7xxx/api/logistics/pallets" \
  -H "Content-Type: application/json" \
  -d '{"id": "PALLET-001", "type": 1}'

# 2. Cargar caja en pallet
curl -X PUT "https://localhost:7xxx/api/logistics/load-item" \
  -H "Content-Type: application/json" \
  -d '{
    "childItemId": "BOX-001",
    "parentItemId": "PALLET-001"
  }'

# 3. Obtener peso total
curl -X GET "https://localhost:7xxx/api/logistics/weight/PALLET-001"
```

## ??? Estructura de Carpetas

```
Harbour/
??? Domain/
?   ??? IStorage.cs
?   ??? StorageItem.cs
?   ??? CompositeStorage.cs
?   ??? Box.cs
?   ??? Pallet.cs
?   ??? Container.cs
?   ??? Ship.cs
?   ??? StorageEnums.cs
??? Application/
?   ??? DTOs/
?   ?   ??? LogisticsDto.cs
?   ??? Services/
?       ??? ILogisticsService.cs
?       ??? LogisticsService.cs
??? Infrastructure/
?   ??? Data/
?   ?   ??? HarbourDbContext.cs
?   ?   ??? Migrations/
?   ?       ??? 20240115000000_InitialMigration.cs
?   ?       ??? HarbourDbContextModelSnapshot.cs
?   ??? Repositories/
?       ??? IStorageRepository.cs
??? Controllers/
?   ??? LogisticsController.cs
??? Program.cs
??? GlobalUsings.cs
??? appsettings.json
??? Harbour.csproj
```

## ? Reglas de Negocio Implementadas

1. **Validación de Peso**: No se puede exceder `MaxCapacity` al cargar items
2. **Relación Recursiva**: Un Pallet puede estar dentro de un Container
3. **Especificaciones Estándar**:
   - **American Pallet**: Tara = 28kg, Max = 1200kg
   - **European Pallet**: Tara = 25kg, Max = 1500kg
   - **Contenedores ISO**: Especificaciones reales según tipo
4. **Estados de Ciclo de Vida**: Received ? Loaded ? Shipped
5. **Ship Viability**: Validar que carga cumpla mínimo y máximo

## ?? Ejemplo de Jerarquía

```
Ship (100,000kg max)
??? Container-20ft (2,330kg tara, 21,700kg max)
?   ??? Pallet-EU (25kg tara, 1,500kg max)
?   ?   ??? Box-1 (2.5kg)
?   ?   ??? Box-2 (3.0kg)
?   ??? Pallet-EU (25kg tara, 1,500kg max)
?       ??? Box-3 (5.0kg)
??? Container-40ft (3,750kg tara, 26,730kg max)
    ??? Pallet-US (28kg tara, 1,200kg max)
    ?   ??? Box-4 (4.0kg)
    ?   ??? Box-5 (6.0kg)
    ??? Pallet-US (28kg tara, 1,200kg max)
        ??? Box-6 (3.5kg)
```

## ?? Endpoints Principales

| Método | Endpoint | Descripción |
|--------|----------|-------------|
| POST | `/api/logistics/boxes` | Crear caja |
| POST | `/api/logistics/pallets` | Crear pallet |
| POST | `/api/logistics/containers` | Crear contenedor |
| PUT | `/api/logistics/load-item` | Cargar item en contenedor |
| GET | `/api/logistics/{id}` | Obtener elemento |
| GET | `/api/logistics/weight/{id}` | Obtener peso total |
| POST | `/api/logistics/ships` | Crear barco |
| GET | `/api/logistics/ships/{id}` | Obtener barco |

## ?? Documentación Interactiva

Swagger está habilitado en desarrollo:
```
https://localhost:7xxx/swagger/index.html
```

## ?? Pruebas

Ejecutar aplicación:
```bash
dotnet run
```

Swagger se abrirá automáticamente en desarrollo en el puerto HTTPS (generalmente 7xxx).

## ??? Generación de Migraciones

Crear nueva migración:
```bash
dotnet ef migrations add NombreMigracion --project Harbour.csproj
```

Actualizar base de datos:
```bash
dotnet ef database update
```

Revertir última migración:
```bash
dotnet ef database update NombreMigracionAnterior
```

## ?? Ver Documentación de API

Consultar `API_DOCUMENTATION.md` para detalles completos de endpoints, ejemplos de request/response y flujos de negocio.

## ?? Patrones Utilizados

- **Composite Pattern**: Estructura recursiva de contenedores
- **Repository Pattern**: Abstracción de acceso a datos
- **Dependency Injection**: IoC Container nativo de ASP.NET Core
- **DTOs**: Desacoplamiento entre capas
- **Domain-Driven Design**: Lógica de negocio en entidades de dominio
- **Table-Per-Hierarchy**: Persistencia polimórfica en EF Core

## ?? Notas de Implementación

- **CompositeStorage** usa una lista interna `_contents` que se maneja en memoria
- **EF Core** persiste la relación mediante el discriminador y `ParentId`
- **Ship** no incluye persistencia de cargo (versión simplificada)
- Se pueden extender fácilmente las reglas de negocio en los métodos de dominio

---

**Autor**: Arquitectura implementada siguiendo Clean Architecture y Domain-Driven Design  
**Versión**: 1.0  
**Framework**: .NET 9  
**Base de Datos**: SQL Server
