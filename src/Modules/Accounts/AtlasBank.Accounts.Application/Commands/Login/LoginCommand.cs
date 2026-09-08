using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using AtlasBank.SharedKernel.Primitives;
using MediatR;

namespace AtlasBank.Accounts.Application.Commands.Login;

/// <summary>
/// Comando para autenticação de uma conta no AtlasBank.
/// Retorna o token JWT em caso de sucesso.
/// </summary>
public sealed record LoginCommand(
    string Email,
    string Password) : IRequest<Result<string>>;