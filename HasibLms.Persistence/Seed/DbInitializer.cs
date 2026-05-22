using HasibLms.Domain.Entities;
using HasibLms.Domain.Entities.AcademyEntities;
using HasibLms.Persistence.Data;
using HasibLms.Shared.Constants;
using HasibLms.Shared.Enumerations;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;

namespace HasibLms.Persistence.Seed;

public static class DbInitializer
{
	public static async Task SeedAsync(IServiceProvider serviceProvider)
	{
		using var scope = serviceProvider.CreateScope();
		var context = scope.ServiceProvider.GetRequiredService<LmsContext>();
		var userManager = scope.ServiceProvider.GetRequiredService<UserManager<User>>();
		var roleManager = scope.ServiceProvider.GetRequiredService<RoleManager<IdentityRole<Guid>>>();

		await context.Database.MigrateAsync();

		// Seed Roles
		var roles = new[] { UserRoles.Admin, UserRoles.Instructor, UserRoles.Student };
		foreach (var role in roles)
		{
			if (!await roleManager.RoleExistsAsync(role))
			{
				await roleManager.CreateAsync(new IdentityRole<Guid>(role));
			}
		}

		// Seed Admin User
		var adminEmail = "admin@hasiblms.com";
		var adminUser = await userManager.FindByEmailAsync(adminEmail);
		if (adminUser == null)
		{
			adminUser = new User
			{
				UserName = adminEmail,
				Email = adminEmail,
				FullName = "مدیر سیستم",
				EmailConfirmed = true,
				CreatedDate = DateTime.UtcNow
			};
			var result = await userManager.CreateAsync(adminUser, "Admin@123");
			if (result.Succeeded)
			{
				await userManager.AddToRoleAsync(adminUser, UserRoles.Admin);
			}
		}

		// Seed Sample Categories
		if (!await context.Categories.AnyAsync())
		{
			var accounting = new Category
			{
				Name = "حسابداری مالی",
				Slug = "accounting",
				Description = "دوره‌های تخصصی حسابداری مالی",
				CreatedDate = DateTime.UtcNow
			};

			var tax = new Category
			{
				Name = "مالیات",
				Slug = "tax",
				Description = "دوره‌های مالیاتی",
				CreatedDate = DateTime.UtcNow
			};

			var investment = new Category
			{
				Name = "سرمایه‌گذاری",
				Slug = "investment",
				Description = "دوره‌های سرمایه‌گذاری",
				CreatedDate = DateTime.UtcNow
			};

			var budgeting = new Category
			{
				Name = "بودجه‌ریزی",
				Slug = "budgeting",
				Description = "دوره‌های بودجه‌ریزی",
				CreatedDate = DateTime.UtcNow
			};

			await context.Categories.AddRangeAsync(accounting, tax, investment, budgeting);
			await context.SaveChangesAsync();
		}

		// Seed Sample Instructor
		var instructorEmail = "instructor@hasiblms.com";
		var instructor = await userManager.FindByEmailAsync(instructorEmail);
		if (instructor == null)
		{
			instructor = new User
			{
				UserName = instructorEmail,
				Email = instructorEmail,
				FullName = "مدرس نمونه",
				Bio = "مدرس حرفه‌ای حوزه اقتصاد و حسابداری با بیش از ۱۰ سال سابقه تدریس",
				EmailConfirmed = true,
				CreatedDate = DateTime.UtcNow
			};
			var result = await userManager.CreateAsync(instructor, "Instructor@123");
			if (result.Succeeded)
			{
				await userManager.AddToRoleAsync(instructor, UserRoles.Instructor);
			}
		}

		// Seed Sample Course
		if (!await context.Courses.AnyAsync())
		{
			var category = await context.Categories.FirstOrDefaultAsync(c => c.Slug == "accounting");
			if (category != null && instructor != null)
			{
				var course = new Course
				{
					Title = "دوره جامع حسابداری مالی",
					Slug = "comprehensive-financial-accounting",
					ShortDescription = "آموزش کامل حسابداری مالی از مقدماتی تا پیشرفته",
					Description = "<p>در این دوره شما با تمام مباحث حسابداری مالی آشنا می‌شوید...</p>",
					Price = 1200000,
					DiscountPrice = 890000,
					ImageUrl = "/images/courses/accounting.jpg",
					CourseType = CourseType.Online,
					StartDate = DateTime.UtcNow.AddDays(7),
					DurationHours = 40,
					Capacity = 50,
					IsPublished = true,
					IsFeatured = true,
					InstructorId = instructor.Id,
					CategoryId = category.Id,
					CreatedDate = DateTime.UtcNow
				};

				await context.Courses.AddAsync(course);
				await context.SaveChangesAsync();
			}
		}
	}
}