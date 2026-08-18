using AutoMapper;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using MusicStore.Application.Common.Results;
using MusicStore.Application.DTOs.Stock;
using MusicStore.Application.Interfaces.Generic;
using MusicStore.Application.Interfaces.Services;
using MusicStore.Domain.Entities;
using MusicStore.Infrastructure.Identity;
using MusicStore.Infrastructure.Repository;

namespace MusicStore.Infrastructure.Services;

public class StockService : IStockService
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly IMapper _mapper;
    private readonly UserManager<ApplicationUser> _userManager;

    public StockService(
        IUnitOfWork unitOfWork,
        IMapper mapper,
        UserManager<ApplicationUser> userManager)
    {
        _unitOfWork = unitOfWork;
        _mapper = mapper;
        _userManager = userManager;
    }

    public async Task<ServiceResult<bool>> DecreaseStockAsync(
        string sku,
        int quantity,
        string reason,
        string? userId)
    {
        if (string.IsNullOrWhiteSpace(sku))
        {
            return ServiceResult<bool>
                .Fail("کد SKU محصول الزامی است.");
        }

        if (quantity <= 0)
        {
            return ServiceResult<bool>
                .Fail("تعداد باید بیشتر از صفر باشد.");
        }

        if (string.IsNullOrWhiteSpace(reason))
        {
            return ServiceResult<bool>
                .Fail("دلیل کاهش موجودی الزامی است.");
        }

        var product = await _unitOfWork
            .Repository<Product>()
            .GetFirstOrDefaultAsync(
                x => x.SKU == sku);

        if (product == null)
        {
            return ServiceResult<bool>
                .Fail("محصولی با این کد SKU یافت نشد.");
        }

        if (product.StockQuantity < quantity)
        {
            return ServiceResult<bool>
                .Fail(
                    $"موجودی محصول {product.Name} برای این مقدار کافی نیست.");
        }

        product.StockQuantity -= quantity;

        await _unitOfWork
            .Repository<Product>()
            .UpdateAsync(product);

        var stockHistory = new StockHistory
        {
            ProductId = product.Id,
            Quantity = -quantity,
            Reason = reason,
            CreatedBy = userId,
            CreatedAt = DateTime.UtcNow
        };

        await _unitOfWork
            .Repository<StockHistory>()
            .AddAsync(stockHistory);

        await _unitOfWork.SaveAsync();

        return ServiceResult<bool>.Ok(
            true,
            "موجودی محصول با موفقیت کاهش یافت.");
    }

    public async Task<ServiceResult<bool>> IncreaseStockAsync(
        string sku,
        int quantity,
        string reason,
        string? userId)
    {
        if (string.IsNullOrWhiteSpace(sku))
        {
            return ServiceResult<bool>
                .Fail("کد SKU محصول الزامی است.");
        }

        if (quantity <= 0)
        {
            return ServiceResult<bool>
                .Fail("تعداد باید بیشتر از صفر باشد.");
        }

        if (string.IsNullOrWhiteSpace(reason))
        {
            return ServiceResult<bool>
                .Fail("دلیل افزایش موجودی الزامی است.");
        }

        var product = await _unitOfWork
            .Repository<Product>()
            .GetFirstOrDefaultAsync(
                x => x.SKU == sku);

        if (product == null)
        {
            return ServiceResult<bool>
                .Fail("محصولی با این کد SKU یافت نشد.");
        }

        product.StockQuantity += quantity;

        await _unitOfWork
            .Repository<Product>()
            .UpdateAsync(product);

        var stockHistory = new StockHistory
        {
            ProductId = product.Id,
            Quantity = quantity,
            Reason = reason,
            CreatedBy = userId,
            CreatedAt = DateTime.UtcNow
        };

        await _unitOfWork
            .Repository<StockHistory>()
            .AddAsync(stockHistory);

        await _unitOfWork.SaveAsync();

        return ServiceResult<bool>.Ok(
            true,
            "موجودی محصول با موفقیت افزایش یافت.");
    }

    public async Task<ServiceResult<StockHistoryDto>> GetHistoryByIdAsync(
        int historyId)
    {
        var history = await _unitOfWork
            .Repository<StockHistory>()
            .GetFirstOrDefaultAsync(
                x => x.Id == historyId,
                query => query
                    .Include(x => x.Product));

        if (history == null)
        {
            return ServiceResult<StockHistoryDto>
                .Fail("رکورد تاریخچه موجودی یافت نشد.");
        }

        var result = _mapper.Map<StockHistoryDto>(history);

        result.CreatedByName = await GetUserNameAsync(
            history.CreatedBy);

        return ServiceResult<StockHistoryDto>
            .Ok(result);
    }

    public async Task<ServiceResult<IEnumerable<StockHistoryDto>>>
        GetProductHistoryAsync(string sku)
    {
        if (string.IsNullOrWhiteSpace(sku))
        {
            return ServiceResult<IEnumerable<StockHistoryDto>>
                .Fail("کد SKU محصول الزامی است.");
        }

        var product = await _unitOfWork
            .Repository<Product>()
            .GetFirstOrDefaultAsync(
                x => x.SKU == sku);

        if (product == null)
        {
            return ServiceResult<IEnumerable<StockHistoryDto>>
                .Fail(
                    "محصولی با این کد SKU یافت نشد.");
        }

        var histories = await _unitOfWork
            .Repository<StockHistory>()
            .GetAllAsync(
                x => x.ProductId == product.Id,
                query => query
                    .Include(x => x.Product));

        var result = new List<StockHistoryDto>();

        foreach (var history in histories
            .OrderByDescending(x => x.CreatedAt))
        {
            var dto = _mapper.Map<StockHistoryDto>(history);

            dto.CreatedByName = await GetUserNameAsync(
                history.CreatedBy);

            result.Add(dto);
        }

        return ServiceResult<IEnumerable<StockHistoryDto>>
            .Ok(result);
    }

    public async Task<ServiceResult<IEnumerable<StockHistoryDto>>>
        GetAllHistoryAsync()
    {
        var histories = await _unitOfWork
            .Repository<StockHistory>()
            .GetAllAsync(
                null,
                query => query
                    .Include(x => x.Product));

        var result = new List<StockHistoryDto>();

        foreach (var history in histories
            .OrderByDescending(x => x.CreatedAt))
        {
            var dto = _mapper.Map<StockHistoryDto>(history);

            dto.CreatedByName = await GetUserNameAsync(
                history.CreatedBy);

            result.Add(dto);
        }

        return ServiceResult<IEnumerable<StockHistoryDto>>
            .Ok(result);
    }

    private async Task<string> GetUserNameAsync(
        string? userId)
    {
        if (string.IsNullOrWhiteSpace(userId))
        {
            return "سیستم";
        }

        var user = await _userManager.FindByIdAsync(userId);

        if (user == null)
        {
            return "کاربر حذف شده";
        }

        var fullName =
            $"{user.FirstName} {user.LastName}".Trim();

        return string.IsNullOrWhiteSpace(fullName)
            ? "کاربر بدون نام"
            : fullName;
    }
}