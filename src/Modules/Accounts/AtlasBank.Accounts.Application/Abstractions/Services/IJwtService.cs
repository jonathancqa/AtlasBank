using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using AtlasBank.Accounts.Domain.Entities;

namespace AtlasBank.Accounts.Application.Abstractions.Services
{
    /// <summary>
    /// Contrato para geração de tokens JWT.
    /// </summary>
    public interface IJwtService
    {
        /// <summary>
        /// Gera um token JWT para a conta informada.
        /// </summary>
        string GenerateToken(Account account);        
    }
}