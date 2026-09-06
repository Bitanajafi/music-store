using Humanizer;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using MusicStore.Application.Common.Results;
using MusicStore.Application.DTOs.Review;
using MusicStore.Application.Interfaces;
using MusicStore.Application.Interfaces.Generic;
using MusicStore.Domain.Entities;
using MusicStore.Domain.Enum;
using MusicStore.Domain.Enums;
using MusicStore.Infrastructure.Identity;
using System;
using System.Collections.Generic;
using System.Text;

namespace MusicStore.Infrastructure.Services
{
    public class ReviewService : IReviewService
    {


        private readonly IUnitOfWork _unitOfWork;
        private readonly UserManager<ApplicationUser> _userManager;


        private readonly IGenericRepository<Review> _reviewRepository;
        private readonly IGenericRepository<ReviewReply> _replyRepository;
        private readonly IGenericRepository<ReviewLike> _likeRepository;
        private readonly IGenericRepository<Product> _productRepository;


        public ReviewService(IUnitOfWork unitOfWork,UserManager<ApplicationUser> userManager)
        {
            _unitOfWork = unitOfWork;
            _userManager = userManager;

            _reviewRepository = _unitOfWork.Repository<Review>();
            _replyRepository = _unitOfWork.Repository<ReviewReply>();
            _likeRepository = _unitOfWork.Repository<ReviewLike>();
            _productRepository = _unitOfWork.Repository<Product>();
        }





        public async Task<ServiceResult<ReviewListDto>> CreateAsync(
            CreateReviewDto dto,
            string userId)
        {
            if (string.IsNullOrWhiteSpace(userId))
            {
                return ServiceResult<ReviewListDto>.Fail(
                    "کاربر شناسایی نشد.");
            }

            if (dto == null)
            {
                return ServiceResult<ReviewListDto>.Fail(
                    "اطلاعات دیدگاه نامعتبر است.");
            }

            if (dto.ProductId <= 0)
            {
                return ServiceResult<ReviewListDto>.Fail(
                    "محصول نامعتبر است.");
            }

            if (dto.Rating < 1 || dto.Rating > 5)
            {
                return ServiceResult<ReviewListDto>.Fail(
                    "امتیاز باید بین ۱ تا ۵ باشد.");
            }

            if (string.IsNullOrWhiteSpace(dto.Comment))
            {
                return ServiceResult<ReviewListDto>.Fail(
                    "متن دیدگاه نمی‌تواند خالی باشد.");
            }

            var product =
                await _productRepository.GetByIdAsync(dto.ProductId);

            if (product == null || product.IsDeleted)
            {
                return ServiceResult<ReviewListDto>.Fail(
                    "محصول موردنظر پیدا نشد.");
            }

            var user =
                await _userManager.FindByIdAsync(userId);

            if (user == null || !user.IsActive)
            {
                return ServiceResult<ReviewListDto>.Fail(
                    "کاربر موردنظر پیدا نشد.");
            }

            var review = new Review
            {
                ProductId = dto.ProductId,
                UserId = userId,
                Rating = dto.Rating,
                Comment = dto.Comment.Trim(),
                Status = ReviewStatus.Pending
            };

            await _reviewRepository.AddAsync(review);

            await _unitOfWork.SaveAsync();

            var fullName =$"{user.FirstName} {user.LastName}".Trim();

            if (string.IsNullOrWhiteSpace(fullName))
            {
                fullName = "کاربر";
            }

            var result = new ReviewListDto
            {
                Id = review.Id,
                ProductId = review.ProductId,
                UserId = review.UserId,

                UserFullName = fullName,

                Rating = review.Rating,
                Comment = review.Comment,

                CreatedAt = review.CreatedAt,
                UpdatedAt = review.UpdatedAt,

                Status = review.Status,

                LikeCount = 0,
                IsLikedByCurrentUser = false,

                Replies = new List<ReviewReplyDto>()
            };

            return ServiceResult<ReviewListDto>.Ok(
                result,"دیدگاه شما ثبت شد و پس از بررسی نمایش داده خواهد شد.");
        }






        public async Task<ServiceResult<List<ReviewListDto>>> GetProductReviewsAsync(
            int productId,
            string? currentUserId = null)
        {
            if (productId <= 0)
            {
                return ServiceResult<List<ReviewListDto>>.Fail(
                    "شناسه محصول نامعتبر است.");
            }

            var product =
                await _productRepository.GetByIdAsync(productId);

            if (product == null || product.IsDeleted)
            {
                return ServiceResult<List<ReviewListDto>>.Fail(
                    "محصول موردنظر پیدا نشد.");
            }

            var reviews = await _reviewRepository.GetAllAsync(
                x =>
                    x.ProductId == productId &&
                    x.Status == ReviewStatus.Approved &&
                    !x.IsDeleted,

                query => query
                    .Include(x => x.Replies)
                    .Include(x => x.Likes)
            );

            var userIds = reviews
                .Select(x => x.UserId)
                .Distinct()
                .ToList();

            var users = await _userManager.Users
                .Where(x => userIds.Contains(x.Id))
                .ToListAsync();

            var result = reviews
                .OrderByDescending(x => x.CreatedAt)
                .Select(review =>
                {
                    var user = users.FirstOrDefault(
                        x => x.Id == review.UserId);

                    var fullName = user == null
                        ? "کاربر"
                        : $"{user.FirstName} {user.LastName}".Trim();

                    if (string.IsNullOrWhiteSpace(fullName))
                    {
                        fullName = "کاربر";
                    }

                    return new ReviewListDto
                    {
                        Id = review.Id,
                        ProductId = review.ProductId,
                        UserId = review.UserId,

                        UserFullName = fullName,

                        Rating = review.Rating,
                        Comment = review.Comment,

                        CreatedAt = review.CreatedAt,
                        UpdatedAt = review.UpdatedAt,

                        Status = review.Status,

                        LikeCount = review.Likes.Count(
                            x => !x.IsDeleted),

                        IsLikedByCurrentUser =
                            !string.IsNullOrWhiteSpace(currentUserId) &&
                            review.Likes.Any(x =>
                                x.UserId == currentUserId &&
                                !x.IsDeleted),

                        Replies = review.Replies
                            .Where(x => !x.IsDeleted)
                            .OrderBy(x => x.CreatedAt)
                            .Select(reply => new ReviewReplyDto
                            {
                                Id = reply.Id,
                                ReviewId = reply.ReviewId,
                                Comment = reply.Comment,
                                CreatedAt = reply.CreatedAt,
                                UpdatedAt = reply.UpdatedAt
                            })
                            .ToList()
                    };
                })
                .ToList();

            return ServiceResult<List<ReviewListDto>>.Ok(result);
        }





        public async Task<ServiceResult<bool>> LikeAsync(ReviewLikeDto dto, string userId)
        {
            if (string.IsNullOrWhiteSpace(userId))
            {
                return ServiceResult<bool>.Fail("کاربر شناسایی نشد.");
            }

            if (dto == null || dto.ReviewId <= 0)
            {
                return ServiceResult<bool>.Fail("شناسه دیدگاه نامعتبر است.");
            }

            var review = await _reviewRepository.GetByIdAsync(dto.ReviewId);

            if (review == null || review.IsDeleted)
            {
                return ServiceResult<bool>.Fail( "دیدگاه موردنظر پیدا نشد.");
            }

            if (review.Status != ReviewStatus.Approved)
            {
                return ServiceResult<bool>.Fail("این دیدگاه قابل پسندیدن نیست.");
            }

            var existingLike = await _likeRepository.GetFirstOrDefaultAsync(x =>
                    x.ReviewId == dto.ReviewId &&
                    x.UserId == userId &&
                    !x.IsDeleted);

            if (existingLike != null)
            {
                return ServiceResult<bool>.Fail("شما قبلاً این دیدگاه را پسندیده‌اید.");
            }

            var like = new ReviewLike
            {
                ReviewId = dto.ReviewId,
                UserId = userId
            };

            await _likeRepository.AddAsync(like);

            await _unitOfWork.SaveAsync();

            return ServiceResult<bool>.Ok(
                true,"دیدگاه پسندیده شد.");
        }



        public async Task<ServiceResult<bool>> UnlikeAsync( ReviewLikeDto dto,string userId)
        {
            if (string.IsNullOrWhiteSpace(userId))
            {
                return ServiceResult<bool>.Fail("کاربر شناسایی نشد.");
            }

            if (dto == null || dto.ReviewId <= 0)
            {
                return ServiceResult<bool>.Fail("شناسه دیدگاه نامعتبر است.");
            }

            var review = await _reviewRepository.GetByIdAsync(dto.ReviewId);

            if (review == null || review.IsDeleted)
            {
                return ServiceResult<bool>.Fail("دیدگاه موردنظر پیدا نشد.");
            }

            var like = await _likeRepository.GetFirstOrDefaultAsync(x =>
                    x.ReviewId == dto.ReviewId &&
                    x.UserId == userId &&
                    !x.IsDeleted);

            if (like == null)
            {
                return ServiceResult<bool>.Fail( "شما این دیدگاه را نپسندیده‌اید.");
            }

            like.IsDeleted = true;
            like.DeletedAt = DateTime.UtcNow;

            await _likeRepository.UpdateAsync(like);

            await _unitOfWork.SaveAsync();

            return ServiceResult<bool>.Ok(true, "پسندیدن دیدگاه لغو شد.");
        }








        public async Task<ServiceResult<ReviewReplyDto>> CreateReplyAsync( CreateReviewReplyDto dto, string adminId)
        {
            if (string.IsNullOrWhiteSpace(adminId))
            {
                return ServiceResult<ReviewReplyDto>.Fail("ادمین شناسایی نشد.");
            }

            if (dto == null || dto.ReviewId <= 0)
            {
                return ServiceResult<ReviewReplyDto>.Fail( "شناسه دیدگاه نامعتبر است.");
            }

            if (string.IsNullOrWhiteSpace(dto.Comment))
            {
                return ServiceResult<ReviewReplyDto>.Fail( "متن پاسخ نمی‌تواند خالی باشد.");
            }

            var review = await _reviewRepository.GetByIdAsync(dto.ReviewId);

            if (review == null || review.IsDeleted)
            {
                return ServiceResult<ReviewReplyDto>.Fail("دیدگاه موردنظر پیدا نشد.");
            }

            if (review.Status != ReviewStatus.Approved)
            {
                return ServiceResult<ReviewReplyDto>.Fail( "برای دیدگاه تأییدنشده امکان ثبت پاسخ وجود ندارد.");
            }

            var reply = new ReviewReply
            {
                ReviewId = review.Id,
                AdminId = adminId,
                Comment = dto.Comment.Trim()
            };

            await _replyRepository.AddAsync(reply);

            await _unitOfWork.SaveAsync();

            var result = new ReviewReplyDto
            {
                Id = reply.Id,
                ReviewId = reply.ReviewId,
                Comment = reply.Comment,
                CreatedAt = reply.CreatedAt,
                UpdatedAt = reply.UpdatedAt
            };

            return ServiceResult<ReviewReplyDto>.Ok(
                result,"پاسخ شما با موفقیت ثبت شد.");
        }





        public async Task<ServiceResult<List<AdminReviewListDto>>> GetAdminReviewsAsync(AdminReviewFilterDto filter)
        {
            filter ??= new AdminReviewFilterDto();

            var reviews = await _reviewRepository.GetAllAsync(
                x => !x.IsDeleted,
                query => query
                    .Include(x => x.Product)
                    .Include(x => x.Likes)
                    .Include(x => x.Replies)
            );

            var userIds = reviews
                .Select(x => x.UserId)
                .Distinct()
                .ToList();

            var users = await _userManager.Users
                .Where(x => userIds.Contains(x.Id))
                .ToListAsync();

            var query = reviews.AsEnumerable();




            if (!string.IsNullOrWhiteSpace(filter.Search))
            {
                var search = filter.Search.Trim();

                query = query.Where(x =>
                {
                    var user = users.FirstOrDefault(
                        u => u.Id == x.UserId);

                    var fullName = user == null
                        ? string.Empty
                        : $"{user.FirstName} {user.LastName}";

                    return
                        fullName.Contains(
                            search,
                            StringComparison.OrdinalIgnoreCase) ||

                        (user?.FirstName?.Contains(
                            search,
                            StringComparison.OrdinalIgnoreCase) ?? false) ||

                        (user?.LastName?.Contains(
                            search,
                            StringComparison.OrdinalIgnoreCase) ?? false) ||

                        (user?.Email?.Contains(
                            search,
                            StringComparison.OrdinalIgnoreCase) ?? false) ||

                        x.Comment.Contains(
                            search,
                            StringComparison.OrdinalIgnoreCase) ||

                        x.Product.Name.Contains(
                            search,
                            StringComparison.OrdinalIgnoreCase);
                });
            }

            if (filter.ProductId.HasValue &&
                filter.ProductId.Value > 0)
            {
                query = query.Where(
                    x => x.ProductId == filter.ProductId.Value);
            }

            if (filter.Status.HasValue)
            {
                query = query.Where(
                    x => x.Status == filter.Status.Value);
            }

            if (filter.Rating.HasValue)
            {
                query = query.Where(
                    x => x.Rating == filter.Rating.Value);
            }

            if (filter.FromDate.HasValue)
            {
                var fromDate = filter.FromDate.Value.Date;

                query = query.Where(
                    x => x.CreatedAt >= fromDate);
            }

            if (filter.ToDate.HasValue)
            {
                var toDate = filter.ToDate.Value.Date.AddDays(1);

                query = query.Where(
                    x => x.CreatedAt < toDate);
            }

            query = filter.SortBy switch
            {
                ReviewSortBy.Oldest =>
                    query.OrderBy(x => x.CreatedAt),

                ReviewSortBy.HighestRating =>
                    query
                        .OrderByDescending(x => x.Rating)
                        .ThenByDescending(x => x.CreatedAt),

                ReviewSortBy.LowestRating =>
                    query
                        .OrderBy(x => x.Rating)
                        .ThenByDescending(x => x.CreatedAt),

                ReviewSortBy.MostLiked =>
                    query
                        .OrderByDescending(
                            x => x.Likes.Count(l => !l.IsDeleted))
                        .ThenByDescending(x => x.CreatedAt),

                _ =>
                    query.OrderByDescending(x => x.CreatedAt)
            };

            var result = query
                .Select(review =>
                {
                    var user = users.FirstOrDefault(
                        x => x.Id == review.UserId);

                    var fullName = user == null
                        ? "کاربر"
                        : $"{user.FirstName} {user.LastName}".Trim();

                    return new AdminReviewListDto
                    {
                        Id = review.Id,
                        ProductId = review.ProductId,
                        ProductName = review.Product.Name,
                        UserId = review.UserId,
                        UserFullName = string.IsNullOrWhiteSpace(fullName)
                            ? "کاربر"
                            : fullName,
                        Rating = review.Rating,
                        Comment = review.Comment,
                        Status = review.Status,
                        LikeCount = review.Likes.Count(
                            x => !x.IsDeleted),
                        ReplyCount = review.Replies.Count(
                            x => !x.IsDeleted),
                        CreatedAt = review.CreatedAt,
                        UpdatedAt = review.UpdatedAt
                    };
                })
                .ToList();

            return ServiceResult<List<AdminReviewListDto>>.Ok(result);
        }





        public async Task<ServiceResult<bool>> UpdateStatusAsync(UpdateReviewStatusDto dto,string adminId)
        {
            if (string.IsNullOrWhiteSpace(adminId))
            {
                return ServiceResult<bool>.Fail("ادمین شناسایی نشد.");
            }

            if (dto == null || dto.ReviewId <= 0)
            {
                return ServiceResult<bool>.Fail("شناسه دیدگاه نامعتبر است.");
            }

            if (dto.Status != ReviewStatus.Approved &&
                dto.Status != ReviewStatus.Rejected)
            {
                return ServiceResult<bool>.Fail( "وضعیت انتخاب‌شده نامعتبر است.");
            }

            var review = await _reviewRepository.GetByIdAsync(dto.ReviewId);

            if (review == null || review.IsDeleted)
            {
                return ServiceResult<bool>.Fail("دیدگاه موردنظر پیدا نشد.");
            }

            review.Status = dto.Status;
            review.UpdatedAt = DateTime.UtcNow;

            await _reviewRepository.UpdateAsync(review);
            await _unitOfWork.SaveAsync();

            var message = dto.Status == ReviewStatus.Approved
                ? "دیدگاه با موفقیت تأیید شد."
                : "دیدگاه با موفقیت رد شد.";

            return ServiceResult<bool>.Ok(true, message);
        }
    }
}
