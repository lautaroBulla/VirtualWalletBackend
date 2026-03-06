using FluentValidation;
using VirtualWallet.Application.DTOs;

namespace VirtualWallet.Application.Validators
{
    public class TransferRequestDtoValidator : AbstractValidator<TransferRequestDto>
    {
        public TransferRequestDtoValidator()
        {
            RuleFor(x => x.ToAccountNumber)
                .NotEmpty().WithMessage("To account number is required.")
                .Length(20).WithMessage("To account number must be exactly 10 characters long.");

            RuleFor(x => x.Amount)
                .GreaterThan(0).WithMessage("Amount must be greater than zero.");
        }
    }
}
