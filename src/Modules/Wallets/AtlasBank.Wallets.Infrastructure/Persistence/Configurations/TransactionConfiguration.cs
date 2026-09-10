using AtlasBank.Wallets.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace AtlasBank.Wallets.Infrastructure.Persistence.Configurations;

/// <summary>
/// Configuração do mapeamento da entidade Transaction para o banco de dados.
/// Transações são imutáveis — nunca atualizadas após criadas.
/// </summary>
public sealed class TransactionConfiguration : IEntityTypeConfiguration<Transaction>
{
    public void Configure(EntityTypeBuilder<Transaction> builder)
    {
        builder.ToTable("transactions");

        builder.HasKey(t => t.Id);

        builder.Property(t => t.Id)
            .ValueGeneratedNever();

        builder.Property(t => t.WalletId)
            .IsRequired();

        builder.Property(t => t.Type)
            .IsRequired()
            .HasConversion<string>()
            .HasMaxLength(20);

        builder.Property(t => t.RelatedWalletId);

        builder.Property(t => t.CreatedAt)
            .IsRequired();

        // Mapeamento do Value Object Money
        builder.OwnsOne(t => t.Amount, amount =>
        {
            amount.Property(a => a.Amount)
                .HasColumnName("amount")
                .HasPrecision(18, 2)
                .IsRequired();

            amount.Property(a => a.Currency)
                .HasColumnName("currency")
                .HasMaxLength(3)
                .IsRequired();
        });

        // Mapeamento do Value Object IdempotencyKey
        builder.OwnsOne(t => t.IdempotencyKey, key =>
        {
            key.Property(k => k.Value)
                .HasColumnName("idempotency_key")
                .HasMaxLength(100)
                .IsRequired();

            key.HasIndex(k => k.Value)
                .IsUnique();
        });

        // Índice para busca por carteira ordenada por data
        builder.HasIndex(t => new { t.WalletId, t.CreatedAt });
    }
}