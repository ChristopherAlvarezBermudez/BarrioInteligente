using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace BarrioInteligenteWeb.Migrations
{
    /// <inheritdoc />
    public partial class AddDenunciasUsuarios_Y_ReputacionDefaults : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "DenunciasUsuarios",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    DenuncianteId = table.Column<int>(type: "int", nullable: false),
                    ReportadoId = table.Column<int>(type: "int", nullable: false),
                    Motivo = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    Descripcion = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: true),
                    Fecha = table.Column<DateTime>(type: "datetime2", nullable: false),
                    Procesada = table.Column<bool>(type: "bit", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_DenunciasUsuarios", x => x.Id);
                    table.ForeignKey(
                        name: "FK_DenunciasUsuarios_Usuarios_DenuncianteId",
                        column: x => x.DenuncianteId,
                        principalTable: "Usuarios",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_DenunciasUsuarios_Usuarios_ReportadoId",
                        column: x => x.ReportadoId,
                        principalTable: "Usuarios",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateIndex(
                name: "IX_DenunciasUsuarios_DenuncianteId",
                table: "DenunciasUsuarios",
                column: "DenuncianteId");

            migrationBuilder.CreateIndex(
                name: "IX_DenunciasUsuarios_ReportadoId",
                table: "DenunciasUsuarios",
                column: "ReportadoId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "DenunciasUsuarios");
        }
    }
}
