namespace HasibLms.Shared.DTOs.Settings;

public class SiteSettingsDto
{
	public Guid Id { get; set; }
	public string InstituteName { get; set; } = "موسسه آموزشی تخصصی حسابداری مالیبا";
	public string InstituteSlug { get; set; } = "maliba";
	public string InstituteShortName { get; set; } = "مالیبا";
	public string Slogan { get; set; } = string.Empty;

	public string Email { get; set; } = string.Empty;
	public string Phone { get; set; } = string.Empty;
	public string Mobile { get; set; } = string.Empty;
	public string Address { get; set; } = string.Empty;
	public string PostalCode { get; set; } = string.Empty;
	public string Fax { get; set; } = string.Empty;

	public string? Instagram { get; set; }
	public string? Telegram { get; set; }
	public string? LinkedIn { get; set; }
	public string? YouTube { get; set; }
	public string? WhatsApp { get; set; }

	public string? MapLatitude { get; set; }
	public string? MapLongitude { get; set; }
	public string? GoogleMapEmbed { get; set; }

	public string MetaTitle { get; set; } = string.Empty;
	public string MetaDescription { get; set; } = string.Empty;
	public string MetaKeywords { get; set; } = string.Empty;
	public string MetaAuthor { get; set; } = string.Empty;

	public string? LogoUrl { get; set; }
	public string? LogoWhiteUrl { get; set; }
	public string? FaviconUrl { get; set; }
	public string PrimaryColor { get; set; } = "#3b82f6";
	public string SecondaryColor { get; set; } = "#10b981";

	public string AboutText { get; set; } = string.Empty;
	public string FooterText { get; set; } = string.Empty;
	public string CopyrightText { get; set; } = string.Empty;

	public bool IsSiteActive { get; set; } = true;
	public bool AllowRegistration { get; set; } = true;
	public string CurrencySymbol { get; set; } = "تومان";
	public bool ShowPrices { get; set; } = true;
}

public class UpdateSiteSettingsDto
{
	public string InstituteName { get; set; } = string.Empty;
	public string InstituteShortName { get; set; } = string.Empty;
	public string Slogan { get; set; } = string.Empty;
	public string Email { get; set; } = string.Empty;
	public string Phone { get; set; } = string.Empty;
	public string Mobile { get; set; } = string.Empty;
	public string Address { get; set; } = string.Empty;
	public string PostalCode { get; set; } = string.Empty;
	public string Fax { get; set; } = string.Empty;
	public string? Instagram { get; set; }
	public string? Telegram { get; set; }
	public string? LinkedIn { get; set; }
	public string? YouTube { get; set; }
	public string? WhatsApp { get; set; }
	public string? MapLatitude { get; set; }
	public string? MapLongitude { get; set; }
	public string? GoogleMapEmbed { get; set; }
	public string MetaTitle { get; set; } = string.Empty;
	public string MetaDescription { get; set; } = string.Empty;
	public string MetaKeywords { get; set; } = string.Empty;
	public string? LogoUrl { get; set; }
	public string? LogoWhiteUrl { get; set; }
	public string? FaviconUrl { get; set; }
	public string PrimaryColor { get; set; } = "#3b82f6";
	public string SecondaryColor { get; set; } = "#10b981";
	public string AboutText { get; set; } = string.Empty;
	public string FooterText { get; set; } = string.Empty;
	public string CopyrightText { get; set; } = string.Empty;
	public bool AllowRegistration { get; set; } = true;
	public string CurrencySymbol { get; set; } = "تومان";
}