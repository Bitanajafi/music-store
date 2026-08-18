
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using MusicStore.Application.DTOs.Stock;
using MusicStore.Application.Interfaces.Services;
using System.Security.Claims;

namespace MusicStore.Web.Controllers;

[Area("Admin")]
[Authorize(Roles = "Admin,SuperAdmin")]
public class StockController : Controller
{
    private readonly IStockService _stockService;

    public StockController(IStockService stockService)
    {
        _stockService = stockService;
    }

    


    [HttpGet]
    public async Task<IActionResult> Index(string? sku)
    {
        if (!string.IsNullOrWhiteSpace(sku))
        {
            var historyResult = await _stockService
                .GetProductHistoryAsync(sku.Trim());

            if (!historyResult.Success)
            {
                TempData["Error"] = historyResult.Message;

                return View(
                    new List<StockHistoryDto>());
            }

            return View(
                historyResult.Data ??
                new List<StockHistoryDto>());
        }

        var result = await _stockService
            .GetAllHistoryAsync();

        if (!result.Success)
        {
            TempData["Error"] = result.Message;

            return View(
                new List<StockHistoryDto>());
        }

        return View(
            result.Data ??
            new List<StockHistoryDto>());
    }



   

[HttpGet]
public async Task<IActionResult> ProductHistory(string sku)
    {
        if (string.IsNullOrWhiteSpace(sku))
        {
            TempData["Error"] =
                "کد SKU محصول الزامی است.";

            return RedirectToAction(nameof(Index));
        }

        var result = await _stockService
            .GetProductHistoryAsync(sku);

        if (!result.Success)
        {
            TempData["Error"] = result.Message;

            return RedirectToAction(nameof(Index));
        }

        return View(result.Data);
    }



    


    [HttpGet]
    public async Task<IActionResult> Details(int id)
    {
        if (id <= 0)
        {
            return NotFound();
        }

        var result = await _stockService
            .GetHistoryByIdAsync(id);

        if (!result.Success || result.Data == null)
        {
            TempData["Error"] =
                result.Message;

            return NotFound();
        }

        return View(result.Data);
    }

  



  



    [HttpGet]
    public IActionResult Increase()
    {
        return View(new ChangeStockDto());
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Increase(
        ChangeStockDto dto)
    {
        if (!ModelState.IsValid)
        {
            return View(dto);
        }

        var userId = User.FindFirstValue(
            ClaimTypes.NameIdentifier);

        var result = await _stockService
            .IncreaseStockAsync(
                dto.SKU,
                dto.Quantity,
                dto.Reason,
                userId);

        if (!result.Success)
        {
            ModelState.AddModelError(
                string.Empty,
                result.Message);

            return View(dto);
        }

        TempData["Success"] =
            result.Message;

        return RedirectToAction(
            nameof(Index));
    }






    [HttpGet]
    public IActionResult Decrease()
    {
        return View(new ChangeStockDto());
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Decrease(
        ChangeStockDto dto)
    {
        if (!ModelState.IsValid)
        {
            return View(dto);
        }

        var userId = User.FindFirstValue(
            ClaimTypes.NameIdentifier);

        var result = await _stockService
            .DecreaseStockAsync(
                dto.SKU,
                dto.Quantity,
                dto.Reason,
                userId);

        if (!result.Success)
        {
            ModelState.AddModelError(
                string.Empty,
                result.Message);

            return View(dto);
        }

        TempData["Success"] =
            result.Message;

        return RedirectToAction(
            nameof(Index));
    }
}

