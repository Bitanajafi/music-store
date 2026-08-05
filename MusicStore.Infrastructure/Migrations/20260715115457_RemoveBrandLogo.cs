using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace MusicStore.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class RemoveBrandLogo : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "LogoUrl",
                table: "Brands");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "LogoUrl",
                table: "Brands",
                type: "nvarchar(500)",
                maxLength: 500,
                nullable: true);
        }
    }
}
