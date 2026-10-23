using AtlasBank.Wallets.Application.Commands.CreateWallet;
using FluentValidation;

namespace AtlasBank.Wallets.Application.Validators;

public sealed class CreateWalletCommandValidator : AbstractValidator<CreateWalletCommand>
{
    public CreateWalletCommandValidator()
    {
        RuleFor(x => x.AccountId)
            .NotEmpty().WithMessage("Account ID is required.");

        RuleFor(x => x.Currency)
            .NotEmpty().WithMessage("Currency is required.")
            .Length(3).WithMessage("Currency must be a 3-letter ISO code (e.g. BRL, USD).")
            .Matches("^[A-Z]{3}$").WithMessage("Currency must contain only uppercase letters.");
    }
}