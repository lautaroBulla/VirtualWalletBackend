using FluentValidation;
using VirtualWallet.Application.DTOs;

namespace VirtualWallet.Application.Validators
{
    public class DepositRequestDtoValidator : AbstractValidator<DepositRequestDto>
    {
        public DepositRequestDtoValidator() 
        { 
            RuleFor(x => x.Amount)
                .GreaterThan(0).WithMessage("Deposit amount must be greater than zero.");
        }
    }
}
