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
        private readonly ICurrentUserService _currentUserService;

        public TransactionService(
            IAccountRepository accountRepository,
            ITransactionRepository transactionRepository,
            IUnitOfWork unitOfWork,
            IValidator<TransferRequestDto> validator,
            ICurrentUserService currentUserService)
        {
            _accountRepository = accountRepository;
            _transactionRepository = transactionRepository;
            _unitOfWork = unitOfWork;
            _validator = validator;
            _currentUserService = currentUserService;
        }

        public async Task MakeTransferAsync(TransferRequestDto request)
        {
            var validationResult = await _validator.ValidateAsync(request);
            if (!validationResult.IsValid)
            {
                throw new ValidationException(validationResult.Errors);
            }

            var currentUserId = _currentUserService.GetUserId();

            var fromAccount = await _accountRepository.GetByUserIdAsync(currentUserId);
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

        public async Task DepositAsync(DepositRequestDto request)
        {
            var userId = _currentUserService.GetUserId();

            var account = await _accountRepository.GetByUserIdAsync(userId);
            if (account == null)
            {
                throw new BadRequestException(DomainErrors.Account.AccountNotFound);
            }

            await _unitOfWork.BeginTransactionAsync();

            try
            {
                account.Balance += request.Amount;
                await _accountRepository.UpdateAsync(account);

                var transaction = new Transaction
                {
                    ToAccountId = account.Id,
                    Amount = request.Amount,
                    Type = TransactionType.Deposit,
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

        public async Task WithdrawalAsync(WithdrawalRequestDto request)
        {
            var userId = _currentUserService.GetUserId();

            var account = await _accountRepository.GetByUserIdAsync(userId);
            if (account == null)
            {
                throw new BadRequestException(DomainErrors.Account.AccountNotFound);
            }

            if (account.Balance < request.Amount)
            {
                throw new BadRequestException(DomainErrors.Transaction.InsufficientFunds);
            }

            await _unitOfWork.BeginTransactionAsync();

            try
            {
                account.Balance -= request.Amount;
                await _accountRepository.UpdateAsync(account);

                var transaction = new Transaction
                {
                    ToAccountId = account.Id,
                    Amount = request.Amount,
                    Type = TransactionType.Withdrawal,
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

        public async Task<IEnumerable<TransactionResponseDto>> GetHistoryAsync(int pageNumber, int pageSize)
        {
            var currentUserId = _currentUserService.GetUserId();

            var account = await _accountRepository.GetByUserIdAsync(currentUserId);
            if (account == null)
            {
                throw new BadRequestException(DomainErrors.Account.FromAccountNotFound);
            }

            var transactions = await _transactionRepository.GetPagedByAccountIdAsync(account.Id, pageNumber, pageSize);

            return transactions.Select(t =>
            {
                bool isSender = t.FromAccountId == account.Id;

                string amountPrefix = t.Type switch
                {
                    TransactionType.Withdrawal => "-",
                    TransactionType.Deposit => "+",
                    TransactionType.Transfer => isSender ? "-" : "+",
                    _ => ""
                };

                string counterpart = t.Type switch
                {
                    TransactionType.Transfer => isSender
                        ? t.ToAccount?.AccountNumber ?? "Unknown"
                        : t.FromAccount?.AccountNumber ?? "Unknown",
                    TransactionType.Deposit => "External Source",
                    TransactionType.Withdrawal => "External Destination",
                    _ => "N/A"
                };

                return new TransactionResponseDto
                {
                    TransactionId = t.Id,
                    Amount = $"{amountPrefix}{t.Amount}",
                    Date = t.CreatedAt,
                    Reference = t.Reference,
                    Type = t.Type.ToString(),
                    CounterpartAccount = counterpart
                };
            });
        }
    }
}
