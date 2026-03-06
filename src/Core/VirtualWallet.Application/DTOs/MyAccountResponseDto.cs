namespace VirtualWallet.Application.DTOs
{
    public record MyAccountResponseDto
    {
        public string Account { get; set; } = string.Empty;
        public decimal Balance { get; set; }
    }
}
