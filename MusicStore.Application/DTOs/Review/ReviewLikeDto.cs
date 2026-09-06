using System;
using System.Collections.Generic;
using System.Text;


    using System.ComponentModel.DataAnnotations;

    namespace MusicStore.Application.DTOs.Review
    {
        public class ReviewLikeDto
        {
            [Required]
            public int ReviewId { get; set; }
        }
    }

