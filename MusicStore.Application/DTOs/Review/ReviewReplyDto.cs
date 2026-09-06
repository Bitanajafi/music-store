namespace MusicStore.Application.DTOs.Review
{
    public class ReviewReplyDto
    {
        public int Id { get; set; }

        public int ReviewId { get; set; }

        public string Comment { get; set; } = null!;

        public string AdminFullName { get; set; } = null!;

        public DateTime CreatedAt { get; set; }

        public DateTime? UpdatedAt { get; set; }
    }
}