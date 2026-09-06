using MusicStore.Domain.Enum;
using MusicStore.Domain.Enums;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Text;

namespace MusicStore.Application.DTOs.Review
{
    public class UpdateReviewStatusDto
    {
        [Required]
        public int ReviewId { get; set; }

        [Required]
        public ReviewStatus Status { get; set; }
    }
}
 