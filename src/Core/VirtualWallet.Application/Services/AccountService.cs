using VirtualWallet.Application.Interfaces;
using VirtualWallet.Domain.Exceptions;
using VirtualWallet.Application.DTOs;
using VirtualWallet.Application.Interfaces.Repositories;

namespace VirtualWallet.Application.Services
{
    public class AccountService : IAccountService
    {
        private readonly ICurrentUserService _currentUserService;
        private readonly IAccountRepository _accountRepository;

        public AccountService(ICurrentUserService currentUserService, IAccountRepository accountRepository)
        {
            _currentUserService = currentUserService;
            _accountRepository = accountRepository;
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
    }
}
