using HasibLms.Domain.Entities;
using HasibLms.Domain.Entities.AcademyEntities;
using HasibLms.Domain.Entities.InvestigativeEntities;
using HasibLms.Domain.Entities.Settings;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;

namespace HasibLms.Persistence.Data;

public class LmsContext : IdentityDbContext<User, IdentityRole<Guid>, Guid>
{
	public LmsContext(DbContextOptions<LmsContext> options) : base(options)
	{

	}


	public DbSet<Category> Categories { get; set; }
	public DbSet<Course> Courses { get; set; }
	public DbSet<Syllabus> Syllabuses { get; set; }
	public DbSet<Lesson> Lessons { get; set; }
	public DbSet<Enrollment> Enrollments { get; set; }
	public DbSet<CourseReview> CourseReviews { get; set; }
	public DbSet<Article> Articles { get; set; }
	public DbSet<Tag> Tags { get; set; }
	public DbSet<ArticleTag> ArticleTags { get; set; }
	public DbSet<SiteSetting> SiteSettings { get; set; }
	public DbSet<InstructorRequest> InstructorRequests { get; set; }
	protected override void OnModelCreating(ModelBuilder builder)
	{
		base.OnModelCreating(builder);

		// ═══════════════ Category ═══════════════
		builder.Entity<Category>(entity =>
		{
			entity.HasKey(c => c.Id);
			entity.Property(c => c.Name).HasMaxLength(200).IsRequired();
			entity.Property(c => c.Slug).HasMaxLength(300).IsRequired();
			entity.HasIndex(c => c.Slug).IsUnique();

			entity.HasOne(c => c.Parent)
				  .WithMany(c => c.Children)
				  .HasForeignKey(c => c.ParentId)
				  .OnDelete(DeleteBehavior.Restrict);
		});

		// ═══════════════ Course ═══════════════
		builder.Entity<Course>(entity =>
		{
			entity.HasKey(c => c.Id);
			entity.Property(c => c.Title).HasMaxLength(300).IsRequired();
			entity.Property(c => c.Slug).HasMaxLength(400).IsRequired();
			entity.HasIndex(c => c.Slug).IsUnique();
			entity.Property(c => c.ShortDescription).HasMaxLength(500);
			entity.Property(c => c.Price).HasColumnType("decimal(18,2)");
			entity.Property(c => c.DiscountPrice).HasColumnType("decimal(18,2)");

			// SEO indexes
			entity.HasIndex(c => c.IsPublished);
			entity.HasIndex(c => c.IsFeatured);
			entity.HasIndex(c => c.CategoryId);
			entity.HasIndex(c => c.InstructorId);

			entity.HasOne(c => c.Instructor)
				  .WithMany(u => u.TaughtCourses)
				  .HasForeignKey(c => c.InstructorId)
				  .OnDelete(DeleteBehavior.Restrict);

			entity.HasOne(c => c.Category)
				  .WithMany(cat => cat.Courses)
				  .HasForeignKey(c => c.CategoryId)
				  .OnDelete(DeleteBehavior.Restrict);
		});

		// ═══════════════ Syllabus ═══════════════
		builder.Entity<Syllabus>(entity =>
		{
			entity.HasKey(s => s.Id);
			entity.Property(s => s.Title).HasMaxLength(300).IsRequired();

			entity.HasOne(s => s.Course)
				  .WithMany(c => c.Syllabuses)
				  .HasForeignKey(s => s.CourseId)
				  .OnDelete(DeleteBehavior.Cascade);
		});

		// ═══════════════ Lesson ═══════════════
		builder.Entity<Lesson>(entity =>
		{
			entity.HasKey(l => l.Id);
			entity.Property(l => l.Title).HasMaxLength(300).IsRequired();

			entity.HasOne(l => l.Syllabus)
				  .WithMany(s => s.Lessons)
				  .HasForeignKey(l => l.SyllabusId)
				  .OnDelete(DeleteBehavior.Cascade);

			entity.HasOne(l => l.Course)
				  .WithMany(c => c.Lessons)
				  .HasForeignKey(l => l.CourseId)
				  .OnDelete(DeleteBehavior.NoAction);
		});

		// ═══════════════ Enrollment ═══════════════
		builder.Entity<Enrollment>(entity =>
		{
			entity.HasKey(e => e.Id);

			entity.HasOne(e => e.Student)
				  .WithMany(u => u.Enrollments)
				  .HasForeignKey(e => e.StudentId)
				  .OnDelete(DeleteBehavior.Restrict);

			entity.HasOne(e => e.Course)
				  .WithMany(c => c.Enrollments)
				  .HasForeignKey(e => e.CourseId)
				  .OnDelete(DeleteBehavior.Cascade);

			entity.HasIndex(e => new { e.StudentId, e.CourseId }).IsUnique();
		});

		// ═══════════════ CourseReview ═══════════════
		builder.Entity<CourseReview>(entity =>
		{
			entity.HasKey(r => r.Id);
			entity.Property(r => r.Rating).IsRequired();

			entity.HasOne(r => r.Course)
				  .WithMany(c => c.Reviews)
				  .HasForeignKey(r => r.CourseId)
				  .OnDelete(DeleteBehavior.Cascade);

			entity.HasOne(r => r.Student)
				  .WithMany(u => u.Reviews)
				  .HasForeignKey(r => r.StudentId)
				  .OnDelete(DeleteBehavior.Restrict);

			entity.HasIndex(r => r.IsApproved);
		});

		// ═══════════════ Article ═══════════════
		builder.Entity<Article>(entity =>
		{
			entity.HasKey(a => a.Id);
			entity.Property(a => a.Title).HasMaxLength(300).IsRequired();
			entity.Property(a => a.Slug).HasMaxLength(400).IsRequired();
			entity.HasIndex(a => a.Slug).IsUnique();
			entity.Property(a => a.Summary).HasMaxLength(500);

			entity.HasIndex(a => a.IsPublished);
			entity.HasIndex(a => a.ArticleType);

			entity.HasOne(a => a.Author)
				  .WithMany(u => u.Articles)
				  .HasForeignKey(a => a.AuthorId)
				  .OnDelete(DeleteBehavior.Restrict);
		});

		// ═══════════════ Tag ═══════════════
		builder.Entity<Tag>(entity =>
		{
			entity.HasKey(t => t.Id);
			entity.Property(t => t.Name).HasMaxLength(100).IsRequired();
			entity.Property(t => t.Slug).HasMaxLength(150).IsRequired();
			entity.HasIndex(t => t.Slug).IsUnique();
		});

		// ═══════════════ ArticleTag (Many-to-Many) ═══════════════
		builder.Entity<ArticleTag>(entity =>
		{
			entity.HasKey(at => new { at.ArticleId, at.TagId });

			entity.HasOne(at => at.Article)
				  .WithMany(a => a.ArticleTags)
				  .HasForeignKey(at => at.ArticleId)
				  .OnDelete(DeleteBehavior.Cascade);

			entity.HasOne(at => at.Tag)
				  .WithMany(t => t.ArticleTags)
				  .HasForeignKey(at => at.TagId)
				  .OnDelete(DeleteBehavior.Cascade);
		});

		// ═══════════════ User (Identity جداول) ═══════════════
		builder.Entity<User>(entity =>
		{
			entity.Property(u => u.FullName).HasMaxLength(150).IsRequired();
			entity.Property(u => u.Bio).HasMaxLength(1000);
		});

		// ═══════════════ InstructorRequest ═══════════════
		builder.Entity<InstructorRequest>(entity =>
		{
			entity.HasKey(r => r.Id);
			entity.Property(r => r.FullName).HasMaxLength(200).IsRequired();
			entity.Property(r => r.Email).HasMaxLength(200).IsRequired();
			entity.Property(r => r.Degree).HasMaxLength(100);
			entity.Property(r => r.University).HasMaxLength(200);
			entity.Property(r => r.Expertise).HasMaxLength(300);
			entity.Property(r => r.Bio).HasMaxLength(1000);
			entity.Property(r => r.RejectReason).HasMaxLength(500);

			entity.HasIndex(r => r.Status);
			entity.HasIndex(r => r.UserId);
		});

		// ═══════════════ BaseEntity (Global Filter) ═══════════════
		builder.Entity<Category>().HasQueryFilter(e => !e.IsDeleted);
		builder.Entity<Course>().HasQueryFilter(e => !e.IsDeleted);
		builder.Entity<Syllabus>().HasQueryFilter(e => !e.IsDeleted);
		builder.Entity<Lesson>().HasQueryFilter(e => !e.IsDeleted);
		builder.Entity<Enrollment>().HasQueryFilter(e => !e.IsDeleted);
		builder.Entity<CourseReview>().HasQueryFilter(e => !e.IsDeleted);
		builder.Entity<Article>().HasQueryFilter(e => !e.IsDeleted);
		builder.Entity<InstructorRequest>().HasQueryFilter(e => !e.IsDeleted);
		builder.Entity<Tag>().HasQueryFilter(e => !e.IsDeleted);
	}
}