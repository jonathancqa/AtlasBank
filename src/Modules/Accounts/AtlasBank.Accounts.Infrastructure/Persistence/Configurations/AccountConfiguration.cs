using AtlasBank.Accounts.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace AtlasBank.Accounts.Infrastructure.Persistence.Configurations;

/// <summary>
/// Configuração do mapeamento da entidade Account para o banco de dados.
/// Separa responsabilidade de mapeamento do DbContext.
/// </summary>
public sealed class AccountConfiguration : IEntityTypeConfiguration<Account>
{
    public void Configure(EntityTypeBuilder<Account> builder)
    {
        builder.ToTable("accounts");

        builder.HasKey(a => a.Id);

        builder.Property(a => a.Id)
            .ValueGeneratedNever(); // Id gerado pelo domínio, não pelo banco

        builder.Property(a => a.FullName)
            .IsRequired()
            .HasMaxLength(200);

        builder.Property(a => a.PasswordHash)
            .IsRequired()
            .HasMaxLength(500);

        builder.Property(a => a.Status)
            .IsRequired()
            .HasConversion<string>() // Persiste como string: "Active", "Inactive"
            .HasMaxLength(20);

        builder.Property(a => a.CreatedAt)
            .IsRequired();

        // Índices únicos via OwnsOne
        builder.OwnsOne(a => a.Email, email =>
        {
            email.Property(e => e.Address)
                .HasColumnName("email")
                .IsRequired()
                .HasMaxLength(254);

            email.HasIndex(e => e.Address)
                .IsUnique();
        });

        builder.OwnsOne(a => a.Document, document =>
        {
            document.Property(d => d.Number)
                .HasColumnName("document")
                .IsRequired()
                .HasMaxLength(11);

            document.HasIndex(d => d.Number)
                .IsUnique();
        });
    }
}