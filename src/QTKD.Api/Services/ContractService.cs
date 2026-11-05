using QTKD.Api.DTOs.Requests;
using QTKD.Api.DTOs.Responses;
using QTKD.Api.Models;
using QTKD.Api.Repositories;

namespace QTKD.Api.Services;

public class ContractService : IContractService
{
	// Các trạng thái cho phép chỉnh sửa theo quy tắc CTR-01
	private static readonly HashSet<string> EditableStatuses = new(StringComparer.OrdinalIgnoreCase)
	{
		"Draft",
		"RevisionRequested"
	};

	// Các role hợp lệ được phép gửi hợp đồng trình duyệt (AC01)
	private static readonly HashSet<string> AllowedSubmitRoles = new(StringComparer.OrdinalIgnoreCase)
	{
		"Sales",
		"KinhDoanh"
	};

	private readonly IContractRepository _contractRepository;
	private readonly ICustomerRepository _customerRepository;

	public ContractService(
		IContractRepository contractRepository,
		ICustomerRepository customerRepository)
	{
		_contractRepository = contractRepository;
		_customerRepository = customerRepository;
	}

	public async Task<List<ContractResponse>> GetAllAsync()
	{
		var contracts = await _contractRepository.GetAllAsync();
		return contracts.Select(MapToResponse).ToList();
	}

	public async Task<ContractResponse?> GetByIdAsync(int id)
	{
		var contract = await _contractRepository.GetByIdAsync(id);
		return contract == null ? null : MapToResponse(contract);
	}

	public async Task<ServiceResult<ContractResponse>> CreateAsync(CreateContractRequest request)
	{
		if (string.IsNullOrWhiteSpace(request.ContractNo))
		{
			return ServiceResult<ContractResponse>.Failure(
				400,
				"INVALID_CONTRACT_NO",
				"Số hợp đồng (ContractNo) là bắt buộc và không được để trống.");
		}

		// Quy tắc CTR-03: Không cho phép tạo thẳng hợp đồng ở trạng thái Approved
		if (!string.IsNullOrWhiteSpace(request.Status) &&
			!request.Status.Trim().Equals("Draft", StringComparison.OrdinalIgnoreCase))
		{
			return ServiceResult<ContractResponse>.Failure(
				400,
				"CTR03_DIRECT_STATUS_FORBIDDEN",
				"Quy tắc CTR-03: Hợp đồng mới tạo bắt buộc ở trạng thái 'Draft', không được tạo hoặc chuyển thẳng sang trạng thái khác.");
		}

		var normalizedNo = request.ContractNo.Trim();
		if (await _contractRepository.ExistsByContractNoAsync(normalizedNo))
		{
			return ServiceResult<ContractResponse>.Failure(
				409,
				"DUPLICATE_CONTRACT_NO",
				$"Số hợp đồng '{normalizedNo}' đã tồn tại trong hệ thống.");
		}

		// Kiểm tra Customer được tham chiếu phải tồn tại và hợp lệ
		var customer = await _customerRepository.GetByIdAsync(request.CustomerId);
		if (customer == null)
		{
			return ServiceResult<ContractResponse>.Failure(
				400,
				"INVALID_CUSTOMER",
				$"Khách hàng với ID = {request.CustomerId} không tồn tại.");
		}

		if (!customer.Status.Equals("Active", StringComparison.OrdinalIgnoreCase))
		{
			return ServiceResult<ContractResponse>.Failure(
				400,
				"INACTIVE_CUSTOMER",
				$"Khách hàng '{customer.Name}' đang ở trạng thái '{customer.Status}', không thể lập hợp đồng mới.");
		}

		// Kiểm tra ngày hiệu lực hợp lệ
		if (request.StartDate == default || request.EndDate == default || request.StartDate > request.EndDate)
		{
			return ServiceResult<ContractResponse>.Failure(
				400,
				"INVALID_EFFECTIVE_DATES",
				"Ngày hiệu lực không hợp lệ: StartDate phải nhỏ hơn hoặc bằng EndDate.");
		}

		var contract = new Contract
		{
			ContractNo = normalizedNo,
			CustomerId = request.CustomerId,
			StartDate = request.StartDate,
			EndDate = request.EndDate,
			Status = "Draft", // Luôn khởi tạo là Draft
			ServiceGroup = string.IsNullOrWhiteSpace(request.ServiceGroup) ? null : request.ServiceGroup.Trim(),
			AttachmentUrl = string.IsNullOrWhiteSpace(request.AttachmentUrl) ? null : request.AttachmentUrl.Trim(),
			CreatedAt = DateTime.UtcNow
		};

		var created = await _contractRepository.CreateAsync(contract);
		return ServiceResult<ContractResponse>.Success(MapToResponse(created), 201);
	}

	public async Task<ServiceResult<ContractResponse>> UpdateAsync(int id, UpdateContractRequest request)
	{
		var contract = await _contractRepository.GetByIdAsync(id, trackChanges: true);
		if (contract == null)
		{
			return ServiceResult<ContractResponse>.Failure(
				404,
				"CONTRACT_NOT_FOUND",
				$"Không tìm thấy hợp đồng với ID = {id}");
		}

		// Quy tắc CTR-01: Chỉ cho sửa hợp đồng ở Draft hoặc RevisionRequested
		if (!EditableStatuses.Contains(contract.Status))
		{
			return ServiceResult<ContractResponse>.Failure(
				409,
				"CTR01_EDIT_NOT_ALLOWED",
				$"Quy tắc CTR-01: Không được phép chỉnh sửa hợp đồng đang ở trạng thái '{contract.Status}'. Chỉ cho phép sửa khi ở trạng thái 'Draft' hoặc 'RevisionRequested'.");
		}

		// Quy tắc CTR-03: Không được chuyển thẳng từ Draft sang Approved (hoặc tự đổi sang PendingApproval qua PUT)
		if (!string.IsNullOrWhiteSpace(request.Status))
		{
			var targetStatus = request.Status.Trim();
			if (targetStatus.Equals("Approved", StringComparison.OrdinalIgnoreCase))
			{
				return ServiceResult<ContractResponse>.Failure(
					400,
					"CTR03_DIRECT_APPROVAL_FORBIDDEN",
					"Quy tắc CTR-03: Không được chuyển trực tiếp từ Draft sang Approved. Hợp đồng phải được gửi trình duyệt qua endpoint POST /api/contracts/{id}/submit.");
			}

			if (!EditableStatuses.Contains(targetStatus))
			{
				return ServiceResult<ContractResponse>.Failure(
					400,
					"INVALID_STATUS_TRANSITION",
					$"Không thể đổi trạng thái trực tiếp sang '{targetStatus}' khi cập nhật hợp đồng.");
			}
		}

		// Kiểm tra Customer hợp lệ
		var customer = await _customerRepository.GetByIdAsync(request.CustomerId);
		if (customer == null)
		{
			return ServiceResult<ContractResponse>.Failure(
				400,
				"INVALID_CUSTOMER",
				$"Khách hàng với ID = {request.CustomerId} không tồn tại.");
		}

		if (request.StartDate == default || request.EndDate == default || request.StartDate > request.EndDate)
		{
			return ServiceResult<ContractResponse>.Failure(
				400,
				"INVALID_EFFECTIVE_DATES",
				"Ngày hiệu lực không hợp lệ: StartDate phải nhỏ hơn hoặc bằng EndDate.");
		}

		contract.CustomerId = request.CustomerId;
		contract.StartDate = request.StartDate;
		contract.EndDate = request.EndDate;
		contract.ServiceGroup = string.IsNullOrWhiteSpace(request.ServiceGroup) ? null : request.ServiceGroup.Trim();
		contract.AttachmentUrl = string.IsNullOrWhiteSpace(request.AttachmentUrl) ? null : request.AttachmentUrl.Trim();

		// Nếu hợp đồng đang ở RevisionRequested, sau khi hiệu chỉnh chuyển về Draft theo sơ đồ trạng thái US03
		if (contract.Status.Equals("RevisionRequested", StringComparison.OrdinalIgnoreCase))
		{
			contract.Status = "Draft";
		}

		await _contractRepository.UpdateAsync(contract);

		var response = MapToResponse(contract);
		response.CustomerName = customer.Name;
		return ServiceResult<ContractResponse>.Success(response);
	}

	public async Task<ServiceResult<SubmitContractResponse>> SubmitAsync(
		int id,
		SubmitContractRequest? request,
		string? userRole,
		string? idempotencyKey)
	{
		var contract = await _contractRepository.GetByIdAsync(id, trackChanges: true);
		if (contract == null)
		{
			return ServiceResult<SubmitContractResponse>.Failure(
				404,
				"CONTRACT_NOT_FOUND",
				$"Không tìm thấy hợp đồng với ID = {id}");
		}

		// 1. Kiểm tra quyền gửi hợp đồng (AC01 / TC01)
		var effectiveRole = string.IsNullOrWhiteSpace(userRole) ? "Sales" : userRole.Trim();
		if (!AllowedSubmitRoles.Contains(effectiveRole))
		{
			return ServiceResult<SubmitContractResponse>.Failure(
				403,
				"AC01_FORBIDDEN",
				$"Người dùng có vai trò '{effectiveRole}' không có quyền gửi hợp đồng trình duyệt. Chỉ Nhân viên Kinh doanh ('Sales') mới được phép thực hiện.");
		}

		// 2. Xử lý chống gửi lặp cùng khóa Idempotency-Key (AC05 / TC05 & Bước 12)
		var normalizedKey = string.IsNullOrWhiteSpace(idempotencyKey) ? null : idempotencyKey.Trim();
		if (contract.Status.Equals("PendingApproval", StringComparison.OrdinalIgnoreCase) &&
			normalizedKey != null &&
			string.Equals(contract.IdempotencyKey, normalizedKey, StringComparison.Ordinal) &&
			!string.IsNullOrEmpty(contract.ApprovalId))
		{
			return ServiceResult<SubmitContractResponse>.Success(new SubmitContractResponse
			{
				ContractId = contract.Id,
				ContractNo = contract.ContractNo,
				Status = contract.Status,
				ApprovalId = contract.ApprovalId,
				SubmittedAt = contract.SubmittedAt,
				Message = "Yêu cầu gửi trình duyệt đã được xử lý trước đó (Idempotent response - không tạo thêm phiên phê duyệt mới)."
			});
		}

		// 3. Kiểm tra trạng thái hợp đồng phải là Draft (AC02 / TC02 & Bước 10, 12)
		if (!contract.Status.Equals("Draft", StringComparison.OrdinalIgnoreCase))
		{
			return ServiceResult<SubmitContractResponse>.Failure(
				409,
				"AC02_INVALID_CONTRACT_STATE",
				$"Không thể gửi trình duyệt khi hợp đồng đang ở trạng thái '{contract.Status}'. Chỉ hợp đồng ở trạng thái 'Draft' mới được phép gửi trình duyệt.");
		}

		// 4. Kiểm tra toàn diện quy tắc CTR-02 (AC03 / TC03 & Bước 10, 11)
		// Điều kiện CTR-02: (1) Customer hợp lệ, (2) Ngày hiệu lực hợp lệ, (3) Đủ nội dung bắt buộc, (4) Đủ tệp bắt buộc
		var validationErrors = new List<string>();

		if (contract.Customer == null || !contract.Customer.Status.Equals("Active", StringComparison.OrdinalIgnoreCase))
		{
			validationErrors.Add("Khách hàng không hợp lệ hoặc không ở trạng thái hoạt động (Active).");
		}

		if (contract.StartDate == default || contract.EndDate == default || contract.StartDate > contract.EndDate)
		{
			validationErrors.Add("Thời gian hiệu lực không hợp lệ (StartDate phải nhỏ hơn hoặc bằng EndDate).");
		}

		if (string.IsNullOrWhiteSpace(contract.ServiceGroup))
		{
			validationErrors.Add("Thiếu nội dung bắt buộc: Nhóm dịch vụ (ServiceGroup) chưa được khai báo.");
		}

		if (string.IsNullOrWhiteSpace(contract.AttachmentUrl))
		{
			validationErrors.Add("Thiếu tệp bắt buộc: Tệp đính kèm hợp đồng (AttachmentUrl) chưa được bổ sung.");
		}

		if (validationErrors.Count > 0)
		{
			return ServiceResult<SubmitContractResponse>.Failure(
				400,
				"CTR02_VALIDATION_FAILED",
				"Hợp đồng chưa đủ điều kiện gửi trình duyệt theo quy tắc CTR-02. Vui lòng hoàn tất các nội dung còn thiếu.",
				validationErrors);
		}

		// 5. Đạt đủ điều kiện (AC04 / TC04 & CTR-03):
		// Chuyển trạng thái từ Draft -> PendingApproval (Tuyệt đối không chuyển thẳng sang Approved theo CTR-03)
		var now = DateTime.UtcNow;
		contract.Status = "PendingApproval";
		contract.ApprovalId = $"APR-{now:yyyyMMdd}-{contract.Id:D4}";
		contract.SubmittedAt = now;
		contract.SubmitComment = string.IsNullOrWhiteSpace(request?.Comment) ? null : request.Comment.Trim();
		contract.IdempotencyKey = normalizedKey;

		await _contractRepository.UpdateAsync(contract);

		return ServiceResult<SubmitContractResponse>.Success(new SubmitContractResponse
		{
			ContractId = contract.Id,
			ContractNo = contract.ContractNo,
			Status = contract.Status,
			ApprovalId = contract.ApprovalId,
			SubmittedAt = contract.SubmittedAt,
			Message = "Hợp đồng đã được gửi vào quy trình phê duyệt."
		});
	}

	private static ContractResponse MapToResponse(Contract contract)
	{
		return new ContractResponse
		{
			Id = contract.Id,
			ContractNo = contract.ContractNo,
			CustomerId = contract.CustomerId,
			CustomerName = contract.Customer?.Name,
			StartDate = contract.StartDate,
			EndDate = contract.EndDate,
			Status = contract.Status,
			ServiceGroup = contract.ServiceGroup,
			AttachmentUrl = contract.AttachmentUrl,
			ApprovalId = contract.ApprovalId,
			SubmittedAt = contract.SubmittedAt,
			SubmitComment = contract.SubmitComment,
			CreatedAt = contract.CreatedAt
		};
	}
}
