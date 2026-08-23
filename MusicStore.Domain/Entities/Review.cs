using MusicStore.Domain.common;
using MusicStore.Domain.Enum;
using System;
using System.Collections.Generic;
using System.Text;

namespace MusicStore.Domain.Entities
{
    public class Review : BaseEntity
    {
        public int Rating { get; set; }

        public string Comment { get; set; } = null!;

        public ReviewStatus Status { get; set; } = ReviewStatus.Pending;

        public string UserId { get; set; } = null!;

        public int ProductId { get; set; }

        public Product Product { get; set; } = null!;

        public ICollection<ReviewReply> Replies { get; set; }
            = new List<ReviewReply>();

        public ICollection<ReviewLike> Likes { get; set; }
            = new List<ReviewLike>();
    }
}
