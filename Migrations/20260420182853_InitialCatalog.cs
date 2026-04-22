using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

#pragma warning disable CA1814 // Prefer jagged arrays over multidimensional

namespace Harbour.Migrations
{
    /// <inheritdoc />
    public partial class InitialCatalog : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "ContainerTypeSpecs",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Name = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    Description = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: false),
                    SelfWeight = table.Column<decimal>(type: "decimal(10,2)", precision: 10, scale: 2, nullable: false),
                    MaxCapacity = table.Column<decimal>(type: "decimal(10,2)", precision: 10, scale: 2, nullable: false),
                    ExternalDimensions = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                    InternalDimensions = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                    VolumeM3 = table.Column<decimal>(type: "decimal(10,2)", precision: 10, scale: 2, nullable: true),
                    IsActive = table.Column<bool>(type: "bit", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ContainerTypeSpecs", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "PalletTypeSpecs",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Name = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    Description = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: false),
                    SelfWeight = table.Column<decimal>(type: "decimal(10,2)", precision: 10, scale: 2, nullable: false),
                    MaxCapacity = table.Column<decimal>(type: "decimal(10,2)", precision: 10, scale: 2, nullable: false),
                    Dimensions = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                    IsActive = table.Column<bool>(type: "bit", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_PalletTypeSpecs", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "Ships",
                columns: table => new
                {
                    ShipId = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    SpotShip = table.Column<bool>(type: "bit", nullable: false),
                    MaxCapacity = table.Column<decimal>(type: "decimal(10,2)", precision: 10, scale: 2, nullable: false),
                    MinCapacity = table.Column<decimal>(type: "decimal(10,2)", precision: 10, scale: 2, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Ships", x => x.ShipId);
                });

            migrationBuilder.CreateTable(
                name: "StorageStatusTypes",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Name = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    Description = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: false),
                    IsActive = table.Column<bool>(type: "bit", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_StorageStatusTypes", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "StorageItems",
                columns: table => new
                {
                    Id = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    SelfWeight = table.Column<decimal>(type: "decimal(10,2)", precision: 10, scale: 2, nullable: false),
                    ParentId = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: true),
                    StatusId = table.Column<int>(type: "int", nullable: false),
                    ContainerId = table.Column<string>(type: "nvarchar(50)", nullable: true),
                    PalletId = table.Column<string>(type: "nvarchar(50)", nullable: true),
                    StorageItemType = table.Column<string>(type: "nvarchar(13)", maxLength: 13, nullable: false),
                    Destination = table.Column<string>(type: "nvarchar(255)", maxLength: 255, nullable: true),
                    ContainerTypeSpecId = table.Column<int>(type: "int", nullable: true),
                    MaxCapacity = table.Column<decimal>(type: "decimal(10,2)", precision: 10, scale: 2, nullable: true),
                    PalletTypeSpecId = table.Column<int>(type: "int", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_StorageItems", x => x.Id);
                    table.ForeignKey(
                        name: "FK_StorageItems_ContainerTypeSpecs_ContainerTypeSpecId",
                        column: x => x.ContainerTypeSpecId,
                        principalTable: "ContainerTypeSpecs",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_StorageItems_PalletTypeSpecs_PalletTypeSpecId",
                        column: x => x.PalletTypeSpecId,
                        principalTable: "PalletTypeSpecs",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_StorageItems_StorageItems_ContainerId",
                        column: x => x.ContainerId,
                        principalTable: "StorageItems",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_StorageItems_StorageItems_PalletId",
                        column: x => x.PalletId,
                        principalTable: "StorageItems",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_StorageItems_StorageItems_ParentId",
                        column: x => x.ParentId,
                        principalTable: "StorageItems",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_StorageItems_StorageStatusTypes_StatusId",
                        column: x => x.StatusId,
                        principalTable: "StorageStatusTypes",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.InsertData(
                table: "ContainerTypeSpecs",
                columns: new[] { "Id", "Description", "ExternalDimensions", "InternalDimensions", "IsActive", "MaxCapacity", "Name", "SelfWeight", "VolumeM3" },
                values: new object[,]
                {
                    { 1, "Contenedor seco 20 pies", "20x8x8.6 feet", "19.4x7.7x7.9 feet", true, 21700m, "DryVan20", 2330m, 33.2m },
                    { 2, "Contenedor seco 40 pies", "40x8x8.6 feet", "39.5x7.7x7.9 feet", true, 26730m, "DryVan40", 3750m, 67.7m },
                    { 3, "Contenedor de altura alta 40 pies", "40x8x9.6 feet", "39.5x7.7x8.9 feet", true, 26500m, "HighCube40", 3970m, 76.3m },
                    { 4, "Contenedor de altura alta 45 pies", "45x8x9.6 feet", "44.6x7.7x8.9 feet", true, 28350m, "HighCube45", 4850m, 86.0m },
                    { 5, "Contenedor refrigerado 20 pies", "20x8x8.6 feet", "18.3x7.2x7.6 feet", true, 20800m, "Reefer20", 2900m, 28.3m },
                    { 6, "Contenedor refrigerado 40 pies", "40x8x8.6 feet", "37.6x7.2x7.6 feet", true, 20350m, "Reefer40", 4650m, 58.0m }
                });

            migrationBuilder.InsertData(
                table: "PalletTypeSpecs",
                columns: new[] { "Id", "Description", "Dimensions", "IsActive", "MaxCapacity", "Name", "SelfWeight" },
                values: new object[,]
                {
                    { 1, "Pallet americano estándar", "40x48 inches", true, 1200m, "American", 28m },
                    { 2, "Pallet europeo estándar", "80x120 cm", true, 1500m, "European", 25m }
                });

            migrationBuilder.InsertData(
                table: "StorageStatusTypes",
                columns: new[] { "Id", "Description", "IsActive", "Name" },
                values: new object[,]
                {
                    { 1, "Elemento recibido en almacén", true, "Received" },
                    { 2, "Elemento cargado en vehículo/contenedor", true, "Loaded" },
                    { 3, "Elemento enviado/despachado", true, "Shipped" }
                });

            migrationBuilder.CreateIndex(
                name: "IX_ContainerTypeSpecs_Name",
                table: "ContainerTypeSpecs",
                column: "Name",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_PalletTypeSpecs_Name",
                table: "PalletTypeSpecs",
                column: "Name",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_StorageItems_ContainerId",
                table: "StorageItems",
                column: "ContainerId");

            migrationBuilder.CreateIndex(
                name: "IX_StorageItems_ContainerTypeSpecId",
                table: "StorageItems",
                column: "ContainerTypeSpecId");

            migrationBuilder.CreateIndex(
                name: "IX_StorageItems_PalletId",
                table: "StorageItems",
                column: "PalletId");

            migrationBuilder.CreateIndex(
                name: "IX_StorageItems_PalletTypeSpecId",
                table: "StorageItems",
                column: "PalletTypeSpecId");

            migrationBuilder.CreateIndex(
                name: "IX_StorageItems_ParentId",
                table: "StorageItems",
                column: "ParentId");

            migrationBuilder.CreateIndex(
                name: "IX_StorageItems_StatusId",
                table: "StorageItems",
                column: "StatusId");

            migrationBuilder.CreateIndex(
                name: "IX_StorageStatusTypes_Name",
                table: "StorageStatusTypes",
                column: "Name",
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "Ships");

            migrationBuilder.DropTable(
                name: "StorageItems");

            migrationBuilder.DropTable(
                name: "ContainerTypeSpecs");

            migrationBuilder.DropTable(
                name: "PalletTypeSpecs");

            migrationBuilder.DropTable(
                name: "StorageStatusTypes");
        }
    }
}
