using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace CatalogoGalacticoApi.Migrations
{
    /// <inheritdoc />
    public partial class AgregarAuthPersonaje : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "Email",
                table: "Personajes",
                type: "longtext",
                nullable: false)
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.AddColumn<string>(
                name: "Rol",
                table: "Personajes",
                type: "longtext",
                nullable: false)
                .Annotation("MySql:CharSet", "utf8mb4");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Email",
                table: "Personajes");

            migrationBuilder.DropColumn(
                name: "Rol",
                table: "Personajes");
        }
    }
}
