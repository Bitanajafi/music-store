using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using MusicStore.Application.Interfaces.Services;
using System.Security.Claims;

namespace MyStoreCore.Controllers
{
    [Authorize]
    public class PaymentController : Controller
    {
        private readonly IPaymentService _paymentService;

        public PaymentController(IPaymentService paymentService)
        {
            _paymentService = paymentService;
        }

        [HttpGet]
        public IActionResult Index(string transactionId)
        {
            if (string.IsNullOrWhiteSpace(transactionId))
            {
                return BadRequest();
            }

            ViewBag.TransactionId = transactionId;

            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Verify(
        string transactionId,
        bool success)
        {
            var userId = User.FindFirstValue(
                ClaimTypes.NameIdentifier);

            if (string.IsNullOrWhiteSpace(userId))
            {
                return Unauthorized();
            }

            var result = await _paymentService.VerifyPaymentAsync(
                transactionId,
                userId,
                success);


            if (!success)
            {
                TempData["Error"] = result.Message;

                return RedirectToAction(
                    "MyOrders",
                    "Orders");
            }

            if (!result.Success)
            {
                TempData["Error"] = result.Message;

                return RedirectToAction(
                    "MyOrders",
                    "Orders");
            }

            TempData["Success"] = result.Message;

            return RedirectToAction(
                "Details",
                "Orders",
                new
                {
                    id = result.Data!.OrderId
                });
        }
    }
}