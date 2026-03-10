using System;
using System.Collections.Generic;
using System.Text;

namespace VirtualWallet.Application.DTOs
{
    public record DepositRequestDto
    {
        public decimal Amount { get; set; }
        public string? Reference { get; set; }
    }
}
