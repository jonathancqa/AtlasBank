using AtlasBank.Wallets.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace AtlasBank.Wallets.Infrastructure.Persistence.Configurations;

/// <summary>
/// Configuração do mapeamento da entidade Wallet para o banco de dados.
/// </summary>
public sealed class WalletConfiguration : IEntityTypeConfiguration<Wallet>
{
    public void Configure(EntityTypeBuilder<Wallet> builder)
    {
        builder.ToTable("wallets");

        builder.HasKey(w => w.Id);

        builder.Property(w => w.Id)
            .ValueGeneratedNever();

        builder.Property(w => w.AccountId)
            .IsRequired();

        builder.Property(w => w.CreatedAt)
            .IsRequired();

        // Mapeamento do Value Object Money (Balance)
        builder.OwnsOne(w => w.Balance, balance =>
        {
            balance.Property(b => b.Amount)
                .HasColumnName("balance_amount")
                .HasPrecision(18, 2)
                .IsRequired();

            balance.Property(b => b.Currency)
                .HasColumnName("balance_currency")
                .HasMaxLength(3)
                .IsRequired();
        });

        // Relacionamento com Transactions
        builder.HasMany(w => w.Transactions)
            .WithOne()
            .HasForeignKey(t => t.WalletId)
            .OnDelete(DeleteBehavior.Cascade);

        // Índice único — uma conta só pode ter uma carteira
        builder.HasIndex(w => w.AccountId)
            .IsUnique();
    }
}