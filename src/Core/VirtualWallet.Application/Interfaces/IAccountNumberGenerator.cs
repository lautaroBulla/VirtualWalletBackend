namespace VirtualWallet.Application.Interfaces
{
    public interface IAccountNumberGenerator
    {
        Task<string> GenerateUniqueAccountNumberAsync();
    }
}
