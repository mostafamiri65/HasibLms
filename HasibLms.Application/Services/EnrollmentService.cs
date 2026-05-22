using HasibLms.Domain.Entities.AcademyEntities;
using HasibLms.Domain.Interfaces;
using HasibLms.Shared.DTOs;
using HasibLms.Shared.DTOs.Auth;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Logging;

namespace HasibLms.Application.Services;

public class EnrollmentService : IEnrollmentService
{
	private readonly IGenericRepository<Enrollment> _enrollmentRepository;
	private readonly ICourseRepository _courseRepository;
	private readonly ICourseService _courseService;
	private readonly IMemoryCache _cache;
	private readonly ILogger<EnrollmentService> _logger;

	public EnrollmentService(
		IGenericRepository<Enrollment> enrollmentRepository,
		ICourseRepository courseRepository,
		ICourseService courseService,
		IMemoryCache cache,
		ILogger<EnrollmentService> logger)
	{
		_enrollmentRepository = enrollmentRepository;
		_courseRepository = courseRepository;
		_courseService = courseService;
		_cache = cache;
		_logger = logger;
	}

	public async Task<AuthResultDto> EnrollInCourseAsync(Guid userId, Guid courseId, CancellationToken cancellationToken = default)
	{
		try
		{
			// Check if already enrolled
			var isEnrolled = await IsUserEnrolledAsync(courseId, userId, cancellationToken);
			if (isEnrolled)
			{
				return new AuthResultDto
				{
					Succeeded = false,
					Message = "شما قبلاً در این دوره ثبت نام کرده‌اید"
				};
			}

			// Check course exists and has capacity
			var course = await _courseRepository.GetByIdAsync(courseId, cancellationToken);
			if (course == null)
			{
				return new AuthResultDto
				{
					Succeeded = false,
					Message = "دوره یافت نشد"
				};
			}

			if (!course.HasCapacity)
			{
				return new AuthResultDto
				{
					Succeeded = false,
					Message = "ظرفیت دوره تکمیل شده است"
				};
			}

			if (!course.IsPublished)
			{
				return new AuthResultDto
				{
					Succeeded = false,
					Message = "این دوره هنوز منتشر نشده است"
				};
			}

			// Create enrollment
			var enrollment = new Enrollment
			{
				Id = Guid.NewGuid(),
				CourseId = courseId,
				StudentId = userId,
				EnrolledAt = DateTime.UtcNow,
				CreatedDate = DateTime.UtcNow,
				LastModifiedDate = DateTime.UtcNow
			};

			await _enrollmentRepository.AddAsync(enrollment, cancellationToken);

			// Clear caches
			await _courseService.ClearCourseCacheAsync(courseId);
			_cache.Remove($"user_enrollments_{userId}");
			_cache.Remove($"course_enrollment_count_{courseId}");

			_logger.LogInformation("User {UserId} enrolled in course {CourseId}", userId, courseId);

			return new AuthResultDto
			{
				Succeeded = true,
				Message = "ثبت نام با موفقیت انجام شد"
			};
		}
		catch (Exception ex)
		{
			_logger.LogError(ex, "Error enrolling user {UserId} in course {CourseId}", userId, courseId);
			return new AuthResultDto
			{
				Succeeded = false,
				Message = "خطا در ثبت نام. لطفاً مجدداً تلاش کنید"
			};
		}
	}

	public async Task<AuthResultDto> CancelEnrollmentAsync(Guid userId, Guid courseId, CancellationToken cancellationToken = default)
	{
		try
		{
			var enrollment = await _enrollmentRepository.GetSingleAsync(e =>
				e.CourseId == courseId && e.StudentId == userId && !e.IsDeleted, cancellationToken);

			if (enrollment == null)
			{
				return new AuthResultDto
				{
					Succeeded = false,
					Message = "ثبت نامی برای این دوره یافت نشد"
				};
			}

			await _enrollmentRepository.DeleteAsync(enrollment, cancellationToken);

			// Clear caches
			await _courseService.ClearCourseCacheAsync(courseId);
			_cache.Remove($"user_enrollments_{userId}");
			_cache.Remove($"course_enrollment_count_{courseId}");

			_logger.LogInformation("User {UserId} canceled enrollment in course {CourseId}", userId, courseId);

			return new AuthResultDto
			{
				Succeeded = true,
				Message = "انصراف از دوره با موفقیت انجام شد"
			};
		}
		catch (Exception ex)
		{
			_logger.LogError(ex, "Error canceling enrollment for user {UserId} in course {CourseId}", userId, courseId);
			return new AuthResultDto
			{
				Succeeded = false,
				Message = "خطا در انصراف از دوره"
			};
		}
	}

	public async Task<bool> IsUserEnrolledAsync(Guid courseId, Guid userId, CancellationToken cancellationToken = default)
	{
		try
		{
			var cacheKey = $"user_enrolled_{userId}_{courseId}";

			if (_cache.TryGetValue(cacheKey, out bool cached) && cached)
				return cached;

			var isEnrolled = await _enrollmentRepository.ExistsAsync(e =>
				e.CourseId == courseId && e.StudentId == userId, cancellationToken);

			_cache.Set(cacheKey, isEnrolled, TimeSpan.FromMinutes(5));

			return isEnrolled;
		}
		catch (Exception ex)
		{
			_logger.LogError(ex, "Error checking enrollment for user {UserId} in course {CourseId}", userId, courseId);
			return false;
		}
	}

	public async Task<List<CourseCardDto>> GetUserEnrolledCoursesAsync(Guid userId, CancellationToken cancellationToken = default)
	{
		try
		{
			var cacheKey = $"user_enrollments_{userId}";

			if (_cache.TryGetValue(cacheKey, out List<CourseCardDto>? cached) && cached != null)
				return cached;

			var enrollments = await _enrollmentRepository.GetAsync(e => e.StudentId == userId, cancellationToken);
			var result = new List<CourseCardDto>();

			foreach (var enrollment in enrollments)
			{
				var course = await _courseService.GetCourseDetailAsync(enrollment.CourseId, cancellationToken);
				if (course != null)
				{
					result.Add(new CourseCardDto
					{
						Id = course.Id,
						Title = course.Title,
						Slug = course.Slug,
						ShortDescription = course.ShortDescription,
						ImageUrl = course.ImageUrl,
						Price = course.Price,
						DiscountPrice = course.DiscountPrice,
						InstructorName = course.InstructorName,
						CategoryName = course.CategoryName,
						EnrolledCount = course.EnrolledCount,
						AverageRating = course.AverageRating,
						StartDate = course.StartDate
					});
				}
			}

			_cache.Set(cacheKey, result, new MemoryCacheEntryOptions
			{
				AbsoluteExpirationRelativeToNow = TimeSpan.FromMinutes(10),
				SlidingExpiration = TimeSpan.FromMinutes(2)
			});

			return result;
		}
		catch (Exception ex)
		{
			_logger.LogError(ex, "Error getting enrolled courses for user {UserId}", userId);
			return new List<CourseCardDto>();
		}
	}

	public async Task<int> GetUserEnrollmentCountAsync(Guid userId, CancellationToken cancellationToken = default)
	{
		try
		{
			return await _enrollmentRepository.CountAsync(e => e.StudentId == userId, cancellationToken);
		}
		catch (Exception ex)
		{
			_logger.LogError(ex, "Error getting enrollment count for user {UserId}", userId);
			return 0;
		}
	}

	public async Task<int> GetCourseEnrollmentCountAsync(Guid courseId, CancellationToken cancellationToken = default)
	{
		try
		{
			var cacheKey = $"course_enrollment_count_{courseId}";

			if (_cache.TryGetValue(cacheKey, out int cached))
				return cached;

			var count = await _enrollmentRepository.CountAsync(e => e.CourseId == courseId, cancellationToken);
			_cache.Set(cacheKey, count, TimeSpan.FromMinutes(15));

			return count;
		}
		catch (Exception ex)
		{
			_logger.LogError(ex, "Error getting enrollment count for course {CourseId}", courseId);
			return 0;
		}
	}
}