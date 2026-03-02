using VirtualWallet.Application.DTOs;

namespace VirtualWallet.Application.Interfaces
{
    public interface IAuthService
    {
        Task<UserResponseDto> RegisterAsync(RegisterUserDto dto);
    }
}
