using AtlasBank.SharedKernel.Primitives;
using MediatR;

namespace AtlasBank.Accounts.Application.Commands.CreateAccount;

/// <summary>
/// Comando para criação de uma nova conta no AtlasBank.
/// Implementa IRequest do MediatR para ser despachado pelo Handler.
/// </summary>
public sealed record CreateAccountCommand(
    string FullName,
    string Email,
    string Document,
    string Password) : IRequest<Result<Guid>>;