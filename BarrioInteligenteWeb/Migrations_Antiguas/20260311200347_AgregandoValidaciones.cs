using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace BarrioInteligenteWeb.Migrations
{
    /// <inheritdoc />
    public partial class AgregandoValidaciones : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "Validaciones",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    FechaVoto = table.Column<DateTime>(type: "datetime2", nullable: false),
                    ReporteId = table.Column<int>(type: "int", nullable: false),
                    UsuarioId = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Validaciones", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Validaciones_Reportes_ReporteId",
                        column: x => x.ReporteId,
                        principalTable: "Reportes",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_Validaciones_Usuarios_UsuarioId",
                        column: x => x.UsuarioId,
                        principalTable: "Usuarios",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_Validaciones_ReporteId",
                table: "Validaciones",
                column: "ReporteId");

            migrationBuilder.CreateIndex(
                name: "IX_Validaciones_UsuarioId",
                table: "Validaciones",
                column: "UsuarioId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "Validaciones");
        }
    }
}
