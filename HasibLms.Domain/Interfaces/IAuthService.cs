using HasibLms.Shared.DTOs.Auth;

namespace HasibLms.Domain.Interfaces;

public interface IAuthService
{
	Task<AuthResultDto> RegisterAsync(RegisterDto model, CancellationToken cancellationToken = default);
	Task<AuthResultDto> LoginAsync(LoginDto model, CancellationToken cancellationToken = default);
	Task LogoutAsync();
	Task<AuthResultDto> ChangePasswordAsync(ChangePasswordDto model, Guid userId, CancellationToken cancellationToken = default);
	Task<AuthResultDto> ForgotPasswordAsync(ForgotPasswordDto model, CancellationToken cancellationToken = default);
	Task<AuthResultDto> ResetPasswordAsync(ResetPasswordDto model, CancellationToken cancellationToken = default);
	Task<UserProfileDto?> GetUserProfileAsync(Guid userId, CancellationToken cancellationToken = default);
	Task<AuthResultDto> UpdateProfileAsync(UpdateProfileDto model, Guid userId, CancellationToken cancellationToken = default);
}