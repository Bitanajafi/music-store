using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using MusicStore.Application.DTOs.Review;
using MusicStore.Application.Interfaces;
using System.Security.Claims;

namespace MyStoreCore.Controllers
{
    public class ReviewController : Controller
    {
        private readonly IReviewService _reviewService;

        public ReviewController(IReviewService reviewService)
        {
            _reviewService = reviewService;
        }







        [HttpPost]
        [Authorize]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(CreateReviewDto dto)
        {
            if (dto == null || !ModelState.IsValid)
            {
                return BadRequest(new
                {
                    success = false,
                    message = "اطلاعات دیدگاه نامعتبر است."
                });
            }

            var userId = User.FindFirst(
                ClaimTypes.NameIdentifier)?.Value;

            if (string.IsNullOrWhiteSpace(userId))
            {
                return Unauthorized(new
                {
                    success = false,
                    message = "کاربر شناسایی نشد."
                });
            }

            var result = await _reviewService.CreateAsync(
                dto,
                userId);

            if (!result.Success)
            {
                return BadRequest(new
                {
                    success = false,
                    message = result.Message
                });
            }

            return Ok(new
            {
                success = true,
                message = result.Message,
                data = result.Data
            });
        }




        [HttpPost]
        [Authorize]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Like(
            ReviewLikeDto dto)
        {
            if (dto == null || dto.ReviewId <= 0)
            {
                return BadRequest(new
                {
                    success = false,
                    message = "شناسه دیدگاه نامعتبر است."
                });
            }

            var userId = User.FindFirst(
                ClaimTypes.NameIdentifier)?.Value;

            if (string.IsNullOrWhiteSpace(userId))
            {
                return Unauthorized(new
                {
                    success = false,
                    message = "کاربر شناسایی نشد."
                });
            }

            var result = await _reviewService.LikeAsync(
                dto,
                userId);

            if (!result.Success)
            {
                return BadRequest(new
                {
                    success = false,
                    message = result.Message
                });
            }

            return Ok(new
            {
                success = true,
                message = result.Message
            });
        }







        [HttpPost]
        [Authorize]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Unlike(
       ReviewLikeDto dto)
        {
            if (dto == null || dto.ReviewId <= 0)
            {
                return BadRequest(new
                {
                    success = false,
                    message = "شناسه دیدگاه نامعتبر است."
                });
            }

            var userId = User.FindFirst(
                ClaimTypes.NameIdentifier)?.Value;

            if (string.IsNullOrWhiteSpace(userId))
            {
                return Unauthorized(new
                {
                    success = false,
                    message = "کاربر شناسایی نشد."
                });
            }

            var result = await _reviewService.UnlikeAsync(
                dto,
                userId);

            if (!result.Success)
            {
                return BadRequest(new
                {
                    success = false,
                    message = result.Message
                });
            }

            return Ok(new
            {
                success = true,
                message = result.Message
            });
        }
    }
}