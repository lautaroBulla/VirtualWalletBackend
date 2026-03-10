using VirtualWallet.Application.DTOs;

namespace VirtualWallet.Application.Interfaces
{
    public interface ITransactionService
    {
        Task MakeTransferAsync(TransferRequestDto dto);
        Task DepositAsync(DepositRequestDto request);
        Task<IEnumerable<TransactionResponseDto>> GetHistoryAsync(int pageNumber, int pageSize);
    }
}
