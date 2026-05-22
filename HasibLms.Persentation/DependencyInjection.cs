using System.IO.Compression;
using HasibLms.Application.Services;
using HasibLms.Domain.Interfaces;
using HasibLms.Persistence.Data;
using HasibLms.Persistence.Repositories;
using Microsoft.AspNetCore.ResponseCompression;
using Microsoft.EntityFrameworkCore;

namespace HasibLms.Presentation; // توجه: Presentation بدون 's'

public static class DependencyInjection
{
	public static IServiceCollection AddPresentation(this IServiceCollection services, IConfiguration configuration)
	{
		// ==========================================
		// DbContext
		// ==========================================
		services.AddDbContext<LmsContext>(options =>
			options.UseSqlServer(
				configuration.GetConnectionString("DefaultConnection"),
				b => b.MigrationsAssembly(typeof(LmsContext).Assembly.FullName)));

		// ==========================================
		// Repositories
		// ==========================================
		services.AddScoped(typeof(IGenericRepository<>), typeof(GenericRepository<>));
		services.AddScoped<ICourseRepository, CourseRepository>();
		services.AddScoped<ICategoryRepository, CategoryRepository>();
		services.AddScoped<IArticleRepository, ArticleRepository>();
		services.AddScoped<IUserRepository, UserRepository>();
		services.AddScoped<ITagRepository, TagRepository>();
		services.AddScoped<ISiteSettingsRepository, SiteSettingsRepository>();
		services.AddScoped<IInstructorRequestRepository, InstructorRequestRepository>();

		// ==========================================
		// Services
		// ==========================================
		services.AddScoped<ICourseService, CourseService>();
		services.AddScoped<IAuthService, AuthService>();
		services.AddScoped<IAdminService, AdminService>();
		services.AddScoped<ICategoryService, CategoryService>();
		services.AddScoped<IArticleService, ArticleService>();
		services.AddScoped<ITagService, TagService>();
		services.AddScoped<IEnrollmentService, EnrollmentService>();
		services.AddScoped<IReviewService, ReviewService>();
		services.AddScoped<ISiteService, SiteService>();
		services.AddScoped<IInstructorRequestService, InstructorRequestService>();

		// ==========================================
		// Caching & Performance
		// ==========================================
		services.AddMemoryCache();
		services.AddHttpContextAccessor();

		// Response Compression
		services.AddResponseCompression(options =>
		{
			options.EnableForHttps = true;
			options.Providers.Add<BrotliCompressionProvider>();
			options.Providers.Add<GzipCompressionProvider>();
			options.MimeTypes = ResponseCompressionDefaults.MimeTypes.Concat(new[]
			{
				"image/svg+xml",
				"application/javascript",
				"text/css",
				"application/json"
			});
		});

		services.Configure<BrotliCompressionProviderOptions>(options =>
		{
			options.Level = CompressionLevel.Optimal;
		});

		services.Configure<GzipCompressionProviderOptions>(options =>
		{
			options.Level = CompressionLevel.Optimal;
		});

		// Output Cache for SEO
		services.AddOutputCache(options =>
		{
			options.AddBasePolicy(builder =>
				builder.Expire(TimeSpan.FromMinutes(10)));

			options.AddPolicy("CourseDetail", policy =>
				policy.Expire(TimeSpan.FromMinutes(10))
					  .Tag("courses"));

			options.AddPolicy("HomePage", policy =>
				policy.Expire(TimeSpan.FromMinutes(15))
					  .Tag("homepage"));
		});

		services.AddHttpClient();

		return services;
	}
}