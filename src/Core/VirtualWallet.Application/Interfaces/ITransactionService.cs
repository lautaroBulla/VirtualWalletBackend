using VirtualWallet.Application.DTOs;

namespace VirtualWallet.Application.Interfaces
{
    public interface ITransactionService
    {
        Task MakeTransferAsync(TransferRequestDto dto);
    }
}
