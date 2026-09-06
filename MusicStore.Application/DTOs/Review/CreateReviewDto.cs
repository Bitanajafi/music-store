using System;
using System.Collections.Generic;
using System.Text;

using System.ComponentModel.DataAnnotations;

namespace MusicStore.Application.DTOs.Review
{
    public class CreateReviewDto
    {
        [Required]
        public int ProductId { get; set; }

        [Required]
        [Range(1, 5)]
        public int Rating { get; set; }

        [Required]
        [StringLength(
            2000,
            MinimumLength = 1,
            ErrorMessage = "متن نظر باید بین 1 تا 2000 کاراکتر باشد.")]
        public string Comment { get; set; } = null!;
    }
}
