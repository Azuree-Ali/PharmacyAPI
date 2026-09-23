using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Migrations;
using PharmacyAPI.DataAccess;

#nullable disable

namespace PharmacyAPI.Migrations
{
    [DbContext(typeof(ApplicationDbContext))]
    [Migration("20260923185147_OrderDeliveryConfirmation")]
    public partial class OrderDeliveryConfirmation : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<bool>(
                name: "IsPaid",
                table: "Orders",
                type: "bit",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<DateTime>(
                name: "DeliveryConfirmationRequestedAt",
                table: "Orders",
                type: "datetime2",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "OrderId",
                table: "ChatMessages",
                type: "int",
                nullable: true);

            migrationBuilder.AddColumn<bool>(
                name: "OffersDeliveryActions",
                table: "ChatMessages",
                type: "bit",
                nullable: false,
                defaultValue: false);

            migrationBuilder.CreateTable(
                name: "OrderItemBatchAllocations",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    OrderItemId = table.Column<int>(type: "int", nullable: false),
                    ProductBatchId = table.Column<int>(type: "int", nullable: false),
                    Quantity = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_OrderItemBatchAllocations", x => x.Id);
                    table.ForeignKey(
                        name: "FK_OrderItemBatchAllocations_OrderItems_OrderItemId",
                        column: x => x.OrderItemId,
                        principalTable: "OrderItems",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_OrderItemBatchAllocations_ProductBatches_ProductBatchId",
                        column: x => x.ProductBatchId,
                        principalTable: "ProductBatches",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateIndex(
                name: "IX_ChatMessages_OrderId",
                table: "ChatMessages",
                column: "OrderId");

            migrationBuilder.CreateIndex(
                name: "IX_OrderItemBatchAllocations_OrderItemId",
                table: "OrderItemBatchAllocations",
                column: "OrderItemId");

            migrationBuilder.CreateIndex(
                name: "IX_OrderItemBatchAllocations_ProductBatchId",
                table: "OrderItemBatchAllocations",
                column: "ProductBatchId");

            migrationBuilder.AddForeignKey(
                name: "FK_ChatMessages_Orders_OrderId",
                table: "ChatMessages",
                column: "OrderId",
                principalTable: "Orders",
                principalColumn: "Id",
                onDelete: ReferentialAction.SetNull);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_ChatMessages_Orders_OrderId",
                table: "ChatMessages");

            migrationBuilder.DropTable(name: "OrderItemBatchAllocations");

            migrationBuilder.DropIndex(
                name: "IX_ChatMessages_OrderId",
                table: "ChatMessages");

            migrationBuilder.DropColumn(name: "OrderId", table: "ChatMessages");
            migrationBuilder.DropColumn(name: "OffersDeliveryActions", table: "ChatMessages");
            migrationBuilder.DropColumn(name: "IsPaid", table: "Orders");
            migrationBuilder.DropColumn(name: "DeliveryConfirmationRequestedAt", table: "Orders");
        }
    }
}
