using VirtualWallet.Application.DTOs;
using VirtualWallet.Application.Interfaces;
using VirtualWallet.Application.Interfaces.Repositories;
using VirtualWallet.Domain.Entities;
using VirtualWallet.Domain.Enums;
using VirtualWallet.Domain.Exceptions;

namespace VirtualWallet.Application.Services
{
    public class AccountService : IAccountService
    {
        private readonly ICurrentUserService _currentUserService;
        private readonly IAccountRepository _accountRepository;
        private readonly IUnitOfWork _unitOfWork;
        private readonly ITransactionRepository _transactionRepository;

        public AccountService(
            ICurrentUserService currentUserService, 
            IAccountRepository accountRepository,
            IUnitOfWork unitOfWork,
            ITransactionRepository transactionRepository)
        {
            _currentUserService = currentUserService;
            _accountRepository = accountRepository;
            _unitOfWork = unitOfWork;
            _transactionRepository = transactionRepository;
        }

        public async Task<MyAccountResponseDto> GetMyAccountAsync()
        {
            var userId = _currentUserService.GetUserId();

            var account = await _accountRepository.GetByUserIdAsync(userId);
            if (account == null)
            {
                throw new BadRequestException(DomainErrors.Account.AccountNotFound);
            }

            return new MyAccountResponseDto
            {
                Account = account.AccountNumber,
                Balance = account.Balance
            };
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
                    Amount = request.Amount,
                    Type = TransactionType.Deposit,
                    Status = TransactionStatus.Completed
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
