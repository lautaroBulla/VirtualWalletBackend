using VirtualWallet.Domain.Enums;

namespace VirtualWallet.Domain.Entities
{
    public class Transcation
    {
        public Guid Id { get; set; } = Guid.NewGuid();

        public Guid? FromAccountId { get; set; }
        public Guid? ToAccountId { get; set; }

        public decimal Amount { get; set; }
        public TransactionType TransactionType { get; set; }
        public TransactionStatus Status { get; set; } = TransactionStatus.Pending;
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        public string? Reference { get; set; }

        public Account? FromAccount { get; set; }
        public Account? ToAccount { get; set; }
    }
}
