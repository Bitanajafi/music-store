using MusicStore.Application.Common.Results;
using MusicStore.Application.DTOs.Review;

namespace MusicStore.Application.Interfaces
{
    public interface IReviewService
    {
        

        Task<ServiceResult<ReviewListDto>> CreateAsync(
            CreateReviewDto dto,
            string userId);

        Task<ServiceResult<List<ReviewListDto>>> GetProductReviewsAsync(
            int productId,
            string? currentUserId = null);

        Task<ServiceResult<bool>> LikeAsync(
            ReviewLikeDto dto,
            string userId);

        Task<ServiceResult<bool>> UnlikeAsync(
            ReviewLikeDto dto,
            string userId);





        

        Task<ServiceResult<List<AdminReviewListDto>>> GetAdminReviewsAsync(
            AdminReviewFilterDto filter);

        Task<ServiceResult<bool>> UpdateStatusAsync(
            UpdateReviewStatusDto dto,
            string adminId);

        Task<ServiceResult<ReviewReplyDto>> CreateReplyAsync(
            CreateReviewReplyDto dto,
            string adminId);
    }
}