using System;
using System.Collections.Generic;
using System.Text;

namespace VirtualWallet.Application.DTOs
{
    public record RegisterResponseDto
    {
        public Guid Id { get; set; }
        public string FirstName { get; set; } = string.Empty;
        public string LastName { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
        public string AccountNumber { get; set; } = string.Empty;
    }
}
