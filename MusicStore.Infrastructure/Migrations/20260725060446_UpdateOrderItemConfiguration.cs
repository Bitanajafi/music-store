using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace MusicStore.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class UpdateOrderItemConfiguration : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateIndex(
                name: "IX_OrderItems_OrderId_ProductId",
                table: "OrderItems",
                columns: new[] { "OrderId", "ProductId" },
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_OrderItems_OrderId_ProductId",
                table: "OrderItems");
        }
    }
}
