using MusicStore.Application.Common.Results;
using MusicStore.Application.DTOs.Stock;

namespace MusicStore.Application.Interfaces.Services;

public interface IStockService
{
    Task<ServiceResult<bool>> IncreaseStockAsync(
        string sku,
        int quantity,
        string reason,
        string? userId);

    Task<ServiceResult<bool>> DecreaseStockAsync(
        string sku,
        int quantity,
        string reason,
        string? userId);

    Task<ServiceResult<StockHistoryDto>>
        GetHistoryByIdAsync(int historyId);

    Task<ServiceResult<IEnumerable<StockHistoryDto>>>
        GetProductHistoryAsync(string sku);

    Task<ServiceResult<IEnumerable<StockHistoryDto>>>
        GetAllHistoryAsync();
}