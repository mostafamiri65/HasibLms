using HasibLms.Domain.Entities.AcademyEntities;
using HasibLms.Domain.Interfaces;
using HasibLms.Persistence.Data;
using Microsoft.EntityFrameworkCore;

namespace HasibLms.Persistence.Repositories;

public class CourseRepository : GenericRepository<Course>, ICourseRepository
{
	public CourseRepository(LmsContext context) : base(context)
	{
	}
    public async Task<IReadOnlyList<Course>> GetPagedCoursesByInstructorAsync(
        Guid instructorId, int page, int pageSize, CancellationToken cancellationToken = default)
    {
        return await _dbSet
            .Where(c => !c.IsDeleted && c.InstructorId == instructorId)
            .Include(c => c.Category)
            .Include(c => c.Instructor)
            .Include(c => c.Enrollments)
            .Include(c => c.Reviews.Where(r => r.IsApproved))
            .OrderByDescending(c => c.CreatedDate)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync(cancellationToken);
    }

    public async Task<Course?> GetCourseBySlug(string slug)
    {
        return await _dbSet
             .FirstOrDefaultAsync(x => x.Slug == slug && x.IsPublished);
    }

    public async Task<Course?> GetCourseWithDetailsAsync(Guid id, CancellationToken cancellationToken = default)
	{
		return await _dbSet
			.Include(c => c.Category)
			.Include(c => c.Instructor)
			.Include(c => c.Syllabuses)
				.ThenInclude(s => s.Lessons)
			.Include(c => c.Reviews.Where(r => r.IsApproved))
				.ThenInclude(r => r.Student)
			.FirstOrDefaultAsync(c => c.Id == id && !c.IsDeleted && c.IsPublished, cancellationToken);
	}

	public async Task<IReadOnlyList<Course>> GetFeaturedCoursesAsync(int count, CancellationToken cancellationToken = default)
	{
		return await _dbSet
			.Where(c => !c.IsDeleted && c.IsPublished && c.IsFeatured)
			.Include(c => c.Category)
			.Include(c => c.Instructor)
			.OrderByDescending(c => c.CreatedDate)
			.Take(count)
			.ToListAsync(cancellationToken);
	}

	public async Task<IReadOnlyList<Course>> GetCoursesByCategoryAsync(Guid categoryId, int page, int pageSize, CancellationToken cancellationToken = default)
	{
		return await _dbSet
			.Where(c => !c.IsDeleted && c.IsPublished && c.CategoryId == categoryId)
			.Include(c => c.Category)
			.Include(c => c.Instructor)
			.OrderByDescending(c => c.CreatedDate)
			.Skip((page - 1) * pageSize)
			.Take(pageSize)
			.ToListAsync(cancellationToken);
	}

	public async Task<bool> IsUserEnrolledAsync(Guid courseId, Guid userId, CancellationToken cancellationToken = default)
	{
		return await _context.Enrollments
			.AnyAsync(e => e.CourseId == courseId && e.StudentId == userId && !e.IsDeleted, cancellationToken);
	}

	// پیاده‌سازی متد GetPagedCoursesAsync با فیلترهای پیشرفته
	public async Task<IReadOnlyList<Course>> GetPagedCoursesAsync(
		int page, int pageSize, Guid? categoryId = null, string? searchTerm = null,
		string? sortBy = null, CancellationToken cancellationToken = default)
	{
		// شروع کوئری با شرط پایه
		IQueryable<Course> query = _dbSet
			.Where(c => !c.IsDeleted && c.IsPublished)
            .Include(c => c.Category)
            .Include(c => c.Instructor)
            .Include(c => c.Syllabuses)
            .ThenInclude(s => s.Lessons)
            .Include(c => c.Reviews.Where(r => r.IsApproved))
            .ThenInclude(r => r.Student);

		// فیلتر بر اساس دسته‌بندی
		if (categoryId.HasValue && categoryId.Value != Guid.Empty)
		{
			query = query.Where(c => c.CategoryId == categoryId.Value);
		}

		// فیلتر بر اساس جستجو
		if (!string.IsNullOrEmpty(searchTerm))
		{
			searchTerm = searchTerm.Trim();
			query = query.Where(c =>
				c.Title.Contains(searchTerm) ||
				c.ShortDescription.Contains(searchTerm) ||
				c.Description.Contains(searchTerm) ||
				(c.Instructor != null && c.Instructor.FullName.Contains(searchTerm)));
		}

		// مرتب‌سازی
		query = (sortBy?.ToLower()) switch
		{
			"price_asc" => query.OrderBy(c => c.DiscountPrice ?? c.Price),
			"price_desc" => query.OrderByDescending(c => c.DiscountPrice ?? c.Price),
			"popular" => query.OrderByDescending(c => c.Enrollments.Count),
			"rating" => query.OrderByDescending(c => c.Reviews.Where(r => r.IsApproved).Average(r => r.Rating)),
			"title_asc" => query.OrderBy(c => c.Title),
			"title_desc" => query.OrderByDescending(c => c.Title),
			"start_date_asc" => query.OrderBy(c => c.StartDate),
			"start_date_desc" => query.OrderByDescending(c => c.StartDate),
			_ => query.OrderByDescending(c => c.CreatedDate) // latest
		};

		// صفحه‌بندی
		return await query
			.Skip((page - 1) * pageSize)
			.Take(pageSize)
			.ToListAsync(cancellationToken);
	}

	public async Task<List<Syllabus>> GetSyllabusByCourseIdAsync(Guid courseId, CancellationToken cancellationToken = default)
	{
		return await _context.Syllabuses
			.Where(s => s.CourseId == courseId && !s.IsDeleted)
			.Include(s => s.Lessons.Where(l => !l.IsDeleted))
			.OrderBy(s => s.Order)
			.ToListAsync(cancellationToken);
	}

	public async Task<Syllabus?> GetSyllabusWithLessonsAsync(Guid syllabusId, Guid instructorId, CancellationToken cancellationToken = default)
	{
		return await _context.Syllabuses
			.Include(s => s.Course)
			.Include(s => s.Lessons.Where(l => !l.IsDeleted))
			.FirstOrDefaultAsync(s => s.Id == syllabusId && !s.IsDeleted && s.Course.InstructorId == instructorId, cancellationToken);

	}

	public async Task<List<Lesson>> GetLessonsBySyllabusId(Guid syllabusId, CancellationToken cancellationToken = default)
	{
		return await _context.Lessons
			.Where(l => l.SyllabusId == syllabusId && !l.IsDeleted)
			.ToListAsync(cancellationToken);
	}

	public async Task<int> GetMaxOrderInSyllabus(Guid courseId)
	{
		return await _context.Syllabuses
			.Where(s => s.CourseId == courseId && !s.IsDeleted)
			.MaxAsync(s => (int?)s.Order) ?? 0;
	}

	public async Task<int> GetMaxOrderInLesson(Guid syllabusId)
	{
		return await _context.Lessons
			.Where(l => l.SyllabusId == syllabusId && !l.IsDeleted)
			.MaxAsync(l => (int?)l.Order) ?? 0;
	}


	public async Task<Syllabus?> GetSyllabusById(Guid syllabusId, CancellationToken cancellationToken = default)
	{
		return await _context.Syllabuses
			.Include(s => s.Course)
			.FirstOrDefaultAsync(s => s.Id == syllabusId && !s.IsDeleted, cancellationToken);

	}

	public async Task<Lesson?> GetLessonsByInstructorIdAndLessonIdAsync(Guid lessonId, Guid instructorId)
	{
		return await _context.Lessons
				.Include(l => l.Syllabus)
				.ThenInclude(s => s.Course)
				.FirstOrDefaultAsync(l => l.Id == lessonId && !l.IsDeleted && l.Syllabus.Course.InstructorId == instructorId);

	}

	public async Task<List<CourseReview>> GetCourseReviewsByCourseIdAsync(Guid courseId, CancellationToken cancellationToken = default)
	{
		return await _context.CourseReviews
			.Where(r => r.CourseId == courseId && r.IsApproved && !r.IsDeleted)
			.ToListAsync(cancellationToken);
	}
}