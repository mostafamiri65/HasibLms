namespace HasibLms.Domain.Entities.Settings;

public class SiteSetting : BaseEntity
{
	// اطلاعات پایه موسسه
	public string? InstituteName { get; set; } = "موسسه آموزشی تخصصی حسابداری مالیبا";
	public string? InstituteSlug { get; set; } = "maliba";
	public string? InstituteShortName { get; set; } = "مالیبا";
	public string? Slogan { get; set; } = "پیشرو در آموزش تخصصی حسابداری و مالی";

	// اطلاعات تماس
	public string? Email { get; set; } = string.Empty;
	public string? Phone { get; set; } = string.Empty;
	public string? Mobile { get; set; } = string.Empty;
	public string? Address { get; set; } = string.Empty;
	public string? PostalCode { get; set; } = string.Empty;
	public string? Fax { get; set; } = string.Empty;

	// شبکه‌های اجتماعی
	public string? Instagram { get; set; }
	public string? Telegram { get; set; }
	public string? LinkedIn { get; set; }
	public string? YouTube { get; set; }
	public string? WhatsApp { get; set; }

	// اطلاعات نقشه و موقعیت
	public string? MapLatitude { get; set; }
	public string? MapLongitude { get; set; }
	public string? GoogleMapEmbed { get; set; }

	// SEO و Meta
	public string? MetaTitle { get; set; } = string.Empty;
	public string? MetaDescription { get; set; } = string.Empty;
	public string? MetaKeywords { get; set; } = string.Empty;
	public string? MetaAuthor { get; set; } = string.Empty;

	// لوگو و ظاهر
	public string? LogoUrl { get; set; }
	public string? LogoWhiteUrl { get; set; }
	public string? FaviconUrl { get; set; }
	public string? PrimaryColor { get; set; } = "#3b82f6";
	public string? SecondaryColor { get; set; } = "#10b981";

	// متن‌های ثابت
	public string? AboutText { get; set; } = string.Empty;
	public string? FooterText { get; set; } = string.Empty;
	public string? CopyrightText { get; set; } = string.Empty;

	// تنظیمات عمومی
	public bool IsSiteActive { get; set; } = true;
	public string? MaintenanceMessage { get; set; }
	public bool AllowRegistration { get; set; } = true;
	public bool RequireEmailConfirmation { get; set; } = false;

	// تنظیمات مالی
	public string? CurrencySymbol { get; set; } = "تومان";
	public bool ShowPrices { get; set; } = true;
	public decimal? DefaultTaxRate { get; set; }

	// تنظیمات ایمیل
	public string? SmtpServer { get; set; }
	public int? SmtpPort { get; set; }
	public string? SmtpUsername { get; set; }
	public string? SmtpPassword { get; set; }
	public bool UseSsl { get; set; } = true;

	public bool ShowWhyChooseUs { get; set; } = true;
	public string? WhyChooseUsTitle { get; set; } = "چرا باید ما را انتخاب کنید؟";
	public string? WhyChooseUsSubtitle { get; set; }

	// آخرین بروزرسانی
	public DateTime LastUpdated { get; set; } = DateTime.UtcNow;
	public Guid? LastUpdatedBy { get; set; }


	public bool ShowFeaturedCourses { get; set; } = true;
	public bool ShowLatestCourses { get; set; } = true;
	public bool ShowPopularCourses { get; set; } = true;
	public bool ShowCategories { get; set; } = true;
	public bool ShowLatestArticles { get; set; } = true;
	public bool ShowPopularArticles { get; set; } = true;
	public bool ShowStats { get; set; } = true;
	public bool ShowTestimonials { get; set; } = true;

	public int HomeFeaturedCoursesCount { get; set; } = 6;
	public int HomeLatestCoursesCount { get; set; } = 6;
	public int HomePopularCoursesCount { get; set; } = 6;
	public int HomeArticlesCount { get; set; } = 3;
}