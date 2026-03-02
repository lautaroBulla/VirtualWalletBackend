using VirtualWallet.Domain.Entities;

namespace VirtualWallet.Application.Interfaces.Repositories
{
    public interface IAccountRepository
    {
        Task<Account?> GetByIdAsync(Guid id);
        Task<Account?> GetByUserIdAsync(Guid userId);
        Task<Account?> GetByAccountNumberAsync(string accountNumber);
        Task<bool> AccountNumberExistsAsync(string accountNumber);
        Task AddAsync(Account account);
        Task UpdateAsync(Account account);
    }
}
