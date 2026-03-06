using System;
using System.Collections.Generic;
using System.Text;

namespace VirtualWallet.Application.DTOs
{
    public record TransferRequestDto
    {
        public string ToAccountNumber { get; set; } = string.Empty;
        public decimal Amount { get; set; }
        public string? Reference { get; set; }
    }
}
