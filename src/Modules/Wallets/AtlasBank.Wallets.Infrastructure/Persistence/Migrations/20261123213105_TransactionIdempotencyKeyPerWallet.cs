using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace AtlasBank.Wallets.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class TransactionIdempotencyKeyPerWallet : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_transactions_idempotency_key",
                schema: "wallets",
                table: "transactions");

            migrationBuilder.CreateIndex(
                name: "IX_transactions_WalletId_idempotency_key",
                schema: "wallets",
                table: "transactions",
                columns: new[] { "WalletId", "idempotency_key" },
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_transactions_WalletId_idempotency_key",
                schema: "wallets",
                table: "transactions");

            migrationBuilder.CreateIndex(
                name: "IX_transactions_idempotency_key",
                schema: "wallets",
                table: "transactions",
                column: "idempotency_key",
                unique: true);
        }
    }
}
