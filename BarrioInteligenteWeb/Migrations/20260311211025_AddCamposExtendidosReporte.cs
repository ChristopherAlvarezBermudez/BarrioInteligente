using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace BarrioInteligenteWeb.Migrations
{
    /// <inheritdoc />
    public partial class AddCamposExtendidosReporte : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Comentarios_Usuarios_UsuarioId",
                table: "Comentarios");

            migrationBuilder.DropForeignKey(
                name: "FK_Validaciones_Usuarios_UsuarioId",
                table: "Validaciones");

            migrationBuilder.AddColumn<string>(
                name: "ImagenUrl",
                table: "Reportes",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<double>(
                name: "Latitud",
                table: "Reportes",
                type: "float",
                nullable: false,
                defaultValue: 0.0);

            migrationBuilder.AddColumn<double>(
                name: "Longitud",
                table: "Reportes",
                type: "float",
                nullable: false,
                defaultValue: 0.0);

            migrationBuilder.AddColumn<int>(
                name: "UsuarioId",
                table: "Reportes",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.CreateIndex(
                name: "IX_Reportes_UsuarioId",
                table: "Reportes",
                column: "UsuarioId");

            // Asignar reportes sin usuario al primer usuario disponible, o eliminarlos si no hay usuarios
            migrationBuilder.Sql(@"
                IF EXISTS (SELECT 1 FROM Usuarios)
                BEGIN
                    UPDATE Reportes
                    SET UsuarioId = (SELECT TOP 1 Id FROM Usuarios ORDER BY Id)
                    WHERE UsuarioId = 0;
                END
                ELSE
                BEGIN
                    DELETE FROM Reportes WHERE UsuarioId = 0;
                END
            ");

            migrationBuilder.AddForeignKey(
                name: "FK_Comentarios_Usuarios_UsuarioId",
                table: "Comentarios",
                column: "UsuarioId",
                principalTable: "Usuarios",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_Reportes_Usuarios_UsuarioId",
                table: "Reportes",
                column: "UsuarioId",
                principalTable: "Usuarios",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_Validaciones_Usuarios_UsuarioId",
                table: "Validaciones",
                column: "UsuarioId",
                principalTable: "Usuarios",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Comentarios_Usuarios_UsuarioId",
                table: "Comentarios");

            migrationBuilder.DropForeignKey(
                name: "FK_Reportes_Usuarios_UsuarioId",
                table: "Reportes");

            migrationBuilder.DropForeignKey(
                name: "FK_Validaciones_Usuarios_UsuarioId",
                table: "Validaciones");

            migrationBuilder.DropIndex(
                name: "IX_Reportes_UsuarioId",
                table: "Reportes");

            migrationBuilder.DropColumn(
                name: "ImagenUrl",
                table: "Reportes");

            migrationBuilder.DropColumn(
                name: "Latitud",
                table: "Reportes");

            migrationBuilder.DropColumn(
                name: "Longitud",
                table: "Reportes");

            migrationBuilder.DropColumn(
                name: "UsuarioId",
                table: "Reportes");

            migrationBuilder.AddForeignKey(
                name: "FK_Comentarios_Usuarios_UsuarioId",
                table: "Comentarios",
                column: "UsuarioId",
                principalTable: "Usuarios",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_Validaciones_Usuarios_UsuarioId",
                table: "Validaciones",
                column: "UsuarioId",
                principalTable: "Usuarios",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }
    }
}
