using HasibLms.Domain.Entities;
using HasibLms.Domain.Interfaces;
using HasibLms.Persistence.Data;
using HasibLms.Shared.DTOs.Admin;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;

namespace HasibLms.Persistence.Repositories;

public class UserRepository : IUserRepository
{
	private readonly UserManager<User> _userManager;
	private readonly LmsContext _context;

	public UserRepository(UserManager<User> userManager, LmsContext context)
	{
		_userManager = userManager;
		_context = context;
	}

	public async Task<List<UserListDto>> GetPagedUsersWithRolesAsync(int page, int pageSize, string? role = null, CancellationToken cancellationToken = default)
	{
		var query = _userManager.Users.AsQueryable();

		if (!string.IsNullOrEmpty(role))
		{
			var usersInRole = await _userManager.GetUsersInRoleAsync(role);
			var userIds = usersInRole.Select(u => u.Id);
			query = query.Where(u => userIds.Contains(u.Id));
		}

		var users = await query
			.OrderByDescending(u => u.CreatedDate)
			.Skip((page - 1) * pageSize)
			.Take(pageSize)
			.ToListAsync(cancellationToken);

		var result = new List<UserListDto>();
		foreach (var user in users)
		{
			var roles = await _userManager.GetRolesAsync(user);
			var isLockedOut = await _userManager.IsLockedOutAsync(user);

			result.Add(new UserListDto
			{
				Id = user.Id,
				FullName = user.FullName,
				Email = user.Email ?? string.Empty,
				PhoneNumber = user.PhoneNumber,
				Roles = roles.ToList(),
				CreatedDate = user.CreatedDate,
				IsLockedOut = isLockedOut
			});
		}

		return result;
	}

	public async Task<int> GetTotalUsersCountAsync(string? role = null, CancellationToken cancellationToken = default)
	{
		if (string.IsNullOrEmpty(role))
		{
			return await _userManager.Users.CountAsync(cancellationToken);
		}

		var usersInRole = await _userManager.GetUsersInRoleAsync(role);
		return usersInRole.Count;
	}

	public async Task<int> GetUsersCountInRoleAsync(string role, CancellationToken cancellationToken = default)
	{
		var usersInRole = await _userManager.GetUsersInRoleAsync(role);
		return usersInRole.Count;
	}

	public async Task<User?> GetUserWithDetailsAsync(Guid userId, CancellationToken cancellationToken = default)
	{
		return await _userManager.Users
			.Include(u => u.Enrollments.Where(e => !e.IsDeleted))
			.Include(u => u.TaughtCourses.Where(c => !c.IsDeleted))
			.Include(u => u.Articles.Where(a => !a.IsDeleted))
			.Include(u => u.Reviews.Where(r => !r.IsDeleted))
			.FirstOrDefaultAsync(u => u.Id == userId, cancellationToken);
	}

	public async Task<bool> IsUserInRoleAsync(Guid userId, string role, CancellationToken cancellationToken = default)
	{
		var user = await _userManager.FindByIdAsync(userId.ToString());
		if (user == null) return false;
		return await _userManager.IsInRoleAsync(user, role);
	}
	public async Task<List<User>> GetUsersInRoleAsync(string role, CancellationToken cancellationToken = default)
	{
		var users = await _userManager.GetUsersInRoleAsync(role);
		return users.ToList();
	}
}