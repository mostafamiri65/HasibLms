using HasibLms.Domain.Entities;
using HasibLms.Shared.DTOs.Admin;

namespace HasibLms.Domain.Interfaces;

public interface IUserRepository
{
	Task<List<UserListDto>> GetPagedUsersWithRolesAsync(int page, int pageSize, string? role = null, CancellationToken cancellationToken = default);
	Task<int> GetTotalUsersCountAsync(string? role = null, CancellationToken cancellationToken = default);
	Task<int> GetUsersCountInRoleAsync(string role, CancellationToken cancellationToken = default);
	Task<User?> GetUserWithDetailsAsync(Guid userId, CancellationToken cancellationToken = default);
	Task<bool> IsUserInRoleAsync(Guid userId, string role, CancellationToken cancellationToken = default);
	Task<List<User>> GetUsersInRoleAsync(string role, CancellationToken cancellationToken = default);
}