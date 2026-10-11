using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace AtlasBank.Accounts.Application.Abstractions.Services;

/// <summary>
/// Contrato para hash e verificação de senhas.
/// </summary>
public interface IPasswordHasher
{
    /// <summary>Gera um hash seguro da senha informada.</summary>
    string Hash(string password);

    /// <summary>Verifica se a senha corresponde ao hash armazenado.</summary>
    bool Verify(string password, string hash);
}