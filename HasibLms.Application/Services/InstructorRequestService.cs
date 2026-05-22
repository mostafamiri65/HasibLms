using HasibLms.Domain.Entities;
using HasibLms.Domain.Interfaces;
using HasibLms.Shared.Constants;
using HasibLms.Shared.DTOs;
using HasibLms.Shared.DTOs.Auth;
using HasibLms.Shared.DTOs.Instructor;
using HasibLms.Shared.Enumerations;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Logging;

namespace HasibLms.Application.Services;

public class InstructorRequestService : IInstructorRequestService
{
	private readonly IInstructorRequestRepository _requestRepository;
	private readonly UserManager<User> _userManager;
	private readonly ILogger<InstructorRequestService> _logger;

	public InstructorRequestService(
		IInstructorRequestRepository requestRepository,
		UserManager<User> userManager,
		ILogger<InstructorRequestService> logger)
	{
		_requestRepository = requestRepository;
		_userManager = userManager;
		_logger = logger;
	}

	public async Task<AuthResultDto> ApplyForInstructorAsync(ApplyForInstructorDto model, Guid userId, CancellationToken cancellationToken = default)
	{
		try
		{
			var user = await _userManager.FindByIdAsync(userId.ToString());
			if (user == null)
			{
				return new AuthResultDto
				{
					Succeeded = false,
					Message = "کاربر یافت نشد"
				};
			}

			// Check if already instructor
			if (await _userManager.IsInRoleAsync(user, UserRoles.Instructor))
			{
				return new AuthResultDto
				{
					Succeeded = false,
					Message = "شما قبلاً مدرس هستید"
				};
			}

			// Check if there's a pending request
			if (await _requestRepository.HasPendingRequestAsync(userId, cancellationToken))
			{
				return new AuthResultDto
				{
					Succeeded = false,
					Message = "شما قبلاً درخواست مدرس شدن ثبت کرده‌اید. لطفاً منتظر بررسی باشید."
				};
			}

			var request = new InstructorRequest
			{
				Id = Guid.NewGuid(),
				UserId = userId,
				FullName = model.FullName,
				Email = model.Email,
				PhoneNumber = model.PhoneNumber,
				Degree = model.Degree,
				University = model.University,
				YearsOfExperience = model.YearsOfExperience,
				Expertise = model.Expertise,
				Bio = model.Bio,
				TeachingExperience = model.TeachingExperience,
				SampleWorkUrl = model.SampleWorkUrl,
				ResumeUrl = model.ResumeUrl,
				CertificateUrl = model.CertificateUrl,
				Status = RequestStatus.Pending,
				RequestedAt = DateTime.UtcNow,
				CreatedDate = DateTime.UtcNow,
				LastModifiedDate = DateTime.UtcNow
			};

			await _requestRepository.AddAsync(request, cancellationToken);

			_logger.LogInformation("User {UserId} applied to become instructor", userId);

			return new AuthResultDto
			{
				Succeeded = true,
				Message = "درخواست شما با موفقیت ثبت شد. پس از تأیید ادمین، شما به عنوان مدرس فعال خواهید شد."
			};
		}
		catch (Exception ex)
		{
			_logger.LogError(ex, "Error applying for instructor by user {UserId}", userId);
			return new AuthResultDto
			{
				Succeeded = false,
				Message = "خطا در ثبت درخواست. لطفاً مجدداً تلاش کنید."
			};
		}
	}

	public async Task<PagedResultDto<InstructorRequestListDto>> GetRequestsAsync(
		int page, int pageSize, RequestStatus? status = null, CancellationToken cancellationToken = default)
	{
		try
		{
			page = page < 1 ? 1 : page;
			pageSize = pageSize < 1 ? 10 : (pageSize > 100 ? 100 : pageSize);

			var predicate = status.HasValue
				? (System.Linq.Expressions.Expression<Func<InstructorRequest, bool>>)(r => r.Status == status.Value)
				: null;

			var requests = await _requestRepository.GetPagedAsync(page, pageSize, predicate,
				q => q.OrderByDescending(r => r.RequestedAt), cancellationToken);

			var totalCount = await _requestRepository.CountAsync(predicate, cancellationToken);

			return new PagedResultDto<InstructorRequestListDto>
			{
				Items = requests.Select(MapToDto).ToList(),
				TotalCount = totalCount,
				PageNumber = page,
				PageSize = pageSize
			};
		}
		catch (Exception ex)
		{
			_logger.LogError(ex, "Error getting instructor requests");
			return new PagedResultDto<InstructorRequestListDto>
			{
				Items = new List<InstructorRequestListDto>(),
				TotalCount = 0,
				PageNumber = page,
				PageSize = pageSize
			};
		}
	}

	public async Task<InstructorRequestListDto?> GetRequestDetailsAsync(Guid requestId, CancellationToken cancellationToken = default)
	{
		try
		{
			var request = await _requestRepository.GetByIdAsync(requestId, cancellationToken);
			return request != null ? MapToDto(request) : null;
		}
		catch (Exception ex)
		{
			_logger.LogError(ex, "Error getting request details {RequestId}", requestId);
			return null;
		}
	}

	public async Task<AuthResultDto> ReviewRequestAsync(Guid requestId, bool isApproved, string? rejectReason, Guid adminId, CancellationToken cancellationToken = default)
	{
		try
		{
			var request = await _requestRepository.GetByIdAsync(requestId, cancellationToken);
			if (request == null)
			{
				return new AuthResultDto
				{
					Succeeded = false,
					Message = "درخواست یافت نشد"
				};
			}

			if (request.Status != RequestStatus.Pending)
			{
				return new AuthResultDto
				{
					Succeeded = false,
					Message = "این درخواست قبلاً بررسی شده است"
				};
			}

			var user = await _userManager.FindByIdAsync(request.UserId.ToString());
			if (user == null)
			{
				return new AuthResultDto
				{
					Succeeded = false,
					Message = "کاربر یافت نشد"
				};
			}

			if (isApproved)
			{
				// Add Instructor role
				if (!await _userManager.IsInRoleAsync(user, UserRoles.Instructor))
				{
					await _userManager.AddToRoleAsync(user, UserRoles.Instructor);
				}
				request.Status = RequestStatus.Approved;

				_logger.LogInformation("User {UserId} approved as instructor by admin {AdminId}", request.UserId, adminId);
			}
			else
			{
				request.Status = RequestStatus.Rejected;
				request.RejectReason = rejectReason;

				_logger.LogInformation("User {UserId} instructor request rejected by admin {AdminId}. Reason: {Reason}",
					request.UserId, adminId, rejectReason);
			}

			request.ReviewedAt = DateTime.UtcNow;
			request.ReviewedBy = adminId;
			request.LastModifiedDate = DateTime.UtcNow;

			await _requestRepository.UpdateAsync(request, cancellationToken);

			// TODO: Send email notification to user

			return new AuthResultDto
			{
				Succeeded = true,
				Message = isApproved ? "درخواست تأیید شد و کاربر به عنوان مدرس اضافه گردید." : "درخواست رد شد."
			};
		}
		catch (Exception ex)
		{
			_logger.LogError(ex, "Error reviewing request {RequestId} by admin {AdminId}", requestId, adminId);
			return new AuthResultDto
			{
				Succeeded = false,
				Message = "خطا در بررسی درخواست"
			};
		}
	}

	public async Task<bool> CanApplyForInstructorAsync(Guid userId, CancellationToken cancellationToken = default)
	{
		var user = await _userManager.FindByIdAsync(userId.ToString());
		if (user == null) return false;

		if (await _userManager.IsInRoleAsync(user, UserRoles.Instructor)) return false;

		if (await _requestRepository.HasPendingRequestAsync(userId, cancellationToken)) return false;

		return true;
	}

	public async Task<InstructorRequest?> GetUserActiveRequestAsync(Guid userId, CancellationToken cancellationToken = default)
	{
		return await _requestRepository.GetSingleAsync(r => r.UserId == userId && r.Status == RequestStatus.Pending, cancellationToken);
	}

	private InstructorRequestListDto MapToDto(InstructorRequest request)
	{
		return new InstructorRequestListDto
		{
			Id = request.Id,
			UserId = request.UserId,
			FullName = request.FullName,
			Email = request.Email,
			PhoneNumber = request.PhoneNumber,
			Degree = request.Degree,
			University = request.University,
			YearsOfExperience = request.YearsOfExperience,
			Expertise = request.Expertise,
			Bio = request.Bio,
			ResumeUrl = request.ResumeUrl,
			CertificateUrl = request.CertificateUrl,
			Status = request.Status,
			RequestedAt = request.RequestedAt,
			ReviewedAt = request.ReviewedAt,
			RejectReason = request.RejectReason
		};
	}
}