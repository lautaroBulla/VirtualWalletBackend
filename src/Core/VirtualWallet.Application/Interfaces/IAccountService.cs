using VirtualWallet.Application.DTOs;

namespace VirtualWallet.Application.Interfaces
{
    public interface IAccountService
    {
        Task<MyAccountResponseDto> GetMyAccountAsync();
    }
}
