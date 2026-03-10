using VirtualWallet.Domain.Entities;

namespace VirtualWallet.Application.Interfaces.Repositories
{
    public interface ITransactionRepository
    {
        Task<Transaction?> GetByIdAsync(Guid id);
        Task<IEnumerable<Transaction>> GetByAccountIdAsync(Guid accountId);
        Task AddAsync(Transaction transaction);
        Task<IEnumerable<Transaction>> GetPagedByAccountIdAsync(Guid accountId, int pageNumber, int pageSize);
    }
}
