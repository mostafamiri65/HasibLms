using HasibLms.Domain.Entities;
using HasibLms.Domain.Entities.AcademyEntities;
using HasibLms.Domain.Entities.InvestigativeEntities;
using HasibLms.Domain.Interfaces;
using HasibLms.Shared.Constants;
using HasibLms.Shared.DTOs;
using HasibLms.Shared.DTOs.Admin;
using HasibLms.Shared.DTOs.Auth;
using HasibLms.Shared.DTOs.Instructor;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using System.Text.RegularExpressions;

namespace HasibLms.Application.Services;

public class AdminService : IAdminService
{
	private readonly UserManager<User> _userManager;
	private readonly RoleManager<IdentityRole<Guid>> _roleManager;
	private readonly ILogger<AdminService> _logger;
	private readonly ICourseRepository _courseRepository;
	private readonly IGenericRepository<CourseReview> _reviewRepository;
	private readonly IGenericRepository<Enrollment> _enrollmentRepository;
	private readonly IGenericRepository<Article> _articleRepository;
	private readonly IUserRepository _userRepository;

	public AdminService(
		UserManager<User> userManager,
		RoleManager<IdentityRole<Guid>> roleManager,
		ILogger<AdminService> logger,
		ICourseRepository courseRepository,
		IGenericRepository<CourseReview> reviewRepository,
		IGenericRepository<Enrollment> enrollmentRepository,
		IGenericRepository<Article> articleRepository,
		IUserRepository userRepository)
	{
		_userManager = userManager;
		_roleManager = roleManager;
		_logger = logger;
		_courseRepository = courseRepository;
		_reviewRepository = reviewRepository;
		_enrollmentRepository = enrollmentRepository;
		_articleRepository = articleRepository;
		_userRepository = userRepository;
	}

	#region User Management

	public async Task<PagedResultDto<UserListDto>> GetUsersAsync(
		int page, int pageSize, string? role = null, CancellationToken cancellationToken = default)
	{
		var users = await _userRepository.GetPagedUsersWithRolesAsync(page, pageSize, role, cancellationToken);
		var totalCount = await _userRepository.GetTotalUsersCountAsync(role, cancellationToken);

		return new PagedResultDto<UserListDto>
		{
			Items = users,
			TotalCount = totalCount,
			PageNumber = page,
			PageSize = pageSize
		};
	}

	public async Task<AuthResultDto> AssignRoleAsync(Guid userId, string role, CancellationToken cancellationToken = default)
	{
		try
		{
			var user = await _userManager.FindByIdAsync(userId.ToString());
			if (user == null)
				return new AuthResultDto { Succeeded = false, Message = "کاربر یافت نشد" };

			if (!await _roleManager.RoleExistsAsync(role))
				await _roleManager.CreateAsync(new IdentityRole<Guid>(role));

			if (await _userManager.IsInRoleAsync(user, role))
				return new AuthResultDto { Succeeded = false, Message = $"کاربر قبلاً در نقش {role} است" };

			var result = await _userManager.AddToRoleAsync(user, role);
			if (!result.Succeeded)
				return new AuthResultDto
				{
					Succeeded = false,
					Errors = result.Errors.Select(e => e.Description).ToList()
				};

			_logger.LogInformation("User {UserId} assigned to role {Role}", userId, role);
			return new AuthResultDto { Succeeded = true, Message = $"نقش {role} با موفقیت به کاربر اضافه شد" };
		}
		catch (Exception ex)
		{
			_logger.LogError(ex, "Error assigning role {Role} to user {UserId}", role, userId);
			return new AuthResultDto { Succeeded = false, Message = "خطا در اختصاص نقش" };
		}
	}

	public async Task<AuthResultDto> RemoveRoleAsync(Guid userId, string role, CancellationToken cancellationToken = default)
	{
		try
		{
			var user = await _userManager.FindByIdAsync(userId.ToString());
			if (user == null)
				return new AuthResultDto { Succeeded = false, Message = "کاربر یافت نشد" };

			if (!await _userManager.IsInRoleAsync(user, role))
				return new AuthResultDto { Succeeded = false, Message = $"کاربر در نقش {role} نیست" };

			if (role == UserRoles.Admin)
			{
				var admins = await _userManager.GetUsersInRoleAsync(UserRoles.Admin);
				if (admins.Count <= 1 && admins.Any(a => a.Id == userId))
					return new AuthResultDto { Succeeded = false, Message = "نمی‌توان آخرین ادمین سیستم را حذف کرد" };
			}

			var result = await _userManager.RemoveFromRoleAsync(user, role);
			if (!result.Succeeded)
				return new AuthResultDto
				{
					Succeeded = false,
					Errors = result.Errors.Select(e => e.Description).ToList()
				};

			_logger.LogInformation("User {UserId} removed from role {Role}", userId, role);
			return new AuthResultDto { Succeeded = true, Message = $"نقش {role} با موفقیت از کاربر حذف شد" };
		}
		catch (Exception ex)
		{
			_logger.LogError(ex, "Error removing role {Role} from user {UserId}", role, userId);
			return new AuthResultDto { Succeeded = false, Message = "خطا در حذف نقش" };
		}
	}

	public async Task<AuthResultDto> ToggleUserStatusAsync(Guid userId, CancellationToken cancellationToken = default)
	{
		try
		{
			var user = await _userManager.FindByIdAsync(userId.ToString());
			if (user == null)
				return new AuthResultDto { Succeeded = false, Message = "کاربر یافت نشد" };

			if (await _userManager.IsInRoleAsync(user, UserRoles.Admin))
			{
				var admins = await _userManager.GetUsersInRoleAsync(UserRoles.Admin);
				if (admins.Count <= 1)
					return new AuthResultDto { Succeeded = false, Message = "نمی‌توان آخرین ادمین سیستم را غیرفعال کرد" };
			}

			if (await _userManager.IsLockedOutAsync(user))
			{
				await _userManager.SetLockoutEndDateAsync(user, null);
				return new AuthResultDto { Succeeded = true, Message = "حساب کاربر با موفقیت فعال شد" };
			}
			else
			{
				await _userManager.SetLockoutEndDateAsync(user, DateTimeOffset.UtcNow.AddYears(100));
				return new AuthResultDto { Succeeded = true, Message = "حساب کاربر با موفقیت غیرفعال شد" };
			}
		}
		catch (Exception ex)
		{
			_logger.LogError(ex, "Error toggling status for user {UserId}", userId);
			return new AuthResultDto { Succeeded = false, Message = "خطا در تغییر وضعیت کاربر" };
		}
	}

	#endregion

	#region Course Management

	public async Task<AuthResultDto> ApproveCourseAsync(Guid courseId, CancellationToken cancellationToken = default)
	{
		try
		{
			var course = await _courseRepository.GetByIdAsync(courseId, cancellationToken);
			if (course == null)
				return new AuthResultDto { Succeeded = false, Message = "دوره یافت نشد" };

			course.Publish();
			course.LastModifiedDate = DateTime.Now;
			await _courseRepository.UpdateAsync(course, cancellationToken);

			_logger.LogInformation("Course {CourseId} ({Title}) approved", courseId, course.Title);
			return new AuthResultDto { Succeeded = true, Message = "دوره با موفقیت تأیید و منتشر شد" };
		}
		catch (Exception ex)
		{
			_logger.LogError(ex, "Error approving course {CourseId}", courseId);
			return new AuthResultDto { Succeeded = false, Message = "خطا در تأیید دوره" };
		}
	}
	public async Task<AuthResultDto> UnPublishCourse(Guid courseId, CancellationToken cancellationToken = default)
	{
		try
		{
			var course = await _courseRepository.GetByIdAsync(courseId, cancellationToken);
			if (course == null)
				return new AuthResultDto { Succeeded = false, Message = "دوره یافت نشد" };

			course.Unpublish();
			course.LastModifiedDate = DateTime.Now;
			await _courseRepository.UpdateAsync(course, cancellationToken);

			_logger.LogInformation("Course {CourseId} ({Title}) Unpublished", courseId, course.Title);
			return new AuthResultDto { Succeeded = true, Message = "دوره با موفقیت عدم منتشر یافت" };
		}
		catch (Exception ex)
		{
			_logger.LogError(ex, "Error approving course {CourseId}", courseId);
			return new AuthResultDto { Succeeded = false, Message = "خطا در تأیید دوره" };
		}
	}

	public async Task<AuthResultDto> FeatureCourseAsync(Guid courseId, bool isFeatured, CancellationToken cancellationToken = default)
	{
		try
		{
			var course = await _courseRepository.GetByIdAsync(courseId, cancellationToken);
			if (course == null)
				return new AuthResultDto { Succeeded = false, Message = "دوره یافت نشد" };

			if (isFeatured) course.Feature();
			else course.UnFeature();

			course.LastModifiedDate = DateTime.Now;
			await _courseRepository.UpdateAsync(course, cancellationToken);

			var message = isFeatured ? "دوره به دوره‌های منتخب اضافه شد" : "دوره از دوره‌های منتخب حذف شد";

			_logger.LogInformation("Course {CourseId} featured status changed to {IsFeatured}", courseId, isFeatured);
			return new AuthResultDto { Succeeded = true, Message = message };
		}
		catch (Exception ex)
		{
			_logger.LogError(ex, "Error changing featured status for course {CourseId}", courseId);
			return new AuthResultDto { Succeeded = false, Message = "خطا در تغییر وضعیت منتخب" };
		}
	}

	public async Task<PagedResultDto<CourseListAdminDto>> GetCoursesForAdminAsync(
		int page, int pageSize, string? status = null, CancellationToken cancellationToken = default)
	{

		var courses = await _courseRepository.GetCoursesForAdmin(page, pageSize, status);

		var totalCount = await _courseRepository.CountAsync(c =>
			string.IsNullOrEmpty(status) ||
			(status == "published" && c.IsPublished) ||
			(status == "pending" && !c.IsPublished), cancellationToken);

		var items = new List<CourseListAdminDto>();
		foreach (var course in courses)
		{
			var enrolledCount = await _enrollmentRepository.CountAsync(e => e.CourseId == course.Id, cancellationToken);
			items.Add(new CourseListAdminDto
			{
				Id = course.Id,
				Title = course.Title,
				Slug = course.Slug,
				Price = course.Price,
				DiscountPrice = course.DiscountPrice,
				IsPublished = course.IsPublished,
				IsFeatured = course.IsFeatured,
				InstructorName = course.Instructor?.FullName ?? "نامشخص",
				CategoryName = course.Category?.Name ?? "دسته‌بندی نشده",
				EnrolledCount = enrolledCount,
				CreatedDate = course.CreatedDate,
				StartDate = course.StartDate
			});
		}

		return new PagedResultDto<CourseListAdminDto>
		{
			Items = items,
			TotalCount = totalCount,
			PageNumber = page,
			PageSize = pageSize
		};
	}

	public async Task<AuthResultDto> DeleteCourseAsync(Guid courseId, CancellationToken cancellationToken = default)
	{
		try
		{
			var course = await _courseRepository.GetByIdAsync(courseId, cancellationToken);
			if (course == null)
				return new AuthResultDto { Succeeded = false, Message = "دوره یافت نشد" };

			await _courseRepository.DeleteAsync(course, cancellationToken);

			_logger.LogInformation("Course {CourseId} ({Title}) deleted", courseId, course.Title);
			return new AuthResultDto { Succeeded = true, Message = "دوره با موفقیت حذف شد" };
		}
		catch (Exception ex)
		{
			_logger.LogError(ex, "Error deleting course {CourseId}", courseId);
			return new AuthResultDto { Succeeded = false, Message = "خطا در حذف دوره" };
		}
	}

	// ==================== ✅ جدید ====================

	public async Task<List<InstructorSelectDto>> GetAllUsersForSelectAsync(CancellationToken ct = default)
	{
		// همه‌ی کاربران را می‌گیریم، نه فقط مدرسان
		var users = await _userManager.Users
			.OrderBy(u => u.FullName)
			.ToListAsync(ct);

		var result = new List<InstructorSelectDto>();
		foreach (var user in users)
		{
			var isInstructor = await _userManager.IsInRoleAsync(user, UserRoles.Instructor);
			result.Add(new InstructorSelectDto
			{
				Id = user.Id,
				FullName = user.FullName,
				Email = user.Email ?? string.Empty,
				IsInstructor = isInstructor
			});
		}
		return result;
	}

	public async Task<List<InstructorSelectDto>> GetInstructorsForSelectAsync(CancellationToken ct = default)
	{
		var instructors = await _userRepository.GetUsersInRoleAsync(UserRoles.Instructor, ct);
		return instructors
			.Where(u => !string.IsNullOrEmpty(u.FullName))
			.OrderBy(u => u.FullName)
			.Select(u => new InstructorSelectDto
			{
				Id = u.Id,
				FullName = u.FullName,
				Email = u.Email ?? string.Empty
			})
			.ToList();
	}

	public async Task<AuthResultDto> CreateCourseByAdminAsync(CreateCourseByAdminDto model, Guid adminId, CancellationToken ct = default)
	{
		try
		{
			// 1) بررسی وجود کاربر انتخاب‌شده
			var instructor = await _userManager.FindByIdAsync(model.InstructorId.ToString());
			if (instructor == null)
				return new AuthResultDto { Succeeded = false, Message = "کاربر انتخاب شده یافت نشد" };

			// 2) اگر کاربر مدرس نیست، نقش مدرس به او داده شود
			var isInstructor = await _userManager.IsInRoleAsync(instructor, UserRoles.Instructor);
			if (!isInstructor)
			{
				var addRoleResult = await _userManager.AddToRoleAsync(instructor, UserRoles.Instructor);
				if (!addRoleResult.Succeeded)
				{
					_logger.LogWarning("Failed to add Instructor role to user {UserId}", model.InstructorId);
					return new AuthResultDto
					{
						Succeeded = false,
						Message = "خطا در اختصاص نقش مدرس به کاربر",
						Errors = addRoleResult.Errors.Select(e => e.Description).ToList()
					};
				}
				_logger.LogInformation("Instructor role assigned to user {UserId} by admin {AdminId}", model.InstructorId, adminId);
			}

			// 3) ساخت Slug یکتا
			var slug = GenerateSlug(model.Title);
			if (string.IsNullOrEmpty(slug))
				slug = $"course-{Guid.NewGuid():N}".Substring(0, 20);

			if (await _courseRepository.ExistsAsync(c => c.Slug == slug, ct))
				slug = $"{slug}-{Guid.NewGuid():N}".Substring(0, 30);

			// 4) ساخت دوره
			var course = new Course
			{
				Id = Guid.NewGuid(),
				Title = model.Title,
				Slug = slug,
				ShortDescription = model.ShortDescription ?? string.Empty,
				Description = model.Description,
				Price = model.Price,
				DiscountPrice = model.DiscountPrice,
				ImageUrl = model.ImageUrl,
				IntroVideoUrl = model.IntroVideoUrl,
				// FullVideoUrl = model.FullVideoUrl,  // اگر فیلد را اضافه کردید
				CourseType = model.CourseType,
				StartDate = model.StartDate,
				EndDate = model.EndDate,
				DurationHours = model.DurationHours,
				Capacity = model.Capacity,
				CategoryId = model.CategoryId,
				InstructorId = model.InstructorId,
				IsPublished = model.IsPublished,
				IsFeatured = model.IsFeatured,
				MetaTitle = model.MetaTitle,
				MetaDescription = model.MetaDescription,
				CreatedDate = DateTime.UtcNow,
				LastModifiedDate = DateTime.UtcNow,
				CreatedBy = adminId
			};

			await _courseRepository.AddAsync(course, ct);

			_logger.LogInformation("Course {CourseId} created by admin {AdminId} for user {InstructorId}",
				course.Id, adminId, model.InstructorId);

			return new AuthResultDto
			{
				Succeeded = true,
				Message = isInstructor
					? "دوره با موفقیت ایجاد شد"
					: "دوره ایجاد شد و نقش مدرس به کاربر اختصاص یافت",
				Data = course.Id
			};
		}
		catch (Exception ex)
		{
			_logger.LogError(ex, "Error creating course by admin {AdminId}", adminId);
			return new AuthResultDto
			{
				Succeeded = false,
				Message = "خطا در ایجاد دوره",
				Errors = new List<string> { ex.Message }
			};
		}
	}

	public async Task<AuthResultDto> UpdateCourseByAdminAsync(UpdateCourseByAdminDto model, Guid adminId, CancellationToken ct = default)
	{
		try
		{
			var course = await _courseRepository.GetByIdAsync(model.Id, ct);
			if (course == null)
				return new AuthResultDto { Succeeded = false, Message = "دوره یافت نشد" };

			// 1) اگر مدرس تغییر کرد
			if (course.InstructorId != model.InstructorId)
			{
				var newInstructor = await _userManager.FindByIdAsync(model.InstructorId.ToString());
				if (newInstructor == null)
					return new AuthResultDto { Succeeded = false, Message = "کاربر انتخاب شده یافت نشد" };

				if (!await _userManager.IsInRoleAsync(newInstructor, UserRoles.Instructor))
				{
					var addRole = await _userManager.AddToRoleAsync(newInstructor, UserRoles.Instructor);
					if (!addRole.Succeeded)
						return new AuthResultDto
						{
							Succeeded = false,
							Message = "خطا در اختصاص نقش مدرس به کاربر جدید",
							Errors = addRole.Errors.Select(e => e.Description).ToList()
						};
				}
			}

			// 2) به‌روزرسانی Slug اگر عنوان تغییر کرد
			if (course.Title != model.Title)
			{
				var newSlug = GenerateSlug(model.Title);
				if (!string.IsNullOrEmpty(newSlug) &&
					await _courseRepository.ExistsAsync(c => c.Slug == newSlug && c.Id != model.Id, ct))
				{
					newSlug = $"{newSlug}-{Guid.NewGuid():N}".Substring(0, 30);
				}
				course.Slug = newSlug;
			}

			// 3) به‌روزرسانی فیلدها
			course.Title = model.Title;
			course.ShortDescription = model.ShortDescription ?? string.Empty;
			course.Description = model.Description;
			course.Price = model.Price;
			course.DiscountPrice = model.DiscountPrice;
			course.ImageUrl = model.ImageUrl;
			course.IntroVideoUrl = model.IntroVideoUrl;
			// course.FullVideoUrl = model.FullVideoUrl;
			course.CourseType = model.CourseType;
			course.StartDate = model.StartDate;
			course.EndDate = model.EndDate;
			course.DurationHours = model.DurationHours;
			course.Capacity = model.Capacity;
			course.CategoryId = model.CategoryId;
			course.InstructorId = model.InstructorId;
			course.MetaTitle = model.MetaTitle;
			course.MetaDescription = model.MetaDescription;
			course.IsPublished = model.IsPublished;
			course.IsFeatured = model.IsFeatured;
			course.LastModifiedDate = DateTime.UtcNow;
			course.LastModifiedBy = adminId;

			await _courseRepository.UpdateAsync(course, ct);

			_logger.LogInformation("Course {CourseId} updated by admin {AdminId}", model.Id, adminId);

			return new AuthResultDto
			{
				Succeeded = true,
				Message = "دوره با موفقیت بروزرسانی شد"
			};
		}
		catch (Exception ex)
		{
			_logger.LogError(ex, "Error updating course {CourseId} by admin {AdminId}", model.Id, adminId);
			return new AuthResultDto
			{
				Succeeded = false,
				Message = "خطا در بروزرسانی دوره",
				Errors = new List<string> { ex.Message }
			};
		}
	}

	public async Task<UpdateCourseByAdminDto?> GetCourseForAdminEditAsync(Guid courseId, CancellationToken ct = default)
	{
		try
		{
			var course = await _courseRepository.GetByIdAsync(courseId, ct);
			if (course == null) return null;

			return new UpdateCourseByAdminDto
			{
				Id = course.Id,
				Title = course.Title,
				Slug = course.Slug,
				ShortDescription = course.ShortDescription,
				Description = course.Description,
				Price = course.Price,
				DiscountPrice = course.DiscountPrice,
				ImageUrl = course.ImageUrl,
				IntroVideoUrl = course.IntroVideoUrl,
				 FullVideoUrl = course.FullVideoUrl,
				CourseType = course.CourseType,
				StartDate = course.StartDate,
				EndDate = course.EndDate,
				DurationHours = course.DurationHours,
				Capacity = course.Capacity,
				CategoryId = course.CategoryId,
				InstructorId = course.InstructorId,
				IsPublished = course.IsPublished,
				IsFeatured = course.IsFeatured,
				MetaTitle = course.MetaTitle,
				MetaDescription = course.MetaDescription
			};
		}
		catch (Exception ex)
		{
			_logger.LogError(ex, "Error getting course for admin edit {CourseId}", courseId);
			return null;
		}
	}

	#endregion

	#region Review Management

	public async Task<AuthResultDto> ApproveReviewAsync(Guid reviewId, CancellationToken cancellationToken = default)
	{
		try
		{
			var review = await _reviewRepository.GetByIdAsync(reviewId, cancellationToken);
			if (review == null)
				return new AuthResultDto { Succeeded = false, Message = "نظر یافت نشد" };

			review.IsApproved = true;
			await _reviewRepository.UpdateAsync(review, cancellationToken);

			_logger.LogInformation("Review {ReviewId} approved", reviewId);
			return new AuthResultDto { Succeeded = true, Message = "نظر با موفقیت تأیید شد" };
		}
		catch (Exception ex)
		{
			_logger.LogError(ex, "Error approving review {ReviewId}", reviewId);
			return new AuthResultDto { Succeeded = false, Message = "خطا در تأیید نظر" };
		}
	}

	public async Task<AuthResultDto> DeleteReviewAsync(Guid reviewId, CancellationToken cancellationToken = default)
	{
		try
		{
			var review = await _reviewRepository.GetByIdAsync(reviewId, cancellationToken);
			if (review == null)
				return new AuthResultDto { Succeeded = false, Message = "نظر یافت نشد" };

			await _reviewRepository.DeleteAsync(review, cancellationToken);

			_logger.LogInformation("Review {ReviewId} deleted", reviewId);
			return new AuthResultDto { Succeeded = true, Message = "نظر با موفقیت حذف شد" };
		}
		catch (Exception ex)
		{
			_logger.LogError(ex, "Error deleting review {ReviewId}", reviewId);
			return new AuthResultDto { Succeeded = false, Message = "خطا در حذف نظر" };
		}
	}

	public async Task<PagedResultDto<ReviewListAdminDto>> GetReviewsForAdminAsync(
		int page, int pageSize, bool? pending = null, CancellationToken cancellationToken = default)
	{
		var predicate = pending == true
			? (System.Linq.Expressions.Expression<Func<CourseReview, bool>>)(r => !r.IsApproved)
			: null;

		var reviews = await _reviewRepository.GetPagedAsync(page, pageSize, predicate,
			q => q.OrderByDescending(r => r.CreatedAt), cancellationToken);

		var totalCount = await _reviewRepository.CountAsync(predicate, cancellationToken);

		var items = new List<ReviewListAdminDto>();
		foreach (var review in reviews)
		{
			var course = await _courseRepository.GetByIdAsync(review.CourseId, cancellationToken);
			items.Add(new ReviewListAdminDto
			{
				Id = review.Id,
				Rating = review.Rating,
				Comment = review.Comment,
				IsApproved = review.IsApproved,
				CourseTitle = course?.Title ?? "نامشخص",
				StudentName = review.Student?.FullName ?? "کاربر",
				CreatedAt = review.CreatedAt
			});
		}

		return new PagedResultDto<ReviewListAdminDto>
		{
			Items = items,
			TotalCount = totalCount,
			PageNumber = page,
			PageSize = pageSize
		};
	}

	#endregion

	#region Dashboard Statistics

	public async Task<DashboardStatsDto> GetDashboardStatsAsync(CancellationToken cancellationToken = default)
	{
		try
		{
			var totalUsers = await _userRepository.GetTotalUsersCountAsync(null, cancellationToken);
			var studentsCount = await _userRepository.GetUsersCountInRoleAsync(UserRoles.Student, cancellationToken);
			var instructorsCount = await _userRepository.GetUsersCountInRoleAsync(UserRoles.Instructor, cancellationToken);

			var allCoursesCount = await _courseRepository.CountAsync(null, cancellationToken);
			var publishedCoursesCount = await _courseRepository.CountAsync(c => c.IsPublished, cancellationToken);

			var enrollments = await _enrollmentRepository.GetAllAsync(cancellationToken);
			var totalEnrollments = enrollments.Count;

			var articlesCount = await _articleRepository.CountAsync(a => a.IsPublished, cancellationToken);
			var pendingReviewsCount = await _reviewRepository.CountAsync(r => !r.IsApproved, cancellationToken);

			var allCourses = await _courseRepository.GetAllAsync(cancellationToken);
			var coursesDictionary = allCourses.ToDictionary(c => c.Id, c => c);

			var totalRevenue = 0m;
			var courseEnrollmentCount = new Dictionary<Guid, int>();

			foreach (var enrollment in enrollments)
			{
				if (coursesDictionary.TryGetValue(enrollment.CourseId, out var course))
				{
					var price = course.DiscountPrice ?? course.Price;
					totalRevenue += price;

					if (!courseEnrollmentCount.ContainsKey(enrollment.CourseId))
						courseEnrollmentCount[enrollment.CourseId] = 0;
					courseEnrollmentCount[enrollment.CourseId]++;
				}
			}

			var monthlyEnrollments = enrollments
				.GroupBy(e => new { e.EnrolledAt.Year, e.EnrolledAt.Month })
				.Select(g => new
				{
					g.Key.Year,
					g.Key.Month,
					Count = g.Count()
				})
				.OrderByDescending(x => x.Year)
				.ThenByDescending(x => x.Month)
				.Take(12)
				.Select(g => new MonthlyStatsDto
				{
					Month = $"{g.Year}/{g.Month}",
					Count = g.Count
				})
				.OrderBy(m => m.Month)
				.ToList();

			var popularCourses = courseEnrollmentCount
				.OrderByDescending(x => x.Value)
				.Take(5)
				.ToList();

			var popularCourseDtos = new List<PopularCourseDto>();
			foreach (var pc in popularCourses)
			{
				if (coursesDictionary.TryGetValue(pc.Key, out var course))
				{
					var price = course.DiscountPrice ?? course.Price;
					popularCourseDtos.Add(new PopularCourseDto
					{
						Title = course.Title,
						Enrollments = pc.Value,
						Revenue = pc.Value * price
					});
				}
			}

			return new DashboardStatsDto
			{
				TotalUsers = totalUsers,
				TotalStudents = studentsCount,
				TotalInstructors = instructorsCount,
				TotalCourses = allCoursesCount,
				PublishedCourses = publishedCoursesCount,
				TotalEnrollments = totalEnrollments,
				TotalArticles = articlesCount,
				PendingReviews = pendingReviewsCount,
				TotalRevenue = totalRevenue,
				MonthlyEnrollments = monthlyEnrollments,
				PopularCourses = popularCourseDtos
			};
		}
		catch (Exception ex)
		{
			_logger.LogError(ex, "Error getting dashboard stats");
			return new DashboardStatsDto();
		}
	}

	#endregion

	#region Helper Methods

	private static string GenerateSlug(string text)
	{
		if (string.IsNullOrWhiteSpace(text)) return string.Empty;
		var slug = text.Trim().ToLowerInvariant();
		slug = slug.Replace("ي", "ی").Replace("ك", "ک");
		slug = slug.Replace(" ", "-");
		slug = System.Text.RegularExpressions.Regex.Replace(slug, @"[^a-z0-9\u0600-\u06FF\-]", "");
		slug = System.Text.RegularExpressions.Regex.Replace(slug, @"-{2,}", "-");
		return slug.Trim('-');
	}

	#endregion
}