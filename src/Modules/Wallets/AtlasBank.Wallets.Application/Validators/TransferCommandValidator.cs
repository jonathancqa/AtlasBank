using AtlasBank.Wallets.Application.Commands.Transfer;
using FluentValidation;

namespace AtlasBank.Wallets.Application.Validators;

public sealed class TransferCommandValidator : AbstractValidator<TransferCommand>
{
    public TransferCommandValidator()
    {
        RuleFor(x => x.SourceWalletId)
            .NotEmpty().WithMessage("Source wallet ID is required.");

        RuleFor(x => x.DestinationWalletId)
            .NotEmpty().WithMessage("Destination wallet ID is required.")
            .NotEqual(x => x.SourceWalletId).WithMessage("Cannot transfer to the same wallet.");

        RuleFor(x => x.Amount)
            .GreaterThan(0).WithMessage("Amount must be greater than zero.");

        RuleFor(x => x.Currency)
            .NotEmpty().WithMessage("Currency is required.")
            .Length(3).WithMessage("Currency must be a 3-letter ISO code (e.g. BRL, USD).");

        RuleFor(x => x.IdempotencyKey)
            .NotEmpty().WithMessage("Idempotency key is required.")
            .MaximumLength(100).WithMessage("Idempotency key must not exceed 100 characters.");
    }
}