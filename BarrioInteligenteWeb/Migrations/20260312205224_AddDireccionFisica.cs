using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace BarrioInteligenteWeb.Migrations
{
    /// <inheritdoc />
    public partial class AddDireccionFisica : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "DireccionFisica",
                table: "Reportes",
                type: "nvarchar(max)",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "DireccionFisica",
                table: "Reportes");
        }
    }
}
