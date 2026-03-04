using FluentValidation;
using VirtualWallet.Application.DTOs;
using VirtualWallet.Application.Interfaces;
using VirtualWallet.Application.Interfaces.Repositories;
using VirtualWallet.Domain.Entities;
using VirtualWallet.Domain.Enums;
using VirtualWallet.Domain.Exceptions;

namespace VirtualWallet.Application.Services
{
    public class TransactionService : ITransactionService
    {
        private readonly IAccountRepository _accountRepository;
        private readonly ITransactionRepository _transactionRepository;
        private readonly IUnitOfWork _unitOfWork;
        private readonly IValidator<TransferRequestDto> _validator;

        public TransactionService(
            IAccountRepository accountRepository,
            ITransactionRepository transactionRepository,
            IUnitOfWork unitOfWork,
            IValidator<TransferRequestDto> validator)
        {
            _accountRepository = accountRepository;
            _transactionRepository = transactionRepository;
            _unitOfWork = unitOfWork;
            _validator = validator;
        }

        public async Task MakeTransferAsync(TransferRequestDto request)
        {
            var validationResult = await _validator.ValidateAsync(request);
            if (!validationResult.IsValid)
            {
                throw new ValidationException(validationResult.Errors);
            }

            var fromAccount = await _accountRepository.GetByIdAsync(request.FromAccountId);
            if (fromAccount == null)
            {
                throw new BadRequestException(DomainErrors.Account.FromAccountNotFound);
            }

            var toAccount = await _accountRepository.GetByAccountNumberAsync(request.ToAccountNumber);
            if (toAccount == null)
            {
                throw new BadRequestException(DomainErrors.Account.ToAccountNotFound);
            }

            if (fromAccount.Id == toAccount.Id)
            {
                throw new BadRequestException(DomainErrors.Transaction.SameAccountTransfer);
            }

            if (fromAccount.Balance < request.Amount)
            {
                throw new BadRequestException(DomainErrors.Transaction.InsufficientFunds);
            }

            await _unitOfWork.BeginTransactionAsync();

            try
            {
                fromAccount.Balance -= request.Amount;
                toAccount.Balance += request.Amount;

                await _accountRepository.UpdateAsync(fromAccount);
                await _accountRepository.UpdateAsync(toAccount);

                var transaction = new Transaction
                {
                    FromAccountId = fromAccount.Id,
                    ToAccountId = toAccount.Id,
                    Amount = request.Amount,
                    Type = TransactionType.Transfer,
                    Status = TransactionStatus.Completed,
                    Reference = request.Reference
                };

                await _transactionRepository.AddAsync(transaction);

                await _unitOfWork.CommitTransactionAsync();
            }
            catch (Exception)
            {
                await _unitOfWork.RollbackTransactionAsync();

                throw;
            }
        }
    }
}
