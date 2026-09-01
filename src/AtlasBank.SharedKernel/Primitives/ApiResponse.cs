using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace AtlasBank.SharedKernel.Primitives
{

    /// <summary>
    /// Envelope padrão para todas as respostas da API.
    /// Garante consistência no formato de retorno independente do endpoint.
    /// </summary>
    public sealed class ApiResponse<T>
    {
        /// <summary>Indica se a operação foi bem-sucedida.</summary>
        public bool Success { get; init; }

        /// <summary>Mensagem descritiva do resulta.</summary>
        public string? Message { get; init; }

        ///<summary>Dados retornados em caso de sucesso.</summary>
        public T? Data { get; init; }

        private ApiResponse() { }

        /// <summary>Cria uma resposta de sucesso com dados e mensagem opcional.</summary>
        public static ApiResponse<T> Ok(T data, string? message = null)
            => new() { Success = true, Data = data, Message = message };

        /// <summary>Cria uma resposta de falha com mensagem de erro.</summary>
        public static ApiResponse<T> Fail(string message)
            => new() { Success = false, Message = message, Data = default };
    }
}