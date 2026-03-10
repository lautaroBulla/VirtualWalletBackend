using Microsoft.EntityFrameworkCore;
using VirtualWallet.Application.Interfaces.Repositories;
using VirtualWallet.Domain.Entities;

namespace VirtualWallet.Infrastructure.Persistence.Repositories
{
    public class TransactionRepository : ITransactionRepository
    {
        private readonly AppDbContext _context;

        public TransactionRepository(AppDbContext context)
        {
            _context = context;
        }

        public async Task<Transaction?> GetByIdAsync(Guid id)
        {
            return await _context.Transactions.FindAsync(id);
        }

        public async Task<IEnumerable<Transaction>> GetByAccountIdAsync(Guid accountId)
        {
            return await _context.Transactions
                .Where(t => t.FromAccountId == accountId || t.ToAccountId == accountId)
                .OrderByDescending(t => t.CreatedAt)
                .ToListAsync();
        }

        public async Task AddAsync(Transaction transaction)
        {
            await _context.Transactions.AddAsync(transaction);
            await _context.SaveChangesAsync();
        }

        public async Task<IEnumerable<Transaction>> GetPagedByAccountIdAsync(Guid accountId, int pageNumber, int pageSize)
        {
            return await _context.Transactions
                .Include(t => t.FromAccount)
                .Include(t => t.ToAccount)
                .Where(t => t.FromAccountId == accountId || t.ToAccountId == accountId)
                .OrderByDescending(t => t.CreatedAt)
                .Skip((pageNumber - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync();
        }
    }
}
