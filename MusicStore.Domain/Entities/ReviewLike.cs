using MusicStore.Domain.common;
using System;
using System.Collections.Generic;
using System.Text;

namespace MusicStore.Domain.Entities
{
    public class ReviewLike : BaseEntity
    {
        public int ReviewId { get; set; }

        public Review Review { get; set; } = null!;

        public string UserId { get; set; } = null!;
    }
}
