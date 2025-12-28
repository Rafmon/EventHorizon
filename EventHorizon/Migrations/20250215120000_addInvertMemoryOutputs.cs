using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace EventHorizon.Migrations
{
    /// <inheritdoc />
    public partial class addInvertMemoryOutputs : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<bool>(
                name: "InvertMemoryOutputs",
                table: "Settings",
                type: "INTEGER",
                nullable: false,
                defaultValue: false);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "InvertMemoryOutputs",
                table: "Settings");
        }
    }
}
