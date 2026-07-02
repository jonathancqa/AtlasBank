using AtlasBank.Accounts.Domain.Events;
using AtlasBank.Accounts.Domain.ValueObjects;
using AtlasBank.SharedKernel.Abstractions;
using AtlasBank.SharedKernel.Primitives;

namespace AtlasBank.Accounts.Domain.Entities;

/// <summary>
/// Aggregate Root do módulo Accounts.
/// Representa uma conta de usuário no AtlasBank.
/// </summary>
public sealed class Account : AggregateRoot
{
    /// <summary>Nome completo do titular.</summary>
    public string FullName { get; private set; } = null!;

    /// <summary>E-mail do titular.</summary>
    public Email Email { get; private set; } = null!;

    /// <summary>CPF do titular.</summary>
    public Document Document { get; private set; } = null!;

    /// <summary>Hash da senha — nunca armazenar senha em texto puro.</summary>
    public string PasswordHash { get; private set; } = null!;

    /// <summary>Status atual da conta.</summary>
    public AccountStatus Status { get; private set; }

    /// <summary>Data de criação da conta.</summary>
    public DateTime CreatedAt { get; private set; }

    private Account() { } // EF Core

    private Account(
        Guid id,
        string fullName,
        Email email,
        Document document,
        string passwordHash) : base(id)
    {
        FullName = fullName;
        Email = email;
        Document = document;
        PasswordHash = passwordHash;
        Status = AccountStatus.Active;
        CreatedAt = DateTime.UtcNow;
    }

    /// <summary>
    /// Cria uma nova conta validando os dados de entrada.
    /// </summary>
    public static Result<Account> Create(
        string fullName,
        string email,
        string document,
        string passwordHash)
    {
        if (string.IsNullOrWhiteSpace(fullName))
            return Result.Failure<Account>("Full name is required.");

        if (fullName.Length < 3)
            return Result.Failure<Account>("Full name must be at least 3 characters.");

        var emailResult = Email.Create(email);
        if (emailResult.IsFailure)
            return Result.Failure<Account>(emailResult.Error);

        var documentResult = Document.Create(document);
        if (documentResult.IsFailure)
            return Result.Failure<Account>(documentResult.Error);

        if (string.IsNullOrWhiteSpace(passwordHash))
            return Result.Failure<Account>("Password hash is required.");

        var account = new Account(
            Guid.NewGuid(),
            fullName,
            emailResult.Value,
            documentResult.Value,
            passwordHash);

        account.RaiseDomainEvent(new AccountCreatedEvent(
            account.Id,
            account.FullName,
            account.Email.Address,
            account.CreatedAt));

        return Result.Success(account);
    }

    /// <summary>
    /// Desativa a conta impedindo novas operações.
    /// </summary>
    public Result Deactivate()
    {
        if (Status == AccountStatus.Inactive)
            return Result.Failure("Account is already inactive.");

        Status = AccountStatus.Inactive;
        return Result.Success();
    }

    /// <summary>
    /// Reativa uma conta previamente desativada.
    /// </summary>
    public Result Activate()
    {
        if (Status == AccountStatus.Active)
            return Result.Failure("Account is already active.");

        Status = AccountStatus.Active;
        return Result.Success();
    }
}

/// <summary>
/// Status possíveis de uma conta no AtlasBank.
/// </summary>
public enum AccountStatus
{
    Active,
    Inactive
}