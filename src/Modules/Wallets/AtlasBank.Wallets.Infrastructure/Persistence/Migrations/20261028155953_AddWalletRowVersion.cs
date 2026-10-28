using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace AtlasBank.Wallets.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class AddWalletRowVersion : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<uint>(
                name: "xmin",
                schema: "wallets",
                table: "wallets",
                type: "xid",
                rowVersion: true,
                nullable: false,
                defaultValue: 0u);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "xmin",
                schema: "wallets",
                table: "wallets");
        }
    }
}
