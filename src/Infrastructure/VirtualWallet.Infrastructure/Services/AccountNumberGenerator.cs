using VirtualWallet.Application.Interfaces;
using VirtualWallet.Application.Interfaces.Repositories;

namespace VirtualWallet.Infrastructure.Services
{
    public class AccountNumberGenerator : IAccountNumberGenerator
    {
        private readonly IAccountRepository _accountRepository;

        public AccountNumberGenerator(IAccountRepository accountRepository)
        {
            _accountRepository = accountRepository;
        }

        public async Task<string> GenerateUniqueAccountNumberAsync()
        {
            string newAccountNumber;
            bool exists;
            var random = new Random();

            do
            {
                newAccountNumber = string.Join("", Enumerable.Range(0, 20).Select(_ => random.Next(0, 10)));

                exists = await _accountRepository.AccountNumberExistsAsync(newAccountNumber);

            } while (exists);

            return newAccountNumber;
        }
    }
}
