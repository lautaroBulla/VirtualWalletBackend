using Microsoft.AspNetCore.Http;
using System.Security.Claims;
using VirtualWallet.Application.Interfaces;
using VirtualWallet.Domain.Exceptions;

namespace VirtualWallet.Infrastructure.Services
{
    public class CurrentUserService : ICurrentUserService
    {
        private readonly IHttpContextAccessor _httpContextAccessor;

        public CurrentUserService(IHttpContextAccessor httpContextAccessor)
        {
            _httpContextAccessor = httpContextAccessor;
        }

        public Guid GetUserId()
        {
            var userIdClaim = _httpContextAccessor.HttpContext?.User?.FindFirst(ClaimTypes.NameIdentifier)
                       ?? _httpContextAccessor.HttpContext?.User?.FindFirst(System.IdentityModel.Tokens.Jwt.JwtRegisteredClaimNames.Sub);

            if (userIdClaim == null || !Guid.TryParse(userIdClaim.Value, out var userId))
            {
                throw new BadRequestException(DomainErrors.User.InvalidToken);
            }

            return userId;
        }
    }
}
