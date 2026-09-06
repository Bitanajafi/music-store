using System.ComponentModel.DataAnnotations;

namespace MusicStore.Application.DTOs.Review
{
    public class CreateReviewReplyDto
    {
        [Required]
        public int ReviewId { get; set; }

        [Required]
        [StringLength(
            2000,
            MinimumLength = 3,
            ErrorMessage = "متن پاسخ باید بین 3 تا 2000 کاراکتر باشد.")]
        public string Comment { get; set; } = null!;
    }
}