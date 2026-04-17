using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace BarrioInteligenteWeb.Migrations
{
    /// <inheritdoc />
    public partial class AddComentariosLikes : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "ComentariosLikes",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    ComentarioId = table.Column<int>(type: "int", nullable: false),
                    UsuarioId = table.Column<int>(type: "int", nullable: false),
                    FechaCreacion = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ComentariosLikes", x => x.Id);
                    table.ForeignKey(
                        name: "FK_ComentariosLikes_Comentarios_ComentarioId",
                        column: x => x.ComentarioId,
                        principalTable: "Comentarios",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_ComentariosLikes_Usuarios_UsuarioId",
                        column: x => x.UsuarioId,
                        principalTable: "Usuarios",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateIndex(
                name: "IX_ComentariosLikes_ComentarioId_UsuarioId",
                table: "ComentariosLikes",
                columns: new[] { "ComentarioId", "UsuarioId" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_ComentariosLikes_UsuarioId",
                table: "ComentariosLikes",
                column: "UsuarioId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "ComentariosLikes");
        }
    }
}
