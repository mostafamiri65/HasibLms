using HasibLms.Domain.Entities;
using HasibLms.Persentation;
using HasibLms.Persistence.Data;
using HasibLms.Persistence.Seed;
using HasibLms.Presentation;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddControllersWithViews(options =>
{
	options.CacheProfiles.Add("SEO", new CacheProfile
	{
		Duration = 3600,
		Location = ResponseCacheLocation.Any,
		VaryByQueryKeys = new[] { "id", "slug" }
	});
});

// Add Presentation services (DbContext, Repositories, Services, Cache, Compression)
builder.Services.AddPresentation(builder.Configuration);

// Identity
builder.Services.AddIdentity<User, IdentityRole<Guid>>(options =>
{
	// Password settings
	options.Password.RequireDigit = false;
	options.Password.RequiredLength = 4;
	options.Password.RequireNonAlphanumeric = false;
	options.Password.RequireUppercase = false;
	options.Password.RequireLowercase = false;

	// User settings
	options.User.RequireUniqueEmail = true;
	options.SignIn.RequireConfirmedAccount = false;

	// Lockout settings
	options.Lockout.DefaultLockoutTimeSpan = TimeSpan.FromMinutes(15);
	options.Lockout.MaxFailedAccessAttempts = 5;
})
.AddEntityFrameworkStores<LmsContext>()
.AddDefaultTokenProviders();

// Cookie configuration
builder.Services.ConfigureApplicationCookie(options =>
{
	options.Cookie.Name = "HasibLms.Auth";
	options.Cookie.HttpOnly = true;
	options.Cookie.SecurePolicy = CookieSecurePolicy.Always;
	options.Cookie.SameSite = SameSiteMode.Strict;
	options.ExpireTimeSpan = TimeSpan.FromDays(30);
	options.SlidingExpiration = true;
	options.LoginPath = "/Auth/Login";
	options.LogoutPath = "/Auth/Logout";
	options.AccessDeniedPath = "/Auth/AccessDenied";
});

builder.Services.AddResponseCaching(options =>
{
	options.MaximumBodySize = 64 * 1024 * 1024; // 64 MB
	options.SizeLimit = 100 * 1024 * 1024; // 100 MB
	options.UseCaseSensitivePaths = false;
});

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
	app.UseDeveloperExceptionPage();
}
else
{
	app.UseExceptionHandler("/Home/Error");
	app.UseHsts();
}
app.UseResponseCaching(); // اضافه کنید

app.UseHttpsRedirection();
app.UseResponseCompression(); // Important for performance
app.UseStaticFiles();
app.UseRouting();

app.UseAuthentication();
app.UseAuthorization();
app.UseOutputCache(); // For caching pages

// SEO Friendly Routes
app.MapControllerRoute(
	name: "default",
	pattern: "{controller=Home}/{action=Index}/{id?}")
	.WithStaticAssets();

app.MapControllerRoute(
	name: "course_detail",
	pattern: "course/{slug}",
	defaults: new { controller = "Course", action = "Detail" });

app.MapControllerRoute(
	name: "article_detail",
	pattern: "article/{slug}",
	defaults: new { controller = "Article", action = "Detail" });

app.MapControllerRoute(
	name: "category_courses",
	pattern: "category/{slug}",
	defaults: new { controller = "Course", action = "ByCategory" });

app.MapControllerRoute(
	name: "admin_dashboard",
	pattern: "admin/{action=Index}/{id?}",
	defaults: new { controller = "Admin" });

// Seed database
using (var scope = app.Services.CreateScope())
{
	var services = scope.ServiceProvider;
	try
	{
		await DbInitializer.SeedAsync(services);
	}
	catch (Exception ex)
	{
		var logger = services.GetRequiredService<ILogger<Program>>();
		logger.LogError(ex, "An error occurred while seeding the database.");
	}
}

app.Run();