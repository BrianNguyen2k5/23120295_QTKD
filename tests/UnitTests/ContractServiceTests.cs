using FluentAssertions;
using Moq;
using QTKD.Api.DTOs.Requests;
using QTKD.Api.Models;
using QTKD.Api.Repositories;
using QTKD.Api.Services;

namespace QTKD.Tests.UnitTests;

public class ContractServiceTests
{
	private readonly Mock<IContractRepository> _contractRepoMock;
	private readonly Mock<ICustomerRepository> _customerRepoMock;
	private readonly ContractService _contractService;

	public ContractServiceTests()
	{
		_contractRepoMock = new Mock<IContractRepository>();
		_customerRepoMock = new Mock<ICustomerRepository>();
		_contractService = new ContractService(_contractRepoMock.Object, _customerRepoMock.Object);
	}

	#region CTR-01 Tests (TC03, TC04)

	// TC03: Sửa Contract ở Draft -> Thành công khi dữ liệu hợp lệ (CTR-01)
	[Fact]
	public async Task TC03_UpdateContract_WhenStatusIsDraft_ReturnsSuccess()
	{
		// Arrange
		var existingContract = new Contract
		{
			Id = 1,
			ContractNo = "HD-2026-001",
			CustomerId = 1,
			StartDate = new DateOnly(2026, 1, 1),
			EndDate = new DateOnly(2026, 12, 31),
			Status = "Draft",
			ServiceGroup = null,
			AttachmentUrl = null
		};

		var customer = new Customer { Id = 1, Name = "Công ty Minh Phát", Status = "Active" };

		_contractRepoMock.Setup(r => r.GetByIdAsync(1, true)).ReturnsAsync(existingContract);
		_customerRepoMock.Setup(r => r.GetByIdAsync(1, false)).ReturnsAsync(customer);

		var updateRequest = new UpdateContractRequest
		{
			CustomerId = 1,
			StartDate = new DateOnly(2026, 1, 1),
			EndDate = new DateOnly(2026, 12, 31),
			ServiceGroup = "Dịch vụ cảng",
			AttachmentUrl = "files/HD-2026-001.pdf"
		};

		// Act
		var result = await _contractService.UpdateAsync(1, updateRequest);

		// Assert
		result.IsSuccess.Should().BeTrue();
		result.StatusCode.Should().Be(200);
		result.Data.Should().NotBeNull();
		result.Data!.ServiceGroup.Should().Be("Dịch vụ cảng");
		result.Data.AttachmentUrl.Should().Be("files/HD-2026-001.pdf");
		existingContract.Status.Should().Be("Draft");
		_contractRepoMock.Verify(r => r.UpdateAsync(existingContract), Times.Once);
	}

	[Fact]
	public async Task UpdateContract_WhenStatusIsRevisionRequested_UpdatesAndResetsToDraft()
	{
		// Arrange
		var existingContract = new Contract
		{
			Id = 2,
			ContractNo = "HD-2026-002",
			CustomerId = 1,
			StartDate = new DateOnly(2026, 1, 1),
			EndDate = new DateOnly(2026, 12, 31),
			Status = "RevisionRequested"
		};

		var customer = new Customer { Id = 1, Name = "Công ty Minh Phát", Status = "Active" };

		_contractRepoMock.Setup(r => r.GetByIdAsync(2, true)).ReturnsAsync(existingContract);
		_customerRepoMock.Setup(r => r.GetByIdAsync(1, false)).ReturnsAsync(customer);

		var updateRequest = new UpdateContractRequest
		{
			CustomerId = 1,
			StartDate = new DateOnly(2026, 1, 1),
			EndDate = new DateOnly(2026, 12, 31),
			ServiceGroup = "Dịch vụ kho bãi",
			AttachmentUrl = "files/HD-2026-002-v2.pdf"
		};

		// Act
		var result = await _contractService.UpdateAsync(2, updateRequest);

		// Assert
		result.IsSuccess.Should().BeTrue();
		existingContract.Status.Should().Be("Draft");
	}

	// TC04: Sửa Contract ở PendingApproval -> Bị từ chối (409 Conflict), dữ liệu giữ nguyên (CTR-01)
	[Theory]
	[InlineData("PendingApproval")]
	[InlineData("Approved")]
	[InlineData("Rejected")]
	[InlineData("Effective")]
	public async Task TC04_UpdateContract_WhenStatusNotEditable_ReturnsConflict(string nonEditableStatus)
	{
		// Arrange
		var existingContract = new Contract
		{
			Id = 3,
			ContractNo = "HD-2026-003",
			CustomerId = 1,
			StartDate = new DateOnly(2026, 1, 1),
			EndDate = new DateOnly(2026, 12, 31),
			Status = nonEditableStatus,
			ServiceGroup = "Dịch vụ cũ",
			AttachmentUrl = "files/old.pdf"
		};

		_contractRepoMock.Setup(r => r.GetByIdAsync(3, true)).ReturnsAsync(existingContract);

		var updateRequest = new UpdateContractRequest
		{
			CustomerId = 1,
			StartDate = new DateOnly(2026, 1, 1),
			EndDate = new DateOnly(2026, 12, 31),
			ServiceGroup = "Dịch vụ mới muốn sửa",
			AttachmentUrl = "files/new.pdf"
		};

		// Act
		var result = await _contractService.UpdateAsync(3, updateRequest);

		// Assert
		result.IsSuccess.Should().BeFalse();
		result.StatusCode.Should().Be(409);
		result.ErrorCode.Should().Be("CTR01_EDIT_NOT_ALLOWED");
		result.Message.Should().Contain("CTR-01");
		existingContract.ServiceGroup.Should().Be("Dịch vụ cũ"); // Dữ liệu giữ nguyên
		existingContract.AttachmentUrl.Should().Be("files/old.pdf");
		_contractRepoMock.Verify(r => r.UpdateAsync(It.IsAny<Contract>()), Times.Never);
	}

	#endregion

	#region CTR-02 Tests (TC05, TC06)

	// TC05: Submit Contract thiếu điều kiện CTR-02 -> Bị từ chối (400 Bad Request), vẫn là Draft
	[Fact]
	public async Task TC05_SubmitContract_MissingRequiredContentAndAttachment_ReturnsBadRequest()
	{
		// Arrange: Hợp đồng thiếu cả ServiceGroup lẫn AttachmentUrl
		var contract = new Contract
		{
			Id = 4,
			ContractNo = "HD-2026-004",
			CustomerId = 1,
			Customer = new Customer { Id = 1, Name = "Minh Phát", Status = "Active" },
			StartDate = new DateOnly(2026, 1, 1),
			EndDate = new DateOnly(2026, 12, 31),
			Status = "Draft",
			ServiceGroup = null,
			AttachmentUrl = null
		};

		_contractRepoMock.Setup(r => r.GetByIdAsync(4, true)).ReturnsAsync(contract);

		var submitRequest = new SubmitContractRequest { Comment = "Trình duyệt" };

		// Act
		var result = await _contractService.SubmitAsync(4, submitRequest, userRole: "Sales", idempotencyKey: null);

		// Assert
		result.IsSuccess.Should().BeFalse();
		result.StatusCode.Should().Be(400);
		result.ErrorCode.Should().Be("CTR02_VALIDATION_FAILED");
		result.Errors.Should().NotBeNull();
		result.Errors.Should().Contain(e => e.Contains("Nhóm dịch vụ"));
		result.Errors.Should().Contain(e => e.Contains("Tệp đính kèm"));
		contract.Status.Should().Be("Draft"); // Trạng thái vẫn là Draft
		_contractRepoMock.Verify(r => r.UpdateAsync(It.IsAny<Contract>()), Times.Never);
	}

	[Fact]
	public async Task SubmitContract_CustomerInactive_ReturnsBadRequest()
	{
		// Arrange: Khách hàng không ở trạng thái Active
		var contract = new Contract
		{
			Id = 5,
			ContractNo = "HD-2026-005",
			CustomerId = 1,
			Customer = new Customer { Id = 1, Name = "Minh Phát", Status = "Inactive" },
			StartDate = new DateOnly(2026, 1, 1),
			EndDate = new DateOnly(2026, 12, 31),
			Status = "Draft",
			ServiceGroup = "Dịch vụ cảng",
			AttachmentUrl = "files/contract.pdf"
		};

		_contractRepoMock.Setup(r => r.GetByIdAsync(5, true)).ReturnsAsync(contract);

		// Act
		var result = await _contractService.SubmitAsync(5, new SubmitContractRequest(), "Sales", null);

		// Assert
		result.IsSuccess.Should().BeFalse();
		result.StatusCode.Should().Be(400);
		result.Errors.Should().Contain(e => e.Contains("hoạt động (Active)"));
	}

	[Fact]
	public async Task SubmitContract_InvalidEffectiveDates_ReturnsBadRequest()
	{
		// Arrange: StartDate > EndDate
		var contract = new Contract
		{
			Id = 6,
			ContractNo = "HD-2026-006",
			CustomerId = 1,
			Customer = new Customer { Id = 1, Name = "Minh Phát", Status = "Active" },
			StartDate = new DateOnly(2026, 12, 31),
			EndDate = new DateOnly(2026, 1, 1),
			Status = "Draft",
			ServiceGroup = "Dịch vụ cảng",
			AttachmentUrl = "files/contract.pdf"
		};

		_contractRepoMock.Setup(r => r.GetByIdAsync(6, true)).ReturnsAsync(contract);

		// Act
		var result = await _contractService.SubmitAsync(6, new SubmitContractRequest(), "Sales", null);

		// Assert
		result.IsSuccess.Should().BeFalse();
		result.StatusCode.Should().Be(400);
		result.Errors.Should().Contain(e => e.Contains("Thời gian hiệu lực không hợp lệ"));
	}

	// TC06: Submit Contract hợp lệ -> Chuyển sang PendingApproval (CTR-02 & CTR-03)
	[Fact]
	public async Task TC06_SubmitContract_WhenAllConditionsMet_TransitionsToPendingApproval()
	{
		// Arrange
		var contract = new Contract
		{
			Id = 7,
			ContractNo = "HD-2026-007",
			CustomerId = 1,
			Customer = new Customer { Id = 1, Name = "Minh Phát", Status = "Active" },
			StartDate = new DateOnly(2026, 1, 1),
			EndDate = new DateOnly(2026, 12, 31),
			Status = "Draft",
			ServiceGroup = "Dịch vụ cảng",
			AttachmentUrl = "files/HD-2026-007.pdf"
		};

		_contractRepoMock.Setup(r => r.GetByIdAsync(7, true)).ReturnsAsync(contract);

		var submitRequest = new SubmitContractRequest { Comment = "Kính trình phê duyệt" };

		// Act
		var result = await _contractService.SubmitAsync(7, submitRequest, userRole: "Sales", idempotencyKey: "key-123");

		// Assert
		result.IsSuccess.Should().BeTrue();
		result.StatusCode.Should().Be(200);
		result.Data.Should().NotBeNull();
		result.Data!.Status.Should().Be("PendingApproval");
		result.Data.ApprovalId.Should().StartWith("APR-");
		contract.Status.Should().Be("PendingApproval");
		contract.ApprovalId.Should().NotBeNullOrEmpty();
		contract.SubmittedAt.Should().NotBeNull();
		contract.SubmitComment.Should().Be("Kính trình phê duyệt");
		contract.IdempotencyKey.Should().Be("key-123");
		_contractRepoMock.Verify(r => r.UpdateAsync(contract), Times.Once);
	}

	#endregion

	#region CTR-03 Tests (TC07)

	// TC07: Thử chuyển Draft thẳng Approved -> Bị từ chối (CTR-03)
	[Fact]
	public async Task TC07_CreateContract_DirectApprovedStatus_ReturnsBadRequest()
	{
		// Arrange
		var request = new CreateContractRequest
		{
			ContractNo = "HD-2026-008",
			CustomerId = 1,
			StartDate = new DateOnly(2026, 1, 1),
			EndDate = new DateOnly(2026, 12, 31),
			Status = "Approved" // Thử tạo thẳng Approved
		};

		// Act
		var result = await _contractService.CreateAsync(request);

		// Assert
		result.IsSuccess.Should().BeFalse();
		result.StatusCode.Should().Be(400);
		result.ErrorCode.Should().Be("CTR03_DIRECT_STATUS_FORBIDDEN");
		result.Message.Should().Contain("CTR-03");
		_contractRepoMock.Verify(r => r.CreateAsync(It.IsAny<Contract>()), Times.Never);
	}

	[Fact]
	public async Task TC07_UpdateContract_DirectApprovedStatus_ReturnsBadRequest()
	{
		// Arrange
		var existingContract = new Contract
		{
			Id = 8,
			ContractNo = "HD-2026-008",
			CustomerId = 1,
			StartDate = new DateOnly(2026, 1, 1),
			EndDate = new DateOnly(2026, 12, 31),
			Status = "Draft"
		};

		_contractRepoMock.Setup(r => r.GetByIdAsync(8, true)).ReturnsAsync(existingContract);

		var updateRequest = new UpdateContractRequest
		{
			CustomerId = 1,
			StartDate = new DateOnly(2026, 1, 1),
			EndDate = new DateOnly(2026, 12, 31),
			Status = "Approved" // Thử đổi thẳng sang Approved
		};

		// Act
		var result = await _contractService.UpdateAsync(8, updateRequest);

		// Assert
		result.IsSuccess.Should().BeFalse();
		result.StatusCode.Should().Be(400);
		result.ErrorCode.Should().Be("CTR03_DIRECT_APPROVAL_FORBIDDEN");
		result.Message.Should().Contain("CTR-03");
		existingContract.Status.Should().Be("Draft");
	}

	#endregion

	#region AC01 & AC05 / TC08 Tests (Quyền & Chống gửi lặp)

	[Theory]
	[InlineData("Guest")]
	[InlineData("Accounting")]
	[InlineData("Operations")]
	public async Task SubmitContract_UnauthorizedRole_ReturnsForbidden(string unauthorizedRole)
	{
		// Arrange
		var contract = new Contract { Id = 9, Status = "Draft" };
		_contractRepoMock.Setup(r => r.GetByIdAsync(9, true)).ReturnsAsync(contract);

		// Act
		var result = await _contractService.SubmitAsync(9, new SubmitContractRequest(), userRole: unauthorizedRole, idempotencyKey: null);

		// Assert
		result.IsSuccess.Should().BeFalse();
		result.StatusCode.Should().Be(403);
		result.ErrorCode.Should().Be("AC01_FORBIDDEN");
	}

	// TC08: Submit lặp một Contract -> Không tạo thêm kết quả xử lý trùng
	[Fact]
	public async Task TC08_SubmitContract_DoubleSubmitWithSameKey_ReturnsSameApprovalIdWithoutDuplicate()
	{
		// Arrange: Hợp đồng đã được submit trước đó với key-abc và đang PendingApproval
		var contract = new Contract
		{
			Id = 10,
			ContractNo = "HD-2026-010",
			CustomerId = 1,
			Status = "PendingApproval",
			ApprovalId = "APR-20261105-0010",
			IdempotencyKey = "key-abc",
			SubmittedAt = DateTime.UtcNow.AddMinutes(-5)
		};

		_contractRepoMock.Setup(r => r.GetByIdAsync(10, true)).ReturnsAsync(contract);

		// Act: Gọi submit lần 2 với cùng key-abc
		var result = await _contractService.SubmitAsync(10, new SubmitContractRequest(), "Sales", idempotencyKey: "key-abc");

		// Assert: Trả về thành công với cùng ApprovalId cũ
		result.IsSuccess.Should().BeTrue();
		result.StatusCode.Should().Be(200);
		result.Data!.ApprovalId.Should().Be("APR-20261105-0010");
		result.Data.Message.Should().Contain("Idempotent");
		_contractRepoMock.Verify(r => r.UpdateAsync(It.IsAny<Contract>()), Times.Never); // Không tạo thêm bản ghi hay gọi update
	}

	[Fact]
	public async Task TC08_SubmitContract_DoubleSubmitWithoutSameKey_ReturnsConflict()
	{
		// Arrange: Hợp đồng đã PendingApproval, nhưng submit lần 2 không có key hoặc khác key
		var contract = new Contract
		{
			Id = 11,
			ContractNo = "HD-2026-011",
			CustomerId = 1,
			Status = "PendingApproval",
			ApprovalId = "APR-20261105-0011"
		};

		_contractRepoMock.Setup(r => r.GetByIdAsync(11, true)).ReturnsAsync(contract);

		// Act
		var result = await _contractService.SubmitAsync(11, new SubmitContractRequest(), "Sales", idempotencyKey: null);

		// Assert
		result.IsSuccess.Should().BeFalse();
		result.StatusCode.Should().Be(409);
		result.ErrorCode.Should().Be("AC02_INVALID_CONTRACT_STATE");
	}

	#endregion
}

