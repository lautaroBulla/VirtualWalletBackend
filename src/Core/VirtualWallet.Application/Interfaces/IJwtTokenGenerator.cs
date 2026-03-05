using VirtualWallet.Domain.Entities;

namespace VirtualWallet.Application.Interfaces
{
    public interface IJwtTokenGenerator
    {
        string GenerateToken(User user);
    }
}
