using HasibLms.Domain.Entities;
using HasibLms.Domain.Entities.AcademyEntities;
using HasibLms.Domain.Entities.Settings;
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
		// ==========================================
		// Seed Sample "Why Choose Us" Items
		// ==========================================
		if (!await context.WhyChooseUsItems.AnyAsync())
		{
			var whyChooseUsItems = new List<WhyChooseUsItem>
			{
				new WhyChooseUsItem
				{
					Title = "اساتید مجرب و حرفه‌ای",
					Description = "تدریس توسط اساتید با سابقه درخشان در حوزه حسابداری و مالی با بیش از ۱۰ سال تجربه عملی",
					Icon = "fas fa-chalkboard-teacher",
					Order = 1,
					IsActive = true,
					ShowOnHomePage = true,
					IconColor = "#3b82f6",
					CreatedDate = DateTime.UtcNow,
					LastModifiedDate = DateTime.UtcNow
				},
				new WhyChooseUsItem
				{
					Title = "محتوای به‌روز و کاربردی",
					Description = "دوره‌ها بر اساس آخرین استانداردهای حسابداری و نیازهای بازار کار طراحی شده‌اند",
					Icon = "fas fa-book-open",
					Order = 2,
					IsActive = true,
					ShowOnHomePage = true,
					IconColor = "#10b981",
					CreatedDate = DateTime.UtcNow,
					LastModifiedDate = DateTime.UtcNow
				},
				new WhyChooseUsItem
				{
					Title = "پشتیبانی ۲۴ ساعته",
					Description = "تیم پشتیبانی ما در تمام ساعات شبانه‌روز پاسخگوی سوالات و مشکلات شما هستند",
					Icon = "fas fa-headset",
					Order = 3,
					IsActive = true,
					ShowOnHomePage = true,
					IconColor = "#8b5cf6",
					CreatedDate = DateTime.UtcNow,
					LastModifiedDate = DateTime.UtcNow
				},
				new WhyChooseUsItem
				{
					Title = "گواهینامه معتبر",
					Description = "پس از اتمام هر دوره، گواهینامه معتبر با قابلیت استعلام دریافت می‌کنید",
					Icon = "fas fa-certificate",
					Order = 4,
					IsActive = true,
					ShowOnHomePage = true,
					IconColor = "#f59e0b",
					CreatedDate = DateTime.UtcNow,
					LastModifiedDate = DateTime.UtcNow
				},
				new WhyChooseUsItem
				{
					Title = "دسترسی مادام‌العمر",
					Description = "پس از ثبت‌نام، به تمام محتوای دوره دسترسی مادام‌العمر خواهید داشت",
					Icon = "fas fa-infinity",
					Order = 5,
					IsActive = true,
					ShowOnHomePage = true,
					IconColor = "#ef4444",
					CreatedDate = DateTime.UtcNow,
					LastModifiedDate = DateTime.UtcNow
				},
				new WhyChooseUsItem
				{
					Title = "قیمت‌های مناسب",
					Description = "دوره‌های با کیفیت بالا با قیمت‌های مناسب و امکان پرداخت اقساط",
					Icon = "fas fa-hand-holding-usd",
					Order = 6,
					IsActive = true,
					ShowOnHomePage = true,
					IconColor = "#06b6d4",
					CreatedDate = DateTime.UtcNow,
					LastModifiedDate = DateTime.UtcNow
				}
			};

			await context.WhyChooseUsItems.AddRangeAsync(whyChooseUsItems);
			await context.SaveChangesAsync();
		}

		// ==========================================
		// Seed Sample Menu Items
		// ==========================================
		if (!await context.MenuItems.AnyAsync())
		{
			var menuItems = new List<MenuItemEntity>
			{
				// Header Menu
				new MenuItemEntity
				{
					Title = "خانه",
					Url = "/",
					Icon = "fas fa-home",
					Order = 1,
					IsActive = true,
					Location = "header",
					Target = "_self",
					CreatedDate = DateTime.UtcNow,
					LastModifiedDate = DateTime.UtcNow
				},
				new MenuItemEntity
				{
					Title = "دوره‌ها",
					Url = "/course",
					Icon = "fas fa-graduation-cap",
					Order = 2,
					IsActive = true,
					Location = "header",
					Target = "_self",
					CreatedDate = DateTime.UtcNow,
					LastModifiedDate = DateTime.UtcNow
				},
				new MenuItemEntity
				{
					Title = "مقالات",
					Url = "/article",
					Icon = "fas fa-newspaper",
					Order = 3,
					IsActive = true,
					Location = "header",
					Target = "_self",
					CreatedDate = DateTime.UtcNow,
					LastModifiedDate = DateTime.UtcNow
				},
				new MenuItemEntity
				{
					Title = "درباره ما",
					Url = "/home/about",
					Icon = "fas fa-info-circle",
					Order = 4,
					IsActive = true,
					Location = "header",
					Target = "_self",
					CreatedDate = DateTime.UtcNow,
					LastModifiedDate = DateTime.UtcNow
				},
				new MenuItemEntity
				{
					Title = "تماس با ما",
					Url = "/home/contact",
					Icon = "fas fa-phone",
					Order = 5,
					IsActive = true,
					Location = "header",
					Target = "_self",
					CreatedDate = DateTime.UtcNow,
					LastModifiedDate = DateTime.UtcNow
				},

				// Footer Menu
				new MenuItemEntity
				{
					Title = "درباره ما",
					Url = "/home/about",
					Order = 1,
					IsActive = true,
					Location = "footer",
					Target = "_self",
					CreatedDate = DateTime.UtcNow,
					LastModifiedDate = DateTime.UtcNow
				},
				new MenuItemEntity
				{
					Title = "تماس با ما",
					Url = "/home/contact",
					Order = 2,
					IsActive = true,
					Location = "footer",
					Target = "_self",
					CreatedDate = DateTime.UtcNow,
					LastModifiedDate = DateTime.UtcNow
				},
				new MenuItemEntity
				{
					Title = "حریم خصوصی",
					Url = "/home/privacy",
					Order = 3,
					IsActive = true,
					Location = "footer",
					Target = "_self",
					CreatedDate = DateTime.UtcNow,
					LastModifiedDate = DateTime.UtcNow
				},
				new MenuItemEntity
				{
					Title = "قوانین و مقررات",
					Url = "/home/terms",
					Order = 4,
					IsActive = true,
					Location = "footer",
					Target = "_self",
					CreatedDate = DateTime.UtcNow,
					LastModifiedDate = DateTime.UtcNow
				}
			};

			await context.MenuItems.AddRangeAsync(menuItems);
			await context.SaveChangesAsync();
		}

		// ==========================================
		// Seed Site Settings with Why Choose Us config
		// ==========================================
		if (!await context.SiteSettings.AnyAsync())
		{
			var siteSetting = new SiteSetting
			{
				InstituteName = "موسسه آموزشی تخصصی حسابداری مالیبا",
				InstituteSlug = "maliba",
				InstituteShortName = "مالیبا",
				Slogan = "پیشرو در آموزش تخصصی حسابداری و مالی",
				Email = "info@hasiblms.com",
				Phone = "021-12345678",
				Address = "تهران، خیابان آزادی، پلاک ۱۲۳",
				PrimaryColor = "#3b82f6",
				SecondaryColor = "#10b981",
				CurrencySymbol = "تومان",
				AllowRegistration = true,
				IsSiteActive = true,
				ShowWhyChooseUs = true,
				WhyChooseUsTitle = "چرا باید ما را انتخاب کنید؟",
				WhyChooseUsSubtitle = "با ما در مسیر موفقیت شغلی خود قدم بردارید",
				AboutText = "موسسه آموزشی مالیبا با بیش از ۱۰ سال تجربه در زمینه آموزش تخصصی حسابداری و مالی، افتخار دارد با بهره‌گیری از اساتید مجرب و محتوای به‌روز، دوره‌های آموزشی با کیفیتی را ارائه دهد.",
				FooterText = "موسسه آموزشی مالیبا - پیشرو در آموزش تخصصی حسابداری و مالی",
				CopyrightText = "تمامی حقوق برای موسسه آموزشی مالیبا محفوظ است.",
				CreatedDate = DateTime.UtcNow,
				LastModifiedDate = DateTime.UtcNow,
				LastUpdated = DateTime.UtcNow
			};

			await context.SiteSettings.AddAsync(siteSetting);
			await context.SaveChangesAsync();
		}
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