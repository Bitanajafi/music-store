using MusicStore.Domain.Enums;
using MusicStore.Domain.Enum;
using System;
using System.Collections.Generic;
using System.Text;

namespace MusicStore.Application.DTOs.Review
{
    public class AdminReviewListDto
    {
        public int Id { get; set; }


        public int ProductId { get; set; }

        public string ProductName { get; set; } = null!;

        public string UserId { get; set; } = null!;

        public string UserFullName { get; set; } = null!;

        public int Rating { get; set; }

        public string Comment { get; set; } = null!;

        public ReviewStatus Status { get; set; }

        public int LikeCount { get; set; }

        public int ReplyCount { get; set; }

        public DateTime CreatedAt { get; set; }

        public DateTime? UpdatedAt { get; set; }
    }
}