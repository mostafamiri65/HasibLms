using HasibLms.Domain.Entities.Settings;
using HasibLms.Domain.Interfaces;
using HasibLms.Shared.DTOs.Settings;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Logging;

namespace HasibLms.Application.Services;

public class SiteService : ISiteService
{
	private readonly ISiteSettingsRepository _siteSettingsRepository;
	private readonly IMemoryCache _cache;
	private readonly ILogger<SiteService> _logger;
	private const string CacheKey = "site_settings";

	public SiteService(
		ISiteSettingsRepository siteSettingsRepository,
		IMemoryCache cache,
		ILogger<SiteService> logger)
	{
		_siteSettingsRepository = siteSettingsRepository;
		_cache = cache;
		_logger = logger;
	}

	public async Task<SiteSettingsDto?> GetSettingsAsync(CancellationToken cancellationToken = default)
	{
		try
		{
			if (_cache.TryGetValue(CacheKey, out SiteSettingsDto? cachedSettings) && cachedSettings != null)
			{
				return cachedSettings;
			}

			var settings = await _siteSettingsRepository.GetSettingsAsync(cancellationToken);

			if (settings == null)
			{
				// Create default settings
				settings = new SiteSetting();
				await _siteSettingsRepository.AddAsync(settings, cancellationToken);
			}

			var dto = MapToDto(settings);

			_cache.Set(CacheKey, dto, new MemoryCacheEntryOptions
			{
				AbsoluteExpirationRelativeToNow = TimeSpan.FromHours(1),
				SlidingExpiration = TimeSpan.FromMinutes(15)
			});

			return dto;
		}
		catch (Exception ex)
		{
			_logger.LogError(ex, "Error getting site settings");
			return GetDefaultSettings();
		}
	}

	public async Task<SiteSettingsDto?> UpdateSettingsAsync(UpdateSiteSettingsDto model, Guid userId, CancellationToken cancellationToken = default)
	{
		try
		{
			var settings = await _siteSettingsRepository.GetSettingsAsync(cancellationToken);

			if (settings == null)
			{
				settings = new SiteSetting();
			}

			// Update properties
			settings.InstituteName = model.InstituteName;
			settings.InstituteShortName = model.InstituteShortName;
			settings.Slogan = model.Slogan;
			settings.Email = model.Email;
			settings.Phone = model.Phone;
			settings.Mobile = model.Mobile;
			settings.Address = model.Address;
			settings.PostalCode = model.PostalCode;
			settings.Fax = model.Fax;
			settings.Instagram = model.Instagram;
			settings.Telegram = model.Telegram;
			settings.LinkedIn = model.LinkedIn;
			settings.YouTube = model.YouTube;
			settings.WhatsApp = model.WhatsApp;
			settings.MapLatitude = model.MapLatitude;
			settings.MapLongitude = model.MapLongitude;
			settings.GoogleMapEmbed = model.GoogleMapEmbed;
			settings.MetaTitle = model.MetaTitle;
			settings.MetaDescription = model.MetaDescription;
			settings.MetaKeywords = model.MetaKeywords;
			settings.LogoUrl = model.LogoUrl;
			settings.LogoWhiteUrl = model.LogoWhiteUrl;
			settings.FaviconUrl = model.FaviconUrl;
			settings.PrimaryColor = model.PrimaryColor;
			settings.SecondaryColor = model.SecondaryColor;
			settings.AboutText = model.AboutText;
			settings.FooterText = model.FooterText;
			settings.CopyrightText = model.CopyrightText;
			settings.AllowRegistration = model.AllowRegistration;
			settings.CurrencySymbol = model.CurrencySymbol;

			settings.ShowFeaturedCourses = model.ShowFeaturedCourses;
			settings.ShowLatestCourses = model.ShowLatestCourses;
			settings.ShowPopularCourses = model.ShowPopularCourses;
			settings.ShowCategories = model.ShowCategories;
			settings.ShowLatestArticles = model.ShowLatestArticles;
			settings.ShowPopularArticles = model.ShowPopularArticles;
			settings.ShowStats = model.ShowStats;
			settings.ShowTestimonials = model.ShowTestimonials;

			settings.HomeFeaturedCoursesCount = model.HomeFeaturedCoursesCount;
			settings.HomeLatestCoursesCount = model.HomeLatestCoursesCount;
			settings.HomePopularCoursesCount = model.HomePopularCoursesCount;
			settings.HomeArticlesCount = model.HomeArticlesCount;

			settings.ShowWhyChooseUs = model.ShowWhyChooseUs;
			settings.WhyChooseUsTitle = model.WhyChooseUsTitle;
			settings.WhyChooseUsSubtitle = model.WhyChooseUsSubtitle;
			settings.LastUpdated = DateTime.Now;
			settings.LastUpdatedBy = userId;

			await _siteSettingsRepository.UpdateAsync(settings, cancellationToken);

			// Clear cache
			_cache.Remove(CacheKey);

			return await GetSettingsAsync(cancellationToken);
		}
		catch (Exception ex)
		{
			_logger.LogError(ex, "Error updating site settings");
			return null;
		}
	}

	public async Task<string> GetInstituteNameAsync()
	{
		var settings = await GetSettingsAsync();
		return settings?.InstituteName ?? "موسسه آموزشی تخصصی حسابداری مالیبا";
	}

	public async Task<string> GetSloganAsync()
	{
		var settings = await GetSettingsAsync();
		return settings?.Slogan ?? "پیشرو در آموزش تخصصی حسابداری و مالی";
	}

	private SiteSettingsDto MapToDto(SiteSetting settings)
	{
		return new SiteSettingsDto
		{
			InstituteName = settings.InstituteName,
			InstituteSlug = settings.InstituteSlug,
			InstituteShortName = settings.InstituteShortName,
			Slogan = settings.Slogan,
			Email = settings.Email,
			Phone = settings.Phone,
			Mobile = settings.Mobile,
			Address = settings.Address,
			PostalCode = settings.PostalCode,
			Fax = settings.Fax,
			Instagram = settings.Instagram,
			Telegram = settings.Telegram,
			LinkedIn = settings.LinkedIn,
			YouTube = settings.YouTube,
			WhatsApp = settings.WhatsApp,
			MapLatitude = settings.MapLatitude,
			MapLongitude = settings.MapLongitude,
			GoogleMapEmbed = settings.GoogleMapEmbed,
			MetaTitle = settings.MetaTitle,
			MetaDescription = settings.MetaDescription,
			MetaKeywords = settings.MetaKeywords,
			LogoUrl = settings.LogoUrl,
			LogoWhiteUrl = settings.LogoWhiteUrl,
			FaviconUrl = settings.FaviconUrl,
			PrimaryColor = settings.PrimaryColor ?? "#3b82f6",
			SecondaryColor = settings.SecondaryColor ?? "#10b981",
			AboutText = settings.AboutText,
			FooterText = settings.FooterText,
			CopyrightText = settings.CopyrightText,
			AllowRegistration = settings.AllowRegistration,
			CurrencySymbol = settings.CurrencySymbol,
			ShowFeaturedCourses = settings.ShowFeaturedCourses,
			ShowLatestCourses = settings.ShowLatestCourses,
			ShowPopularCourses = settings.ShowPopularCourses,
			ShowCategories = settings.ShowCategories,
			ShowLatestArticles = settings.ShowLatestArticles,
			ShowPopularArticles = settings.ShowPopularArticles,
			ShowStats = settings.ShowStats,
			ShowTestimonials = settings.ShowTestimonials,

			HomeFeaturedCoursesCount = settings.HomeFeaturedCoursesCount,
			HomeLatestCoursesCount = settings.HomeLatestCoursesCount,
			HomePopularCoursesCount = settings.HomePopularCoursesCount,
			HomeArticlesCount = settings.HomeArticlesCount,

			//ShowWhyChooseUs = settings.ShowWhyChooseUs,
			//WhyChooseUsTitle = settings.WhyChooseUsTitle,
			//WhyChooseUsSubtitle = settings.WhyChooseUsSubtitle,
		};
	}

	private SiteSettingsDto GetDefaultSettings()
	{
		return new SiteSettingsDto
		{
			InstituteName = "موسسه آموزشی تخصصی حسابداری مالیبا",
			InstituteSlug = "maliba",
			InstituteShortName = "مالیبا",
			Slogan = "پیشرو در آموزش تخصصی حسابداری و مالی",
			PrimaryColor = "#3b82f6",
			SecondaryColor = "#10b981",
			CurrencySymbol = "تومان",
			AllowRegistration = true
		};
	}
}