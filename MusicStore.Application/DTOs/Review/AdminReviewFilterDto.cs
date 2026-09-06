using MusicStore.Domain.Enum;
using MusicStore.Domain.Enums;

namespace MusicStore.Application.DTOs.Review
{
    public class AdminReviewFilterDto
    {
        public string? Search { get; set; }

        public int? ProductId { get; set; }

        public ReviewStatus? Status { get; set; }

        public int? Rating { get; set; }

        public DateTime? FromDate { get; set; }

        public DateTime? ToDate { get; set; }

        public ReviewSortBy SortBy { get; set; } = ReviewSortBy.Newest;
    }
} 