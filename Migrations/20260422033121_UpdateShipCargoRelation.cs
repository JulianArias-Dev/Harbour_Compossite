using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Harbour.Migrations
{
    /// <inheritdoc />
    public partial class UpdateShipCargoRelation : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "ShipId",
                table: "StorageItems",
                type: "nvarchar(50)",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "Status",
                table: "Ships",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.CreateIndex(
                name: "IX_StorageItems_ShipId",
                table: "StorageItems",
                column: "ShipId");

            migrationBuilder.AddForeignKey(
                name: "FK_StorageItems_Ships_ShipId",
                table: "StorageItems",
                column: "ShipId",
                principalTable: "Ships",
                principalColumn: "ShipId",
                onDelete: ReferentialAction.Cascade);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_StorageItems_Ships_ShipId",
                table: "StorageItems");

            migrationBuilder.DropIndex(
                name: "IX_StorageItems_ShipId",
                table: "StorageItems");

            migrationBuilder.DropColumn(
                name: "ShipId",
                table: "StorageItems");

            migrationBuilder.DropColumn(
                name: "Status",
                table: "Ships");
        }
    }
}
