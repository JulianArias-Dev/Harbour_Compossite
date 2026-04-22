using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Harbour.Migrations
{
    /// <inheritdoc />
    public partial class AddPalletConstructor : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "MaxCapacity",
                table: "StorageItems");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<decimal>(
                name: "MaxCapacity",
                table: "StorageItems",
                type: "decimal(10,2)",
                precision: 10,
                scale: 2,
                nullable: true);
        }
    }
}
