using MusicStore.Domain.common;
using System;
using System.Collections.Generic;
using System.Text;

namespace MusicStore.Domain.Entities
{
    public class ReviewReply : BaseEntity
    {
        public string Comment { get; set; } = null!;

        public int ReviewId { get; set; }

        public Review Review { get; set; } = null!;

        public string AdminId { get; set; } = null!;
    }
}
