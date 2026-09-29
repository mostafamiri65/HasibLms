namespace HasibLms.Shared.DTOs.Page;

public class UploadResult
{
	public bool Succeeded { get; set; }
	public string? Url { get; set; }
	public string? Error { get; set; }
}
