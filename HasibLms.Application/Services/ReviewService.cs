using HasibLms.Domain.Interfaces;
using HasibLms.Domain.Entities.AcademyEntities;
using HasibLms.Shared.DTOs;
using HasibLms.Shared.DTOs.Auth;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Logging;

namespace HasibLms.Application.Services;

public class ReviewService : IReviewService
{
	private readonly IGenericRepository<CourseReview> _reviewRepository;
	private readonly ICourseRepository _courseRepository;
	private readonly ICourseService _courseService;
	private readonly IMemoryCache _cache;
	private readonly ILogger<ReviewService> _logger;

	public ReviewService(
		IGenericRepository<CourseReview> reviewRepository,
		ICourseRepository courseRepository,
		ICourseService courseService,
		IMemoryCache cache,
		ILogger<ReviewService> logger)
	{
		_reviewRepository = reviewRepository;
		_courseRepository = courseRepository;
		_courseService = courseService;
		_cache = cache;
		_logger = logger;
	}

	public async Task<AuthResultDto> AddReviewAsync(Guid userId, Guid courseId, int rating, string? comment, CancellationToken cancellationToken = default)
	{
		try
		{
			// Validate rating
			if (rating < 1 || rating > 5)
			{
				return new AuthResultDto
				{
					Succeeded = false,
					Message = "امتیاز باید بین 1 تا 5 باشد"
				};
			}

			// Check if course exists
			var course = await _courseRepository.GetByIdAsync(courseId, cancellationToken);
			if (course == null)
			{
				return new AuthResultDto
				{
					Succeeded = false,
					Message = "دوره یافت نشد"
				};
			}

			// Check if user has already reviewed
			var existingReview = await _reviewRepository.GetSingleAsync(r =>
				r.CourseId == courseId && r.StudentId == userId && !r.IsDeleted, cancellationToken);

			if (existingReview != null)
			{
				return new AuthResultDto
				{
					Succeeded = false,
					Message = "شما قبلاً برای این دوره نظر ثبت کرده‌اید"
				};
			}

			// Create review
			var review = new CourseReview
			{
				Id = Guid.NewGuid(),
				CourseId = courseId,
				StudentId = userId,
				Rating = rating,
				Comment = comment,
				CreatedAt = DateTime.UtcNow,
				IsApproved = false, // نیاز به تأیید ادمین
				CreatedDate = DateTime.UtcNow,
				LastModifiedDate = DateTime.UtcNow
			};

			await _reviewRepository.AddAsync(review, cancellationToken);

			// Clear caches
			await _courseService.ClearCourseCacheAsync(courseId);
			await ClearReviewCacheAsync(courseId);

			_logger.LogInformation("User {UserId} added review for course {CourseId}", userId, courseId);

			return new AuthResultDto
			{
				Succeeded = true,
				Message = "نظر شما با موفقیت ثبت شد و پس از تأیید نمایش داده می‌شود"
			};
		}
		catch (Exception ex)
		{
			_logger.LogError(ex, "Error adding review for course {CourseId} by user {UserId}", courseId, userId);
			return new AuthResultDto
			{
				Succeeded = false,
				Message = "خطا در ثبت نظر"
			};
		}
	}

	public async Task<AuthResultDto> DeleteReviewAsync(Guid reviewId, Guid userId, CancellationToken cancellationToken = default)
	{
		try
		{
			var review = await _reviewRepository.GetByIdAsync(reviewId, cancellationToken);
			if (review == null)
			{
				return new AuthResultDto
				{
					Succeeded = false,
					Message = "نظر یافت نشد"
				};
			}

			// Check if user owns the review or is admin
			if (review.StudentId != userId)
			{
				// TODO: Check if user is admin
				return new AuthResultDto
				{
					Succeeded = false,
					Message = "شما اجازه حذف این نظر را ندارید"
				};
			}

			await _reviewRepository.DeleteAsync(review, cancellationToken);

			// Clear caches
			await _courseService.ClearCourseCacheAsync(review.CourseId);
			await ClearReviewCacheAsync(review.CourseId);

			_logger.LogInformation("Review {ReviewId} deleted by user {UserId}", reviewId, userId);

			return new AuthResultDto
			{
				Succeeded = true,
				Message = "نظر با موفقیت حذف شد"
			};
		}
		catch (Exception ex)
		{
			_logger.LogError(ex, "Error deleting review {ReviewId}", reviewId);
			return new AuthResultDto
			{
				Succeeded = false,
				Message = "خطا در حذف نظر"
			};
		}
	}

	public async Task<PagedResultDto<ReviewDto>> GetCourseReviewsAsync(Guid courseId, int page, int pageSize, CancellationToken cancellationToken = default)
	{
		try
		{
			page = page < 1 ? 1 : page;
			pageSize = pageSize < 1 ? 10 : (pageSize > 50 ? 50 : pageSize);

			var cacheKey = $"course_reviews_{courseId}_{page}_{pageSize}";

			if (_cache.TryGetValue(cacheKey, out PagedResultDto<ReviewDto>? cached) && cached != null)
				return cached;

			var reviews = await _reviewRepository.GetPagedAsync(
				page, pageSize,
				r => r.CourseId == courseId && r.IsApproved && !r.IsDeleted,
				q => q.OrderByDescending(r => r.CreatedAt),
				cancellationToken);

			var totalCount = await _reviewRepository.CountAsync(r => r.CourseId == courseId && r.IsApproved && !r.IsDeleted, cancellationToken);

			var result = new PagedResultDto<ReviewDto>
			{
				Items = reviews.Select(r => new ReviewDto
				{
					Id = r.Id,
					Rating = r.Rating,
					Comment = r.Comment,
					StudentName = r.Student?.FullName ?? "کاربر",
					StudentAvatar = r.Student?.AvatarUrl,
					CreatedAt = r.CreatedAt,
					IsApproved = r.IsApproved
				}).ToList(),
				TotalCount = totalCount,
				PageNumber = page,
				PageSize = pageSize
			};

			_cache.Set(cacheKey, result, new MemoryCacheEntryOptions
			{
				AbsoluteExpirationRelativeToNow = TimeSpan.FromMinutes(10),
				SlidingExpiration = TimeSpan.FromMinutes(2)
			});

			return result;
		}
		catch (Exception ex)
		{
			_logger.LogError(ex, "Error getting reviews for course {CourseId}", courseId);
			return new PagedResultDto<ReviewDto>
			{
				Items = new List<ReviewDto>(),
				TotalCount = 0,
				PageNumber = page,
				PageSize = pageSize
			};
		}
	}

	public async Task<double> GetCourseAverageRatingAsync(Guid courseId, CancellationToken cancellationToken = default)
	{
		try
		{
			var cacheKey = $"course_avg_rating_{courseId}";

			if (_cache.TryGetValue(cacheKey, out double cached))
				return cached;

			var reviews = await _reviewRepository.GetAsync(r => r.CourseId == courseId && r.IsApproved && !r.IsDeleted, cancellationToken);
			var average = reviews.Any() ? reviews.Average(r => r.Rating) : 0;

			_cache.Set(cacheKey, average, TimeSpan.FromMinutes(15));

			return average;
		}
		catch (Exception ex)
		{
			_logger.LogError(ex, "Error getting average rating for course {CourseId}", courseId);
			return 0;
		}
	}

	public async Task ClearReviewCacheAsync(Guid? courseId = null)
	{
		try
		{
			if (courseId.HasValue)
			{
				_cache.Remove($"course_reviews_{courseId.Value}_*");
				_cache.Remove($"course_avg_rating_{courseId.Value}");
			}

			_logger.LogInformation("Review cache cleared for {CourseId}", courseId ?? Guid.Empty);
			await Task.CompletedTask;
		}
		catch (Exception ex)
		{
			_logger.LogError(ex, "Error clearing review cache");
		}
	}
}