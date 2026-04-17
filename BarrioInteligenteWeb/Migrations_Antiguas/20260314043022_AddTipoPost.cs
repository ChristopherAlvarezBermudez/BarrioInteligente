using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace BarrioInteligenteWeb.Migrations
{
    /// <inheritdoc />
    public partial class AddTipoPost : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "TipoPost",
                table: "Reportes",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "TipoPost",
                table: "Reportes");
        }
    }
}
