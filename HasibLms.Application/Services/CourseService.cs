using HasibLms.Domain.Entities.AcademyEntities;
using HasibLms.Domain.Interfaces;
using HasibLms.Shared.DTOs;
using HasibLms.Shared.DTOs.Auth;
using HasibLms.Shared.DTOs.Instructor;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Logging;

namespace HasibLms.Application.Services;

public class CourseService : ICourseService
{
	private readonly ICourseRepository _courseRepository;
	private readonly ICategoryRepository _categoryRepository;
	private readonly IArticleRepository _articleRepository;
	private readonly IGenericRepository<Enrollment> _enrollmentRepository;
	private readonly IMemoryCache _cache;
	private readonly ILogger<CourseService> _logger;
	private readonly ILogger<CategoryService> _categoryLogger;
	private readonly IGenericRepository<Syllabus> _syllabusRepository;
	private readonly IGenericRepository<Lesson> _lessonRepository;
	public CourseService(
		ICourseRepository courseRepository,
		ICategoryRepository categoryRepository,
		IArticleRepository articleRepository,
		IGenericRepository<Enrollment> enrollmentRepository,
		IMemoryCache cache,
		ILogger<CourseService> logger, ILogger<CategoryService> categoryLogger,
		IGenericRepository<Syllabus> syllabusRepository, IGenericRepository<Lesson> lessonRepository)
	{
		_courseRepository = courseRepository;
		_categoryRepository = categoryRepository;
		_articleRepository = articleRepository;
		_enrollmentRepository = enrollmentRepository;
		_cache = cache;
		_logger = logger;
		_categoryLogger = categoryLogger;
		_syllabusRepository = syllabusRepository;
		_lessonRepository = lessonRepository;
	}

	public async Task<CourseDetailDto?> GetCourseDetailAsync(Guid id, CancellationToken cancellationToken = default)
	{
		try
		{
			var cacheKey = $"course_detail_{id}";

			if (_cache.TryGetValue(cacheKey, out CourseDetailDto? cached) && cached != null)
				return cached;

			var course = await _courseRepository.GetCourseWithDetailsAsync(id, cancellationToken);
			if (course == null) return null;

			var enrolledCount = await _enrollmentRepository.CountAsync(e => e.CourseId == course.Id, cancellationToken);
			var averageRating = course.Reviews != null && course.Reviews.Any()
				? course.Reviews.Average(r => r.Rating)
				: 0;

			var result = new CourseDetailDto
			{
				Id = course.Id,
				Title = course.Title,
				Slug = course.Slug,
				ShortDescription = course.ShortDescription,
				Description = course.Description,
				ImageUrl = course.ImageUrl,
				IntroVideoUrl = course.IntroVideoUrl,
				Price = course.Price,
				DiscountPrice = course.DiscountPrice,
				InstructorName = course.Instructor?.FullName ?? "نامشخص",
				InstructorAvatar = course.Instructor?.AvatarUrl ?? string.Empty,
				CategoryName = course.Category?.Name ?? "دسته‌بندی نشده",
				DurationHours = course.DurationHours,
				Capacity = course.Capacity,
				RemainingCapacity = course.Capacity - enrolledCount,
				StartDate = course.StartDate,
				EndDate = course.EndDate,
				CourseType = course.CourseType.ToString(),
				EnrolledCount = enrolledCount,
				AverageRating = averageRating,
				ReviewCount = course.Reviews?.Count(r => r.IsApproved) ?? 0,
				IsPublished = course.IsPublished,
				MetaTitle = course.MetaTitle,
				MetaDescription = course.MetaDescription,
				Syllabuses = course.Syllabuses?.OrderBy(s => s.Order).Select(s => new SyllabusDto
				{
					Id = s.Id,
					Title = s.Title,
					Description = s.Description,
					Order = s.Order,
					Lessons = s.Lessons?.OrderBy(l => l.Order).Select(l => new LessonDto
					{
						Id = l.Id,
						Title = l.Title,
						DurationMinutes = l.DurationMinutes,
						IsPreview = l.IsPreview,
						VideoUrl = l.VideoUrl,
						Order = l.Order
					}).ToList() ?? new List<LessonDto>()
				}).ToList() ?? new List<SyllabusDto>(),
				Reviews = course.Reviews?.Where(r => r.IsApproved)
					.OrderByDescending(r => r.CreatedAt)
					.Take(10)
					.Select(r => new ReviewDto
					{
						Id = r.Id,
						Rating = r.Rating,
						Comment = r.Comment,
						StudentName = r.Student?.FullName ?? "کاربر",
						StudentAvatar = r.Student?.AvatarUrl,
						CreatedAt = r.CreatedAt,
						IsApproved = r.IsApproved
					}).ToList() ?? new List<ReviewDto>()
			};

			// Get related courses
			result.RelatedCourses = (await GetRelatedCoursesAsync(course.Id, 4, cancellationToken)).ToList();

			_cache.Set(cacheKey, result, new MemoryCacheEntryOptions
			{
				AbsoluteExpirationRelativeToNow = TimeSpan.FromMinutes(15),
				SlidingExpiration = TimeSpan.FromMinutes(5)
			});

			return result;
		}
		catch (Exception ex)
		{
			_logger.LogError(ex, "Error getting course detail for ID: {Id}", id);
			return null;
		}
	}

	public async Task<CourseDetailDto?> GetCourseBySlugAsync(string slug, CancellationToken cancellationToken = default)
	{
		try
		{
			var cacheKey = $"course_slug_{slug}";

			if (_cache.TryGetValue(cacheKey, out Guid? courseId) && courseId.HasValue)
				return await GetCourseDetailAsync(courseId.Value, cancellationToken);

			var course = await _courseRepository.GetSingleAsync(c => c.Slug == slug && c.IsPublished, cancellationToken);
			if (course == null) return null;

			_cache.Set(cacheKey, course.Id, TimeSpan.FromHours(1));

			return await GetCourseDetailAsync(course.Id, cancellationToken);
		}
		catch (Exception ex)
		{
			_logger.LogError(ex, "Error getting course by slug: {Slug}", slug);
			return null;
		}
	}

	public async Task<PagedResultDto<CourseCardDto>> GetPagedCoursesAsync(
		int page, int pageSize, Guid? categoryId = null, string? searchTerm = null,
		string? sortBy = null, CancellationToken cancellationToken = default)
	{
		try
		{
			page = page < 1 ? 1 : page;
			pageSize = pageSize < 1 ? 10 : (pageSize > 100 ? 100 : pageSize);

			var cacheKey = $"courses_page_{page}_{pageSize}_{categoryId}_{searchTerm}_{sortBy}";

			if (_cache.TryGetValue(cacheKey, out PagedResultDto<CourseCardDto>? cached) && cached != null)
				return cached;

			var courses = await _courseRepository.GetPagedCoursesAsync(page, pageSize, categoryId, searchTerm, sortBy, cancellationToken);
			var totalCount = await _courseRepository.CountAsync(c =>
				(categoryId == null || c.CategoryId == categoryId) &&
				(string.IsNullOrEmpty(searchTerm) || c.Title.Contains(searchTerm) || c.ShortDescription.Contains(searchTerm)) &&
				c.IsPublished, cancellationToken);
			var items = new List<CourseCardDto>();
			foreach (var course in courses)
			{
				items.Add(MapToCardDto(course)!);
			}
			var result = new PagedResultDto<CourseCardDto>
			{
				Items = items,
				TotalCount = totalCount,
				PageNumber = page,
				PageSize = pageSize
			};

			_cache.Set(cacheKey, result, new MemoryCacheEntryOptions
			{
				AbsoluteExpirationRelativeToNow = TimeSpan.FromMinutes(5),
				SlidingExpiration = TimeSpan.FromMinutes(1)
			});

			return result;
		}
		catch (Exception ex)
		{
			_logger.LogError(ex, "Error getting paged courses");
			return new PagedResultDto<CourseCardDto>
			{
				Items = new List<CourseCardDto>(),
				TotalCount = 0,
				PageNumber = page,
				PageSize = pageSize
			};
		}
	}

	public async Task<IReadOnlyList<CourseCardDto>> GetFeaturedCoursesAsync(int count, CancellationToken cancellationToken = default)
	{
		try
		{
			count = Math.Clamp(count, 1, 50);
			var cacheKey = $"featured_courses_{count}";

			if (_cache.TryGetValue(cacheKey, out List<CourseCardDto>? cached) && cached != null)
				return cached;

			var courses = await _courseRepository.GetFeaturedCoursesAsync(count, cancellationToken);
			var result = courses.Select(MapToCardDto).Where(dto => dto != null).ToList()!;

			_cache.Set(cacheKey, result, new MemoryCacheEntryOptions
			{
				AbsoluteExpirationRelativeToNow = TimeSpan.FromHours(1),
				SlidingExpiration = TimeSpan.FromMinutes(10)
			});

			return result!;
		}
		catch (Exception ex)
		{
			_logger.LogError(ex, "Error getting featured courses");
			return new List<CourseCardDto>();
		}
	}

	public async Task<IReadOnlyList<CourseCardDto>> GetLatestCoursesAsync(int count, CancellationToken cancellationToken = default)
	{
		try
		{
			count = Math.Clamp(count, 1, 50);
			var cacheKey = $"latest_courses_{count}";

			if (_cache.TryGetValue(cacheKey, out List<CourseCardDto>? cached) && cached != null)
				return cached;

			var courses = await _courseRepository.GetAsync(c => c.IsPublished, cancellationToken);
			var result = courses
				.OrderByDescending(c => c.CreatedDate)
				.Take(count)
				.Select(MapToCardDto)
				.Where(dto => dto != null)
				.ToList()!;

			_cache.Set(cacheKey, result, new MemoryCacheEntryOptions
			{
				AbsoluteExpirationRelativeToNow = TimeSpan.FromHours(1),
				SlidingExpiration = TimeSpan.FromMinutes(10)
			});

			return result!;
		}
		catch (Exception ex)
		{
			_logger.LogError(ex, "Error getting latest courses");
			return new List<CourseCardDto>();
		}
	}

	public async Task<IReadOnlyList<CourseCardDto>> GetPopularCoursesAsync(int count, CancellationToken cancellationToken = default)
	{
		try
		{
			count = Math.Clamp(count, 1, 50);
			var cacheKey = $"popular_courses_{count}";

			if (_cache.TryGetValue(cacheKey, out List<CourseCardDto>? cached) && cached != null)
				return cached;

			var enrollments = await _enrollmentRepository.GetAllAsync(cancellationToken);
			var popularCourseIds = enrollments
				.GroupBy(e => e.CourseId)
				.Select(g => new { CourseId = g.Key, Count = g.Count() })
				.OrderByDescending(x => x.Count)
				.Take(count)
				.Select(x => x.CourseId)
				.ToList();

			var result = new List<CourseCardDto>();
			foreach (var courseId in popularCourseIds)
			{
				var course = await GetCourseDetailAsync(courseId, cancellationToken);
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
						AverageRating = course.AverageRating
					});
				}
			}

			_cache.Set(cacheKey, result, new MemoryCacheEntryOptions
			{
				AbsoluteExpirationRelativeToNow = TimeSpan.FromHours(2),
				SlidingExpiration = TimeSpan.FromMinutes(15)
			});

			return result;
		}
		catch (Exception ex)
		{
			_logger.LogError(ex, "Error getting popular courses");
			return new List<CourseCardDto>();
		}
	}

	public async Task<IReadOnlyList<CourseCardDto>> GetRelatedCoursesAsync(Guid courseId, int count, CancellationToken cancellationToken = default)
	{
		try
		{
			count = Math.Clamp(count, 1, 10);
			var cacheKey = $"related_courses_{courseId}_{count}";

			if (_cache.TryGetValue(cacheKey, out List<CourseCardDto>? cached) && cached != null)
				return cached;

			var currentCourse = await _courseRepository.GetByIdAsync(courseId, cancellationToken);
			if (currentCourse == null) return new List<CourseCardDto>();

			var result = new List<CourseCardDto>();
			var addedIds = new HashSet<Guid> { courseId };

			// 1. Same category courses
			if (currentCourse.CategoryId != Guid.Empty)
			{
				var sameCategory = await _courseRepository.GetAsync(c =>
					c.CategoryId == currentCourse.CategoryId &&
					c.Id != courseId &&
					c.IsPublished, cancellationToken);

				foreach (var course in sameCategory.Take(count))
				{
					if (!addedIds.Contains(course.Id))
					{
						var dto = MapToCardDto(course);
						if (dto != null)
						{
							result.Add(dto);
							addedIds.Add(course.Id);
						}
					}
				}
			}

			// 2. Same instructor courses (if needed)
			if (result.Count < count && currentCourse.InstructorId != Guid.Empty)
			{
				var sameInstructor = await _courseRepository.GetAsync(c =>
					c.InstructorId == currentCourse.InstructorId &&
					c.Id != courseId &&
					c.IsPublished, cancellationToken);

				foreach (var course in sameInstructor.Take(count - result.Count))
				{
					if (!addedIds.Contains(course.Id))
					{
						var dto = MapToCardDto(course);
						if (dto != null)
						{
							result.Add(dto);
							addedIds.Add(course.Id);
						}
					}
				}
			}

			// 3. Latest courses as fallback
			if (result.Count < count)
			{
				var latest = await _courseRepository.GetAsync(c => c.IsPublished && !addedIds.Contains(c.Id), cancellationToken);
				foreach (var course in latest.OrderByDescending(c => c.CreatedDate).Take(count - result.Count))
				{
					var dto = MapToCardDto(course);
					if (dto != null) result.Add(dto);
				}
			}

			_cache.Set(cacheKey, result, new MemoryCacheEntryOptions
			{
				AbsoluteExpirationRelativeToNow = TimeSpan.FromHours(1),
				SlidingExpiration = TimeSpan.FromMinutes(10)
			});

			return result;
		}
		catch (Exception ex)
		{
			_logger.LogError(ex, "Error getting related courses for {CourseId}", courseId);
			return new List<CourseCardDto>();
		}
	}

	public async Task<PagedResultDto<CourseCardDto>> SearchCoursesAsync(
		string searchTerm, int page, int pageSize, CancellationToken cancellationToken = default)
	{
		return await GetPagedCoursesAsync(page, pageSize, null, searchTerm, null, cancellationToken);
	}

	public async Task<HomePageDto> GetHomePageDataAsync(CancellationToken cancellationToken = default)
	{
		try
		{
			var cacheKey = "homepage_data";

			if (_cache.TryGetValue(cacheKey, out HomePageDto? cached) && cached != null)
				return cached;

			var result = new HomePageDto
			{
				FeaturedCourses = (await GetFeaturedCoursesAsync(6, cancellationToken)).ToList(),
				LatestCourses = (await GetLatestCoursesAsync(6, cancellationToken)).ToList(),
				PopularCourses = (await GetPopularCoursesAsync(6, cancellationToken)).ToList(),
				Categories = await GetAllCategoriesWithChildrenAsync(cancellationToken),
				LatestArticles = await GetLatestArticlesAsync(3, cancellationToken),
				PopularArticles = await GetPopularArticlesAsync(3, cancellationToken),
				Stats = new HomeStatsDto
				{
					TotalCourses = await _courseRepository.CountAsync(c => c.IsPublished, cancellationToken),
					TotalInstructors = 0, // TODO: Implement instructor count
					TotalStudents = await _enrollmentRepository.CountAsync(null, cancellationToken),
					TotalArticles = await _articleRepository.CountAsync(a => a.IsPublished, cancellationToken)
				}
			};

			_cache.Set(cacheKey, result, new MemoryCacheEntryOptions
			{
				AbsoluteExpirationRelativeToNow = TimeSpan.FromMinutes(15),
				SlidingExpiration = TimeSpan.FromMinutes(5)
			});

			return result;
		}
		catch (Exception ex)
		{
			_logger.LogError(ex, "Error getting home page data");
			return new HomePageDto();
		}
	}

	public async Task<AuthResultDto> DeleteCourseByAdminAsync(Guid courseId, CancellationToken cancellationToken = default)
	{
		throw new NotImplementedException();
	}

	public async Task ClearCourseCacheAsync(Guid? courseId = null)
	{
		try
		{
			if (courseId.HasValue)
			{
				_cache.Remove($"course_detail_{courseId.Value}");
				_cache.Remove($"course_slug_*");
			}

			_cache.Remove("homepage_data");
			_cache.Remove("featured_courses_*");
			_cache.Remove("latest_courses_*");
			_cache.Remove("popular_courses_*");
			_cache.Remove("courses_page_*");

			_logger.LogInformation("Course cache cleared for {CourseId}", courseId ?? Guid.Empty);
		}
		catch (Exception ex)
		{
			_logger.LogError(ex, "Error clearing course cache");
		}
	}

	private async Task<List<CategoryDto>> GetAllCategoriesWithChildrenAsync(CancellationToken cancellationToken)
	{
		var categoryService = new CategoryService(_categoryRepository, _cache, _categoryLogger);
		return await categoryService.GetAllCategoriesWithChildrenAsync(cancellationToken);
	}

	private async Task<List<ArticleCardDto>> GetLatestArticlesAsync(int count, CancellationToken cancellationToken)
	{
		try
		{
			var articles = await _articleRepository.GetLatestPublishedAsync(count, cancellationToken);
			return articles.Select(a => new ArticleCardDto
			{
				Id = a.Id,
				Title = a.Title,
				Slug = a.Slug,
				Summary = a.Summary,
				ImageUrl = a.ImageUrl,
				PublishedAt = a.PublishedAt,
				AuthorName = a.Author?.FullName ?? "نویسنده",
				ViewCount = 0
			}).ToList();
		}
		catch (Exception ex)
		{
			_logger.LogError(ex, "Error getting latest articles");
			return new List<ArticleCardDto>();
		}
	}

	private async Task<List<ArticleCardDto>> GetPopularArticlesAsync(int count, CancellationToken cancellationToken)
	{
		try
		{
			var articles = await _articleRepository.GetLatestPublishedAsync(count, cancellationToken);
			return articles.Select(a => new ArticleCardDto
			{
				Id = a.Id,
				Title = a.Title,
				Slug = a.Slug,
				Summary = a.Summary,
				ImageUrl = a.ImageUrl,
				PublishedAt = a.PublishedAt,
				AuthorName = a.Author?.FullName ?? "نویسنده",
				ViewCount = 0
			}).ToList();
		}
		catch (Exception ex)
		{
			_logger.LogError(ex, "Error getting popular articles");
			return new List<ArticleCardDto>();
		}
	}

	private CourseCardDto? MapToCardDto(Course? course)
	{
		if (course == null) return null;

		try
		{
			return new CourseCardDto
			{
				Id = course.Id,
				Title = course.Title,
				Slug = course.Slug,
				ShortDescription = course.ShortDescription,
				ImageUrl = course.ImageUrl,
				Price = course.Price,
				DiscountPrice = course.DiscountPrice,
				InstructorName = course.Instructor?.FullName ?? "نامشخص",
				InstructorAvatar = course.Instructor?.AvatarUrl ?? string.Empty,
				CategoryName = course.Category?.Name ?? "دسته‌بندی نشده",
				EnrolledCount = course.Enrollments?.Count(e => !e.IsDeleted) ?? 0,
				AverageRating = course.Reviews?.Where(r => r.IsApproved).Average(r => r.Rating) ?? 0,
				ReviewCount = course.Reviews?.Count(r => r.IsApproved) ?? 0,
				StartDate = course.StartDate,
				IsPublished = course.IsPublished
			};
		}
		catch (Exception ex)
		{
			_logger.LogError(ex, "Error mapping course to DTO for ID: {CourseId}", course.Id);
			return null;
		}
	}

	// ============================= Instructor Methods =============================

	public async Task<PagedResultDto<CourseCardDto>> GetCoursesByInstructorAsync(
		Guid instructorId, int page, int pageSize, CancellationToken cancellationToken = default)
	{
		try
		{
			page = page < 1 ? 1 : page;
			pageSize = pageSize < 1 ? 10 : (pageSize > 100 ? 100 : pageSize);

			var courses = await _courseRepository.GetPagedAsync(page, pageSize,
				c => c.InstructorId == instructorId && !c.IsDeleted,
				q => q.OrderByDescending(c => c.CreatedDate),
				cancellationToken);

			var totalCount = await _courseRepository.CountAsync(c => c.InstructorId == instructorId && !c.IsDeleted, cancellationToken);

			return new PagedResultDto<CourseCardDto>
			{
				Items = courses.Select(c => MapToCardDto(c)).Where(dto => dto != null).ToList()!,
				TotalCount = totalCount,
				PageNumber = page,
				PageSize = pageSize
			};
		}
		catch (Exception ex)
		{
			_logger.LogError(ex, "Error getting courses by instructor {InstructorId}", instructorId);
			return new PagedResultDto<CourseCardDto>
			{
				Items = new List<CourseCardDto>(),
				TotalCount = 0,
				PageNumber = page,
				PageSize = pageSize
			};
		}
	}

	public async Task<CourseEditDto?> GetCourseForEditAsync(Guid courseId, Guid instructorId, CancellationToken cancellationToken = default)
	{
		try
		{
			var course = await _courseRepository.GetSingleAsync(c => c.Id == courseId && c.InstructorId == instructorId && !c.IsDeleted, cancellationToken);
			if (course == null) return null;

			return new CourseEditDto
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
				CourseType = course.CourseType,
				StartDate = course.StartDate,
				EndDate = course.EndDate,
				DurationHours = course.DurationHours,
				Capacity = course.Capacity,
				CategoryId = course.CategoryId,
				IsPublished = course.IsPublished,
				MetaTitle = course.MetaTitle,
				MetaDescription = course.MetaDescription
			};
		}
		catch (Exception ex)
		{
			_logger.LogError(ex, "Error getting course for edit {CourseId}", courseId);
			return null;
		}
	}

	public async Task<AuthResultDto> CreateCourseAsync(CreateCourseDto model, Guid instructorId, CancellationToken cancellationToken = default)
	{
		try
		{
			var slug = GenerateSlug(model.Title);

			// Check if slug exists
			var existingCourse = await _courseRepository.GetSingleAsync(c => c.Slug == slug, cancellationToken);
			if (existingCourse != null)
			{
				slug = $"{slug}-{Guid.NewGuid().ToString()[..8]}";
			}

			var course = new Course
			{
				Id = Guid.NewGuid(),
				Title = model.Title,
				Slug = slug,
				ShortDescription = model.ShortDescription,
				Description = model.Description,
				Price = model.Price,
				DiscountPrice = model.DiscountPrice,
				ImageUrl = model.ImageUrl,
				IntroVideoUrl = model.IntroVideoUrl,
				CourseType = model.CourseType,
				StartDate = model.StartDate,
				EndDate = model.EndDate,
				DurationHours = model.DurationHours,
				Capacity = model.Capacity,
				CategoryId = model.CategoryId,
				InstructorId = instructorId,
				IsPublished = false, // نیاز به تأیید ادمین
				IsFeatured = false,
				MetaTitle = model.MetaTitle,
				MetaDescription = model.MetaDescription,
				CreatedDate = DateTime.UtcNow,
				LastModifiedDate = DateTime.UtcNow
			};

			await _courseRepository.AddAsync(course, cancellationToken);

			_logger.LogInformation("Course created: {Title} by instructor {InstructorId}", model.Title, instructorId);

			return new AuthResultDto
			{
				Succeeded = true,
				Message = "دوره با موفقیت ایجاد شد. پس از تأیید ادمین منتشر خواهد شد."
			};
		}
		catch (Exception ex)
		{
			_logger.LogError(ex, "Error creating course by instructor {InstructorId}", instructorId);
			return new AuthResultDto
			{
				Succeeded = false,
				Message = "خطا در ایجاد دوره"
			};
		}
	}

	public async Task<AuthResultDto> UpdateCourseAsync(UpdateCourseDto model, Guid instructorId, CancellationToken cancellationToken = default)
	{
		try
		{
			var course = await _courseRepository.GetSingleAsync(c => c.Id == model.Id && c.InstructorId == instructorId && !c.IsDeleted, cancellationToken);
			if (course == null)
			{
				return new AuthResultDto
				{
					Succeeded = false,
					Message = "دوره یافت نشد"
				};
			}

			// Update slug if title changed
			if (course.Title != model.Title)
			{
				var newSlug = GenerateSlug(model.Title);
				var existingCourse = await _courseRepository.GetSingleAsync(c => c.Slug == newSlug && c.Id != model.Id, cancellationToken);
				if (existingCourse != null)
				{
					newSlug = $"{newSlug}-{Guid.NewGuid().ToString()[..8]}";
				}
				course.Slug = newSlug;
			}

			course.Title = model.Title;
			course.ShortDescription = model.ShortDescription;
			course.Description = model.Description;
			course.Price = model.Price;
			course.DiscountPrice = model.DiscountPrice;
			course.ImageUrl = model.ImageUrl;
			course.IntroVideoUrl = model.IntroVideoUrl;
			course.CourseType = model.CourseType;
			course.StartDate = model.StartDate;
			course.EndDate = model.EndDate;
			course.DurationHours = model.DurationHours;
			course.Capacity = model.Capacity;
			course.CategoryId = model.CategoryId;
			course.MetaTitle = model.MetaTitle;
			course.MetaDescription = model.MetaDescription;
			course.LastModifiedDate = DateTime.UtcNow;

			// اگر قبلاً منتشر نشده بود و ادمین تأیید نکرده بود، همچنان منتشر نشده بماند
			if (course.IsPublished == false)
			{
				course.IsPublished = false;
			}

			await _courseRepository.UpdateAsync(course, cancellationToken);
			await ClearCourseCacheAsync(course.Id);

			_logger.LogInformation("Course updated: {Title} by instructor {InstructorId}", model.Title, instructorId);

			return new AuthResultDto
			{
				Succeeded = true,
				Message = "دوره با موفقیت بروزرسانی شد"
			};
		}
		catch (Exception ex)
		{
			_logger.LogError(ex, "Error updating course {CourseId} by instructor {InstructorId}", model.Id, instructorId);
			return new AuthResultDto
			{
				Succeeded = false,
				Message = "خطا در بروزرسانی دوره"
			};
		}
	}

	public async Task<AuthResultDto> DeleteCourseAsync(Guid courseId, Guid instructorId, CancellationToken cancellationToken = default)
	{
		try
		{
			var course = await _courseRepository.GetSingleAsync(c => c.Id == courseId && c.InstructorId == instructorId && !c.IsDeleted, cancellationToken);
			if (course == null)
			{
				return new AuthResultDto
				{
					Succeeded = false,
					Message = "دوره یافت نشد"
				};
			}

			await _courseRepository.DeleteAsync(course, cancellationToken);
			await ClearCourseCacheAsync(courseId);

			_logger.LogInformation("Course deleted: {CourseId} by instructor {InstructorId}", courseId, instructorId);

			return new AuthResultDto
			{
				Succeeded = true,
				Message = "دوره با موفقیت حذف شد"
			};
		}
		catch (Exception ex)
		{
			_logger.LogError(ex, "Error deleting course {CourseId} by instructor {InstructorId}", courseId, instructorId);
			return new AuthResultDto
			{
				Succeeded = false,
				Message = "خطا در حذف دوره"
			};
		}
	}

	// ============================= Syllabus Management =============================

	public async Task<CourseDetailDto?> GetCourseWithSyllabusAsync(Guid courseId, Guid instructorId, CancellationToken cancellationToken = default)
	{
		try
		{
			var course = await _courseRepository.GetSingleAsync(c => c.Id == courseId && c.InstructorId == instructorId && !c.IsDeleted, cancellationToken);
			if (course == null) return null;

			var syllabuses = await _courseRepository.GetSyllabusByCourseIdAsync(courseId, cancellationToken);

			return new CourseDetailDto
			{
				Id = course.Id,
				Title = course.Title,
				Slug = course.Slug,
				ShortDescription = course.ShortDescription,
				Description = course.Description,
				ImageUrl = course.ImageUrl,
				Price = course.Price,
				DiscountPrice = course.DiscountPrice,
				InstructorName = course.Instructor?.FullName ?? "",
				CategoryName = course.Category?.Name ?? "",
				DurationHours = course.DurationHours,
				Capacity = course.Capacity,
				StartDate = course.StartDate,
				CourseType = course.CourseType.ToString(),
				Syllabuses = syllabuses.Select(s => new SyllabusDto
				{
					Id = s.Id,
					Title = s.Title,
					Description = s.Description,
					Order = s.Order,
					Lessons = s.Lessons.OrderBy(l => l.Order).Select(l => new LessonDto
					{
						Id = l.Id,
						Title = l.Title,
						DurationMinutes = l.DurationMinutes,
						IsPreview = l.IsPreview,
						VideoUrl = l.VideoUrl,
						Order = l.Order
					}).ToList()
				}).ToList()
			};
		}
		catch (Exception ex)
		{
			_logger.LogError(ex, "Error getting course with syllabus {CourseId}", courseId);
			return null;
		}
	}

	public async Task<SyllabusDetailDto?> GetSyllabusWithLessonsAsync(Guid syllabusId, Guid instructorId, CancellationToken cancellationToken = default)
	{
		try
		{
			var syllabus =
				await _courseRepository.GetSyllabusWithLessonsAsync(syllabusId, instructorId, cancellationToken);
			if (syllabus == null) return null;

			return new SyllabusDetailDto
			{
				Id = syllabus.Id,
				Title = syllabus.Title,
				Description = syllabus.Description,
				Order = syllabus.Order,
				CourseId = syllabus.CourseId,
				CourseTitle = syllabus.Course?.Title ?? "",
				Lessons = syllabus.Lessons.OrderBy(l => l.Order).Select(l => new LessonDetailDto
				{
					Id = l.Id,
					Title = l.Title,
					Content = l.Content,
					VideoUrl = l.VideoUrl,
					DurationMinutes = l.DurationMinutes,
					IsPreview = l.IsPreview,
					Order = l.Order
				}).ToList()
			};
		}
		catch (Exception ex)
		{
			_logger.LogError(ex, "Error getting syllabus with lessons {SyllabusId}", syllabusId);
			return null;
		}
	}

	public async Task<AuthResultDto> AddSyllabusAsync(AddSyllabusDto model, Guid instructorId, CancellationToken cancellationToken = default)
	{
		try
		{
			// Check if course belongs to instructor
			var course = await _courseRepository.GetSingleAsync(c => c.Id == model.CourseId && c.InstructorId == instructorId && !c.IsDeleted, cancellationToken);
			if (course == null)
			{
				return new AuthResultDto
				{
					Succeeded = false,
					Message = "دوره یافت نشد"
				};
			}

			var maxOrder = await _courseRepository.GetMaxOrderInSyllabus(model.CourseId);

			var syllabus = new Syllabus
			{
				Id = Guid.NewGuid(),
				Title = model.Title,
				Description = model.Description,
				Order = maxOrder + 1,
				CourseId = model.CourseId,
				CreatedDate = DateTime.UtcNow,
				LastModifiedDate = DateTime.UtcNow
			};

			await _syllabusRepository.AddAsync(syllabus, cancellationToken);
			await ClearCourseCacheAsync(model.CourseId);

			return new AuthResultDto
			{
				Succeeded = true,
				Message = "سرفصل با موفقیت اضافه شد",
				Data = syllabus.Id
			};
		}
		catch (Exception ex)
		{
			_logger.LogError(ex, "Error adding syllabus to course {CourseId}", model.CourseId);
			return new AuthResultDto
			{
				Succeeded = false,
				Message = "خطا در افزودن سرفصل"
			};
		}
	}

	public async Task<AuthResultDto> UpdateSyllabusAsync(UpdateSyllabusDto model, Guid instructorId, CancellationToken cancellationToken = default)
	{
		try
		{
			var syllabus =
				await _courseRepository.GetSyllabusWithLessonsAsync(model.Id, instructorId, cancellationToken);


			if (syllabus == null)
			{
				return new AuthResultDto
				{
					Succeeded = false,
					Message = "سرفصل یافت نشد"
				};
			}

			syllabus.Title = model.Title;
			syllabus.Description = model.Description;

			await _syllabusRepository.UpdateAsync(syllabus, cancellationToken);
			await ClearCourseCacheAsync(syllabus.CourseId);

			return new AuthResultDto
			{
				Succeeded = true,
				Message = "سرفصل با موفقیت بروزرسانی شد"
			};
		}
		catch (Exception ex)
		{
			_logger.LogError(ex, "Error updating syllabus {SyllabusId}", model.Id);
			return new AuthResultDto
			{
				Succeeded = false,
				Message = "خطا در بروزرسانی سرفصل"
			};
		}
	}

	public async Task<AuthResultDto> DeleteSyllabusAsync(Guid syllabusId, Guid instructorId, CancellationToken cancellationToken = default)
	{
		try
		{
			var syllabus =
				await _courseRepository.GetSyllabusWithLessonsAsync(syllabusId, instructorId, cancellationToken);

			if (syllabus == null)
			{
				return new AuthResultDto
				{
					Succeeded = false,
					Message = "سرفصل یافت نشد"
				};
			}

			syllabus.IsDeleted = true;
			syllabus.LastModifiedDate = DateTime.UtcNow;

			// Also soft delete lessons
			var lessons = await _courseRepository.GetLessonsBySyllabusId(syllabusId, cancellationToken);

			foreach (var lesson in lessons)
			{
				await _lessonRepository.DeleteAsync(lesson, cancellationToken);
			}

			await ClearCourseCacheAsync(syllabus.CourseId);

			return new AuthResultDto
			{
				Succeeded = true,
				Message = "سرفصل با موفقیت حذف شد"
			};
		}
		catch (Exception ex)
		{
			_logger.LogError(ex, "Error deleting syllabus {SyllabusId}", syllabusId);
			return new AuthResultDto
			{
				Succeeded = false,
				Message = "خطا در حذف سرفصل"
			};
		}
	}

	public async Task<AuthResultDto> ReorderSyllabusAsync(List<Guid> syllabusIds, Guid instructorId, CancellationToken cancellationToken = default)
	{
		try
		{
			if (syllabusIds == null || !syllabusIds.Any())
			{
				return new AuthResultDto
				{
					Succeeded = false,
					Message = "لیست سرفصل‌ها خالی است"
				};
			}

			// Get first syllabus to check course ownership
			Guid firstSyllabusId = syllabusIds.First();
			var firstSyllabus = await _courseRepository.GetSyllabusById(firstSyllabusId, cancellationToken);
			if (firstSyllabus == null || firstSyllabus.Course.InstructorId != instructorId)
			{
				return new AuthResultDto
				{
					Succeeded = false,
					Message = "دسترسی غیرمجاز"
				};
			}

			for (int i = 0; i < syllabusIds.Count; i++)
			{
				var syllabus = await _syllabusRepository.GetByIdAsync(syllabusIds[i], cancellationToken);
				if (syllabus != null && !syllabus.IsDeleted)
				{
					syllabus.Order = i + 1;
					syllabus.LastModifiedDate = DateTime.UtcNow;
					await _syllabusRepository.UpdateAsync(syllabus, cancellationToken);
				}

			}
			await ClearCourseCacheAsync(firstSyllabus.CourseId);

			return new AuthResultDto
			{
				Succeeded = true,
				Message = "ترتیب سرفصل‌ها با موفقیت ذخیره شد"
			};
		}
		catch (Exception ex)
		{
			_logger.LogError(ex, "Error reordering syllabuses");
			return new AuthResultDto
			{
				Succeeded = false,
				Message = "خطا در تغییر ترتیب سرفصل‌ها"
			};
		}
	}

	// ============================= Lesson Management =============================

	public async Task<AuthResultDto> AddLessonAsync(AddLessonDto model, Guid instructorId, CancellationToken cancellationToken = default)
	{
		try
		{
			// Check if syllabus belongs to instructor's course
			var syllabus =
				await _courseRepository.GetSyllabusWithLessonsAsync(model.SyllabusId, instructorId, cancellationToken);
			if (syllabus == null)
			{
				return new AuthResultDto
				{
					Succeeded = false,
					Message = "سرفصل یافت نشد"
				};
			}

			var maxOrder = await _courseRepository.GetMaxOrderInLesson(model.SyllabusId);


			var lesson = new Lesson
			{
				Id = Guid.NewGuid(),
				Title = model.Title,
				Content = model.Content,
				VideoUrl = model.VideoUrl,
				DurationMinutes = model.DurationMinutes,
				IsPreview = model.IsPreview,
				Order = maxOrder + 1,
				SyllabusId = model.SyllabusId,
				CourseId = syllabus.CourseId,
				CreatedDate = DateTime.UtcNow,
				LastModifiedDate = DateTime.UtcNow
			};
			await _lessonRepository.AddAsync(lesson, cancellationToken);
			await ClearCourseCacheAsync(syllabus.CourseId);

			return new AuthResultDto
			{
				Succeeded = true,
				Message = "جلسه با موفقیت اضافه شد",
				Data = lesson.Id
			};
		}
		catch (Exception ex)
		{
			_logger.LogError(ex, "Error adding lesson to syllabus {SyllabusId}", model.SyllabusId);
			return new AuthResultDto
			{
				Succeeded = false,
				Message = "خطا در افزودن جلسه"
			};
		}
	}

	public async Task<AuthResultDto> UpdateLessonAsync(UpdateLessonDto model, Guid instructorId, CancellationToken cancellationToken = default)
	{
		try
		{
			var lesson = await _courseRepository
				.GetLessonsByInstructorIdAndLessonIdAsync(model.Id, instructorId);
			if (lesson == null)
			{
				return new AuthResultDto
				{
					Succeeded = false,
					Message = "جلسه یافت نشد"
				};
			}

			lesson.Title = model.Title;
			lesson.Content = model.Content;
			lesson.VideoUrl = model.VideoUrl;
			lesson.DurationMinutes = model.DurationMinutes;
			lesson.IsPreview = model.IsPreview;

			await _lessonRepository.UpdateAsync(lesson, cancellationToken);
			await ClearCourseCacheAsync(lesson.Syllabus.CourseId);

			return new AuthResultDto
			{
				Succeeded = true,
				Message = "جلسه با موفقیت بروزرسانی شد"
			};
		}
		catch (Exception ex)
		{
			_logger.LogError(ex, "Error updating lesson {LessonId}", model.Id);
			return new AuthResultDto
			{
				Succeeded = false,
				Message = "خطا در بروزرسانی جلسه"
			};
		}
	}

	public async Task<AuthResultDto> DeleteLessonAsync(Guid lessonId, Guid instructorId, CancellationToken cancellationToken = default)
	{
		try
		{
			var lesson = await _courseRepository.GetLessonsByInstructorIdAndLessonIdAsync(lessonId, instructorId);


			if (lesson == null)
			{
				return new AuthResultDto
				{
					Succeeded = false,
					Message = "جلسه یافت نشد"
				};
			}

			await _lessonRepository.DeleteAsync(lesson,cancellationToken);
			await ClearCourseCacheAsync(lesson.Syllabus.CourseId);

			return new AuthResultDto
			{
				Succeeded = true,
				Message = "جلسه با موفقیت حذف شد"
			};
		}
		catch (Exception ex)
		{
			_logger.LogError(ex, "Error deleting lesson {LessonId}", lessonId);
			return new AuthResultDto
			{
				Succeeded = false,
				Message = "خطا در حذف جلسه"
			};
		}
	}

	public async Task<AuthResultDto> ReorderLessonsAsync(List<Guid> lessonIds, Guid instructorId, CancellationToken cancellationToken = default)
	{
		try
		{
			if (lessonIds == null || !lessonIds.Any())
			{
				return new AuthResultDto
				{
					Succeeded = false,
					Message = "لیست جلسات خالی است"
				};
			}

			// Get first lesson to check ownership
			var firstLesson =
				await _courseRepository
					.GetLessonsByInstructorIdAndLessonIdAsync(lessonIds.First(), instructorId);

			if (firstLesson == null || firstLesson.Syllabus.Course.InstructorId != instructorId)
			{
				return new AuthResultDto
				{
					Succeeded = false,
					Message = "دسترسی غیرمجاز"
				};
			}

			for (int i = 0; i < lessonIds.Count; i++)
			{
				var lesson = await _lessonRepository.GetByIdAsync(lessonIds[i],cancellationToken);
				if (lesson != null && !lesson.IsDeleted)
				{
					lesson.Order = i + 1;
					lesson.LastModifiedDate = DateTime.UtcNow;
					await _lessonRepository.UpdateAsync(lesson,cancellationToken);

				}
			}

			await ClearCourseCacheAsync(firstLesson.Syllabus.CourseId);

			return new AuthResultDto
			{
				Succeeded = true,
				Message = "ترتیب جلسات با موفقیت ذخیره شد"
			};
		}
		catch (Exception ex)
		{
			_logger.LogError(ex, "Error reordering lessons");
			return new AuthResultDto
			{
				Succeeded = false,
				Message = "خطا در تغییر ترتیب جلسات"
			};
		}
	}

	// ============================= Statistics =============================

	public async Task<CourseStatisticsDto?> GetCourseStatisticsAsync(Guid courseId, Guid instructorId, CancellationToken cancellationToken = default)
	{
		try
		{
			var course = await _courseRepository.GetSingleAsync(c => c.Id == courseId && c.InstructorId == instructorId && !c.IsDeleted, cancellationToken);
			if (course == null) return null;

			var enrollments = await _enrollmentRepository.GetAsync(e => e.CourseId == courseId && !e.IsDeleted, cancellationToken);
			var reviews = await _courseRepository.GetCourseReviewsByCourseIdAsync(courseId, cancellationToken);

			var totalRevenue = enrollments.Sum(e => course.DiscountPrice ?? course.Price);
			var averageRating = reviews.Any() ? reviews.Average(r => r.Rating) : 0;

			var monthlyEnrollments = enrollments
				.GroupBy(e => new { e.EnrolledAt.Year, e.EnrolledAt.Month })
				.Select(g => new MonthlyEnrollmentDto
				{
					Month = $"{g.Key.Year}/{g.Key.Month}",
					Count = g.Count()
				})
				.OrderBy(m => m.Month)
				.Take(12)
				.ToList();

			return new CourseStatisticsDto
			{
				CourseId = course.Id,
				CourseTitle = course.Title,
				TotalEnrollments = enrollments.Count,
				TotalStudents = enrollments.Count,
				TotalRevenue = totalRevenue,
				AverageRating = averageRating,
				TotalReviews = reviews.Count,
				Capacity = course.Capacity,
				RemainingCapacity = course.Capacity - enrollments.Count,
				MonthlyEnrollments = monthlyEnrollments
			};
		}
		catch (Exception ex)
		{
			_logger.LogError(ex, "Error getting course statistics for {CourseId}", courseId);
			return null;
		}
	}



	// ============================= Helper Methods =============================

	private string GenerateSlug(string text)
	{
		if (string.IsNullOrEmpty(text)) return string.Empty;

		var slug = text.Trim().ToLower();
		slug = slug.Replace(" ", "-");
		slug = slug.Replace("ي", "ی");
		slug = slug.Replace("ك", "ک");

		// Remove invalid characters
		slug = System.Text.RegularExpressions.Regex.Replace(slug, @"[^a-z0-9\u0600-\u06FF-]", "");
		slug = System.Text.RegularExpressions.Regex.Replace(slug, @"-{2,}", "-");
		slug = slug.Trim('-');

		return slug;
	}
}