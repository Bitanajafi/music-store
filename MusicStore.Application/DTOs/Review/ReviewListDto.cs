using MusicStore.Domain.Enum;


namespace MusicStore.Application.DTOs.Review
{
    public class ReviewListDto
    {
        public int Id { get; set; }

        public int ProductId { get; set; }

        public string UserId { get; set; } = null!;

        public string UserFullName { get; set; } = null!;

        public int Rating { get; set; }

        public string Comment { get; set; } = null!;

        public DateTime CreatedAt { get; set; }

        public DateTime? UpdatedAt { get; set; }

        public ReviewStatus Status { get; set; }

        public int LikeCount { get; set; }

        public bool IsLikedByCurrentUser { get; set; }

        public List<ReviewReplyDto> Replies { get; set; }= new();
    }
}