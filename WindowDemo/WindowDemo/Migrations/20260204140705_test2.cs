using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace WindowDemo.Migrations
{
    /// <inheritdoc />
    public partial class test2 : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "CategoryId",
                table: "BoardGames",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.CreateTable(
                name: "ProduktCategory",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Kategori = table.Column<string>(type: "nvarchar(max)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ProduktCategory", x => x.Id);
                });

            migrationBuilder.CreateIndex(
                name: "IX_BoardGames_CategoryId",
                table: "BoardGames",
                column: "CategoryId");

            migrationBuilder.AddForeignKey(
                name: "FK_BoardGames_ProduktCategory_CategoryId",
                table: "BoardGames",
                column: "CategoryId",
                principalTable: "ProduktCategory",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_BoardGames_ProduktCategory_CategoryId",
                table: "BoardGames");

            migrationBuilder.DropTable(
                name: "ProduktCategory");

            migrationBuilder.DropIndex(
                name: "IX_BoardGames_CategoryId",
                table: "BoardGames");

            migrationBuilder.DropColumn(
                name: "CategoryId",
                table: "BoardGames");
        }
    }
}
