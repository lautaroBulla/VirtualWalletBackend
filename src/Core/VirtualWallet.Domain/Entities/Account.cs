using VirtualWallet.Domain.Enums;

namespace VirtualWallet.Domain.Entities
{
    public class Account
    {
        public Guid Id { get; set; } = Guid.NewGuid();
        public Guid UserId { get; set; }
        public string AccountNumber { get; set; } = string.Empty;

        public decimal Balance { get; set; } = 0m;
        public string Currency { get; set; } = nameof(CurrencyType.UYU);
        public bool IsActive { get; set; } = true;
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        public User? User { get; set; }
        public List<Transcation> SentTransactions { get; set; } = new List<Transcation>();
        public List<Transcation> ReceivedTransactions { get; set; } = new List<Transcation>();
    }
}
