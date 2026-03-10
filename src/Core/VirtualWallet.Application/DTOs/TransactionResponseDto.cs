namespace VirtualWallet.Application.DTOs
{
    public record TransactionResponseDto
    {
        public Guid TransactionId { get; set; }
        public string Amount { get; set; }
        public string Type { get; set; } = string.Empty;
        public DateTime Date { get; set; }
        public string Reference { get; set; } = string.Empty;
        public string CounterpartAccount { get; set; } = string.Empty;
    }
}
