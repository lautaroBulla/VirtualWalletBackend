using FluentValidation;
using VirtualWallet.Application.DTOs;

namespace VirtualWallet.Application.Validators
{
    public class WithdrawalRequestDtoValidator : AbstractValidator<WithdrawalRequestDto>
    {
        public WithdrawalRequestDtoValidator()
        {
            RuleFor(x => x.Amount)
                .GreaterThan(0).WithMessage("Withdrawal amount must be greater than zero.");
        }
    }
}
