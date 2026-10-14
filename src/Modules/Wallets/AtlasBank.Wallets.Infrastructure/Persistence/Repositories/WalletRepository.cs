using AtlasBank.Wallets.Application.Abstractions;
using AtlasBank.Wallets.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Dapper;
using Npgsql;
using Microsoft.Extensions.Configuration;
using AtlasBank.Wallets.Application.ReadModels;

namespace AtlasBank.Wallets.Infrastructure.Persistence.Repositories;

/// <summary>
/// Implementação do repositório de carteiras usando Entity Framework Core.
/// Métodos de leitura usam AsNoTracking — sem rastreamento, mais performático.
/// Métodos de escrita usam tracking — EF Core detecta mudanças automaticamente.
/// </summary>
public sealed class WalletRepository : IWalletRepository
{
    private readonly WalletsDbContext _context;
    private readonly string? _connectionString;

    public WalletRepository(WalletsDbContext context, IConfiguration configuration)
    {
        _context = context;
        _connectionString = configuration.GetConnectionString("Default");
    }

    /// <summary>Busca para modificação — com tracking.</summary>
    public async Task<Wallet?> GetByIdAsync(
        Guid id,
        CancellationToken cancellationToken = default)
        => await _context.Wallets
            .Include(w => w.Transactions)
            .FirstOrDefaultAsync(w => w.Id == id, cancellationToken);

    /// <summary>Busca para leitura pura — sem tracking, mais performático.</summary>
    public async Task<Wallet?> GetByIdReadOnlyAsync(
        Guid id,
        CancellationToken cancellationToken = default)
        => await _context.Wallets
            .AsNoTracking()
            .Include(w => w.Transactions)
            .FirstOrDefaultAsync(w => w.Id == id, cancellationToken);

    public async Task<Wallet?> GetByAccountIdAsync(
        Guid accountId,
        CancellationToken cancellationToken = default)
        => await _context.Wallets
            .AsNoTracking()
            .Include(w => w.Transactions)
            .FirstOrDefaultAsync(w => w.AccountId == accountId, cancellationToken);

    public async Task<bool> ExistsByAccountIdAsync(
        Guid accountId,
        CancellationToken cancellationToken = default)
        => await _context.Wallets
            .AsNoTracking()
            .AnyAsync(w => w.AccountId == accountId, cancellationToken);

    public async Task<bool> ExistsByIdempotencyKeyAsync(
        string idempotencyKey,
        CancellationToken cancellationToken = default)
        => await _context.Transactions
            .AsNoTracking()
            .AnyAsync(t => t.IdempotencyKey.Value == idempotencyKey, cancellationToken);

    public async Task AddAsync(
        Wallet wallet,
        CancellationToken cancellationToken = default)
        => await _context.Wallets.AddAsync(wallet, cancellationToken);

    public async Task UpdateAsync(
        Wallet wallet,
        CancellationToken cancellationToken = default)
    {
        foreach (var transaction in wallet.Transactions)
        {
            if (_context.Entry(transaction).State == EntityState.Detached)
                _context.Transactions.Add(transaction);
        }
    }

    public async Task<(IReadOnlyList<TransactionReadModel> Transactions, int Total)> GetStatementAsync(Guid walletId, DateTime? from, DateTime? to, int page, int pageSize, CancellationToken cancellationToken = default)
    {
        await using var connection = new NpgsqlConnection(_connectionString);

        var where = "WHERE t.\"WalletId\" = @WalletId";
        if (from.HasValue) where += " AND t.\"CreatedAt\" >= @From";
        if (to.HasValue) where += " AND t.\"CreatedAt\" <= @To";

        var countSql = $"""
        SELECT COUNT(*) FROM wallets.transactions t
        {where}
        """;

        var dataSql = $"""
        SELECT 
            t."Id",
            t."WalletId",
            t."Type",
            t."RelatedWalletId",
            t."CreatedAt",
            t."amount" AS "Amount",
            t."currency" AS "Currency"
        FROM wallets.transactions t
        {where}
        ORDER BY t."CreatedAt" DESC
        LIMIT @PageSize OFFSET @Offset
        """;

        var parameters = new
        {
            WalletId = walletId,
            From = from,
            To = to,
            PageSize = pageSize,
            Offset = (page - 1) * pageSize
        };

        var total = await connection.ExecuteScalarAsync<int>(
            new CommandDefinition(countSql, parameters, cancellationToken: cancellationToken));

        var rows = await connection.QueryAsync<TransactionReadModel>(
            new CommandDefinition(dataSql, parameters, cancellationToken: cancellationToken));

        return (rows.ToList(), total);
    }
}