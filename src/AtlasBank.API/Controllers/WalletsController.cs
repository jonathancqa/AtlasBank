using AtlasBank.SharedKernel.Primitives;
using AtlasBank.Wallets.Application.Commands.CreateWallet;
using AtlasBank.Wallets.Application.Commands.Deposit;
using AtlasBank.Wallets.Application.Commands.Transfer;
using AtlasBank.Wallets.Application.Commands.Withdraw;
using AtlasBank.Wallets.Application.Queries.GetBalance;
using AtlasBank.Wallets.Application.Queries.GetStatement;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace AtlasBank.API.Controllers;

[ApiController]
[Route("api/[controller]")]
public sealed class WalletsController : ControllerBase
{
    private readonly IMediator _mediator;

    public WalletsController(IMediator mediator)
        => _mediator = mediator;

    /// <summary>Cria uma nova carteira para uma conta.</summary>
    [HttpPost]
    [ProducesResponseType(typeof(ApiResponse<Guid>), StatusCodes.Status201Created)]
    [ProducesResponseType(typeof(ApiResponse<Guid>), StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> Create(
        [FromBody] CreateWalletCommand command,
        CancellationToken cancellationToken)
    {
        var result = await _mediator.Send(command, cancellationToken);

        if (result.IsFailure)
            return BadRequest(ApiResponse<Guid>.Fail(result.Error));

        return Created(
            $"api/wallets/{result.Value}",
            ApiResponse<Guid>.Ok(result.Value, "Wallet created successfully."));
    }

    /// <summary>Consulta o saldo de uma carteira.</summary>
    [HttpGet("{walletId:guid}/balance")]
    [ProducesResponseType(typeof(ApiResponse<GetBalanceResponse>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse<GetBalanceResponse>), StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetBalance(
        Guid walletId,
        CancellationToken cancellationToken)
    {
        var result = await _mediator.Send(new GetBalanceQuery(walletId), cancellationToken);

        if (result.IsFailure)
            return NotFound(ApiResponse<GetBalanceResponse>.Fail(result.Error));

        return Ok(ApiResponse<GetBalanceResponse>.Ok(result.Value));
    }

    /// <summary>Consulta o extrato de uma carteira.</summary>
    [HttpGet("{walletId:guid}/statement")]
    [ProducesResponseType(typeof(ApiResponse<GetStatementResponse>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse<GetStatementResponse>), StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetStatement(
        Guid walletId,
        [FromQuery] DateTime? from,
        [FromQuery] DateTime? to,
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 20,
        CancellationToken cancellationToken = default)
    {
        var query = new GetStatementQuery(walletId, from, to, page, pageSize);
        var result = await _mediator.Send(query, cancellationToken);

        if (result.IsFailure)
            return NotFound(ApiResponse<GetStatementResponse>.Fail(result.Error));

        return Ok(ApiResponse<GetStatementResponse>.Ok(result.Value));
    }

    /// <summary>Realiza um depósito em uma carteira.</summary>
    [HttpPost("{walletId:guid}/deposit")]
    [ProducesResponseType(typeof(ApiResponse<string>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse<string>), StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> Deposit(
        Guid walletId,
        [FromBody] DepositRequest request,
        [FromHeader(Name = "Idempotency-Key")] string idempotencyKey,
        CancellationToken cancellationToken)
    {
        var command = new DepositCommand(walletId, request.Amount, request.Currency, idempotencyKey);
        var result = await _mediator.Send(command, cancellationToken);

        if (result.IsFailure)
            return BadRequest(ApiResponse<string>.Fail(result.Error));

        return Ok(ApiResponse<string>.Ok("Deposit completed successfully."));
    }

    /// <summary>Realiza um saque de uma carteira.</summary>
    [HttpPost("{walletId:guid}/withdraw")]
    [ProducesResponseType(typeof(ApiResponse<string>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse<string>), StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> Withdraw(
        Guid walletId,
        [FromBody] WithdrawRequest request,
        [FromHeader(Name = "Idempotency-Key")] string idempotencyKey,
        CancellationToken cancellationToken)
    {
        var command = new WithdrawCommand(walletId, request.Amount, request.Currency, idempotencyKey);
        var result = await _mediator.Send(command, cancellationToken);

        if (result.IsFailure)
            return BadRequest(ApiResponse<string>.Fail(result.Error));

        return Ok(ApiResponse<string>.Ok("Withdrawal completed successfully."));
    }

    /// <summary>Realiza uma transferência entre carteiras.</summary>
    [HttpPost("{walletId:guid}/transfer")]
    [ProducesResponseType(typeof(ApiResponse<string>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse<string>), StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> Transfer(
        Guid walletId,
        [FromBody] TransferRequest request,
        [FromHeader(Name = "Idempotency-Key")] string idempotencyKey,
        CancellationToken cancellationToken)
    {
        var command = new TransferCommand(
            walletId,
            request.DestinationWalletId,
            request.Amount,
            request.Currency,
            idempotencyKey);

        var result = await _mediator.Send(command, cancellationToken);

        if (result.IsFailure)
            return BadRequest(ApiResponse<string>.Fail(result.Error));

        return Ok(ApiResponse<string>.Ok("Transfer completed successfully."));
    }
}

// Request DTOs
public sealed record DepositRequest(decimal Amount, string Currency = "BRL");
public sealed record WithdrawRequest(decimal Amount, string Currency = "BRL");
public sealed record TransferRequest(Guid DestinationWalletId, decimal Amount, string Currency = "BRL");