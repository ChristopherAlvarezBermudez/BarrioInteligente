using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace BarrioInteligenteWeb.Migrations
{
    /// <inheritdoc />
    public partial class RolesInsigniasSoftDelete : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<bool>(
                name: "EsAdmin",
                table: "Usuarios",
                type: "bit",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<bool>(
                name: "EsEliminado",
                table: "Reportes",
                type: "bit",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<bool>(
                name: "EsEliminado",
                table: "Comentarios",
                type: "bit",
                nullable: false,
                defaultValue: false);

            migrationBuilder.CreateTable(
                name: "Insignias",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Nombre = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    IconoEmoji = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    ColorCss = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Insignias", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "InsigniaUsuario",
                columns: table => new
                {
                    InsigniasId = table.Column<int>(type: "int", nullable: false),
                    UsuariosId = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_InsigniaUsuario", x => new { x.InsigniasId, x.UsuariosId });
                    table.ForeignKey(
                        name: "FK_InsigniaUsuario_Insignias_InsigniasId",
                        column: x => x.InsigniasId,
                        principalTable: "Insignias",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_InsigniaUsuario_Usuarios_UsuariosId",
                        column: x => x.UsuariosId,
                        principalTable: "Usuarios",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_InsigniaUsuario_UsuariosId",
                table: "InsigniaUsuario",
                column: "UsuariosId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "InsigniaUsuario");

            migrationBuilder.DropTable(
                name: "Insignias");

            migrationBuilder.DropColumn(
                name: "EsAdmin",
                table: "Usuarios");

            migrationBuilder.DropColumn(
                name: "EsEliminado",
                table: "Reportes");

            migrationBuilder.DropColumn(
                name: "EsEliminado",
                table: "Comentarios");
        }
    }
}
