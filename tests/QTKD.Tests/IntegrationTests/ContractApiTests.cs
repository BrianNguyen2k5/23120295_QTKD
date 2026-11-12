using System.Net;
using System.Net.Http.Json;
using System.Text.Json;
using FluentAssertions;
using Microsoft.Extensions.DependencyInjection;
using QTKD.Api.Data;
using QTKD.Api.DTOs.Requests;
using QTKD.Api.DTOs.Responses;

namespace QTKD.Tests.IntegrationTests;

public class ContractApiTests : IClassFixture<CustomWebApplicationFactory>
{
	private readonly CustomWebApplicationFactory _factory;
	private readonly HttpClient _client;

	public ContractApiTests(CustomWebApplicationFactory factory)
	{
		_factory = factory;
		_client = factory.CreateClient();
	}

	/// <summary>
	/// TC03: Sửa Contract ở Draft -> Thành công khi dữ liệu hợp lệ (CTR-01)
	/// </summary>
	[Fact]
	public async Task TC03_UpdateContract_WhenInDraftStatus_ReturnsOk_AndUpdatesDatabase()
	{
		// Arrange: Tạo một contract Draft mới
		var contractNo = $"HD-TEST-TC03-{Guid.NewGuid():N}"[..18];
		var createRequest = new CreateContractRequest
		{
			ContractNo = contractNo,
			CustomerId = 1,
			StartDate = new DateOnly(2026, 6, 1),
			EndDate = new DateOnly(2027, 5, 31)
		};

		var createResp = await _client.PostAsJsonAsync("/api/contracts", createRequest);
		createResp.StatusCode.Should().Be(HttpStatusCode.Created);
		var created = await createResp.Content.ReadFromJsonAsync<ContractResponse>();

		// Act: Cập nhật thông tin khi hợp đồng đang ở Draft
		var updateRequest = new UpdateContractRequest
		{
			CustomerId = 1,
			StartDate = new DateOnly(2026, 6, 1),
			EndDate = new DateOnly(2027, 5, 31),
			ServiceGroup = "Dịch vụ bốc dỡ container",
			AttachmentUrl = "files/hd-tc03.pdf"
		};

		var updateResp = await _client.PutAsJsonAsync($"/api/contracts/{created!.Id}", updateRequest);

		// Assert: HTTP Status và dữ liệu đọc lại từ DB (Bước 17)
		updateResp.StatusCode.Should().Be(HttpStatusCode.OK);
		var updated = await updateResp.Content.ReadFromJsonAsync<ContractResponse>();
		updated!.ServiceGroup.Should().Be("Dịch vụ bốc dỡ container");
		updated.AttachmentUrl.Should().Be("files/hd-tc03.pdf");
		updated.Status.Should().Be("Draft");

		using var scope = _factory.Services.CreateScope();
		var db = scope.ServiceProvider.GetRequiredService<QtkdDbContext>();
		var dbContract = await db.Contracts.FindAsync(created.Id);
		dbContract.Should().NotBeNull();
		dbContract!.ServiceGroup.Should().Be("Dịch vụ bốc dỡ container");
		dbContract.AttachmentUrl.Should().Be("files/hd-tc03.pdf");
	}

	/// <summary>
	/// TC04: Sửa Contract ở PendingApproval -> Bị từ chối (409 Conflict), dữ liệu giữ nguyên (CTR-01)
	/// </summary>
	[Fact]
	public async Task TC04_UpdateContract_WhenInPendingApprovalStatus_ReturnsConflict_AndDataUnchanged()
	{
		// Arrange: Tạo contract hợp lệ và submit sang PendingApproval
		var contractNo = $"HD-TEST-TC04-{Guid.NewGuid():N}"[..18];
		var createRequest = new CreateContractRequest
		{
			ContractNo = contractNo,
			CustomerId = 1,
			StartDate = new DateOnly(2026, 6, 1),
			EndDate = new DateOnly(2027, 5, 31),
			ServiceGroup = "Dịch vụ cảng ban đầu",
			AttachmentUrl = "files/initial.pdf"
		};
		var createResp = await _client.PostAsJsonAsync("/api/contracts", createRequest);
		var created = await createResp.Content.ReadFromJsonAsync<ContractResponse>();

		// Submit sang PendingApproval
		var submitResp = await _client.PostAsJsonAsync($"/api/contracts/{created!.Id}/submit", new SubmitContractRequest());
		submitResp.StatusCode.Should().Be(HttpStatusCode.OK);

		// Act: Cố gắng sửa contract khi đã ở PendingApproval
		var updateRequest = new UpdateContractRequest
		{
			CustomerId = 1,
			StartDate = new DateOnly(2026, 6, 1),
			EndDate = new DateOnly(2027, 5, 31),
			ServiceGroup = "Cố tình sửa đổi sau khi đã submit",
			AttachmentUrl = "files/hacked.pdf"
		};

		var updateResp = await _client.PutAsJsonAsync($"/api/contracts/{created.Id}", updateRequest);

		// Assert: Phải bị từ chối 409 Conflict theo CTR-01
		updateResp.StatusCode.Should().Be(HttpStatusCode.Conflict);
		var errContent = await updateResp.Content.ReadAsStringAsync();
		errContent.Should().Contain("CTR01_EDIT_NOT_ALLOWED");

		// Dữ liệu trong DB vẫn giữ nguyên giá trị ban đầu (Bước 17)
		using var scope = _factory.Services.CreateScope();
		var db = scope.ServiceProvider.GetRequiredService<QtkdDbContext>();
		var dbContract = await db.Contracts.FindAsync(created.Id);
		dbContract!.ServiceGroup.Should().Be("Dịch vụ cảng ban đầu");
		dbContract.AttachmentUrl.Should().Be("files/initial.pdf");
		dbContract.Status.Should().Be("PendingApproval");
	}

	/// <summary>
	/// TC05: Submit Contract thiếu điều kiện CTR-02 -> Bị từ chối (400 Bad Request), vẫn là Draft
	/// </summary>
	[Fact]
	public async Task TC05_SubmitContract_WhenMissingCTR02Conditions_ReturnsBadRequest_AndRemainsDraft()
	{
		// Arrange: Tạo hợp đồng Draft thiếu ServiceGroup và AttachmentUrl
		var contractNo = $"HD-TEST-TC05-{Guid.NewGuid():N}"[..18];
		var createRequest = new CreateContractRequest
		{
			ContractNo = contractNo,
			CustomerId = 1,
			StartDate = new DateOnly(2026, 1, 1),
			EndDate = new DateOnly(2026, 12, 31),
			ServiceGroup = null,
			AttachmentUrl = null
		};
		var createResp = await _client.PostAsJsonAsync("/api/contracts", createRequest);
		var created = await createResp.Content.ReadFromJsonAsync<ContractResponse>();

		// Act: Submit khi thiếu dữ liệu bắt buộc
		var submitResp = await _client.PostAsJsonAsync($"/api/contracts/{created!.Id}/submit", new SubmitContractRequest());

		// Assert: HTTP Status 400 và danh sách lỗi
		submitResp.StatusCode.Should().Be(HttpStatusCode.BadRequest);
		var errContent = await submitResp.Content.ReadAsStringAsync();
		errContent.Should().Contain("CTR02_VALIDATION_FAILED");
		errContent.Should().Contain("Nhóm dịch vụ");
		errContent.Should().Contain("Tệp đính kèm");

		// Kiểm tra trong DB: Trạng thái vẫn là Draft, không có ApprovalId
		using var scope = _factory.Services.CreateScope();
		var db = scope.ServiceProvider.GetRequiredService<QtkdDbContext>();
		var dbContract = await db.Contracts.FindAsync(created.Id);
		dbContract!.Status.Should().Be("Draft");
		dbContract.ApprovalId.Should().BeNull();
	}

	/// <summary>
	/// TC06: Submit Contract hợp lệ -> Chuyển sang PendingApproval (CTR-02 & CTR-03)
	/// </summary>
	[Fact]
	public async Task TC06_SubmitContract_WhenValid_TransitionsToPendingApproval_AndPersistsToDb()
	{
		// Arrange: Tạo hợp đồng đầy đủ điều kiện CTR-02
		var contractNo = $"HD-TEST-TC06-{Guid.NewGuid():N}"[..18];
		var createRequest = new CreateContractRequest
		{
			ContractNo = contractNo,
			CustomerId = 1,
			StartDate = new DateOnly(2026, 1, 1),
			EndDate = new DateOnly(2026, 12, 31),
			ServiceGroup = "Dịch vụ kho bãi",
			AttachmentUrl = "files/hd-tc06.pdf"
		};
		var createResp = await _client.PostAsJsonAsync("/api/contracts", createRequest);
		var created = await createResp.Content.ReadFromJsonAsync<ContractResponse>();

		// Act: Submit với vai trò Sales
		var requestMsg = new HttpRequestMessage(HttpMethod.Post, $"/api/contracts/{created!.Id}/submit")
		{
			Content = JsonContent.Create(new SubmitContractRequest { Comment = "Kính trình duyệt TC06" })
		};
		requestMsg.Headers.Add("X-User-Role", "Sales");

		var submitResp = await _client.SendAsync(requestMsg);

		// Assert: Thành công 200 OK và đổi sang PendingApproval
		submitResp.StatusCode.Should().Be(HttpStatusCode.OK);
		var submitResult = await submitResp.Content.ReadFromJsonAsync<SubmitContractResponse>();
		submitResult.Should().NotBeNull();
		submitResult!.Status.Should().Be("PendingApproval");
		submitResult.ApprovalId.Should().StartWith("APR-");

		// Kiểm tra trong DB: Cột Status = PendingApproval và ApprovalId đã lưu
		using var scope = _factory.Services.CreateScope();
		var db = scope.ServiceProvider.GetRequiredService<QtkdDbContext>();
		var dbContract = await db.Contracts.FindAsync(created.Id);
		dbContract!.Status.Should().Be("PendingApproval");
		dbContract.ApprovalId.Should().Be(submitResult.ApprovalId);
		dbContract.SubmittedAt.Should().NotBeNull();
	}

	/// <summary>
	/// TC07: Thử chuyển Draft thẳng Approved -> Bị từ chối (CTR-03)
	/// </summary>
	[Fact]
	public async Task TC07_CreateContract_DirectApprovedStatus_ReturnsBadRequest()
	{
		// Arrange: Cố tình truyền Status = "Approved" khi tạo
		var request = new CreateContractRequest
		{
			ContractNo = $"HD-TEST-TC07-{Guid.NewGuid():N}"[..18],
			CustomerId = 1,
			StartDate = new DateOnly(2026, 1, 1),
			EndDate = new DateOnly(2026, 12, 31),
			Status = "Approved"
		};

		// Act
		var response = await _client.PostAsJsonAsync("/api/contracts", request);

		// Assert: Bị từ chối theo CTR-03
		response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
		var errContent = await response.Content.ReadAsStringAsync();
		errContent.Should().Contain("CTR03_DIRECT_STATUS_FORBIDDEN");
	}

	/// <summary>
	/// TC08: Submit lặp một Contract -> Không tạo thêm kết quả xử lý trùng (AC05 & Bước 12)
	/// </summary>
	[Fact]
	public async Task TC08_SubmitContract_DoubleSubmit_ReturnsSameApprovalIdWithoutDuplicate()
	{
		// Arrange: Tạo và chuẩn bị hợp đồng đủ điều kiện
		var contractNo = $"HD-TEST-TC08-{Guid.NewGuid():N}"[..18];
		var createRequest = new CreateContractRequest
		{
			ContractNo = contractNo,
			CustomerId = 1,
			StartDate = new DateOnly(2026, 1, 1),
			EndDate = new DateOnly(2026, 12, 31),
			ServiceGroup = "Dịch vụ cảng",
			AttachmentUrl = "files/hd-tc08.pdf"
		};
		var createResp = await _client.PostAsJsonAsync("/api/contracts", createRequest);
		var created = await createResp.Content.ReadFromJsonAsync<ContractResponse>();
		var idempotencyKey = $"idem-key-{created!.Id}";

		// Act 1: Gửi lần 1 với Idempotency-Key
		var req1 = new HttpRequestMessage(HttpMethod.Post, $"/api/contracts/{created.Id}/submit")
		{
			Content = JsonContent.Create(new SubmitContractRequest { Comment = "Lần 1" })
		};
		req1.Headers.Add("X-User-Role", "Sales");
		req1.Headers.Add("Idempotency-Key", idempotencyKey);
		var resp1 = await _client.SendAsync(req1);
		resp1.StatusCode.Should().Be(HttpStatusCode.OK);
		var res1 = await resp1.Content.ReadFromJsonAsync<SubmitContractResponse>();

		// Act 2: Gửi lần 2 với CÙNG Idempotency-Key
		var req2 = new HttpRequestMessage(HttpMethod.Post, $"/api/contracts/{created.Id}/submit")
		{
			Content = JsonContent.Create(new SubmitContractRequest { Comment = "Lần 2 gửi lặp" })
		};
		req2.Headers.Add("X-User-Role", "Sales");
		req2.Headers.Add("Idempotency-Key", idempotencyKey);
		var resp2 = await _client.SendAsync(req2);

		// Assert: Cùng trả về mã 200 OK với ApprovalId y hệt lần 1 (Idempotent - không tạo thêm lần xử lý mới)
		resp2.StatusCode.Should().Be(HttpStatusCode.OK);
		var res2 = await resp2.Content.ReadFromJsonAsync<SubmitContractResponse>();
		res2!.ApprovalId.Should().Be(res1!.ApprovalId);

		// Act 3: Gửi lần 3 KHÔNG có Idempotency-Key khi hợp đồng đã ở PendingApproval -> Bị từ chối 409
		var req3 = new HttpRequestMessage(HttpMethod.Post, $"/api/contracts/{created.Id}/submit")
		{
			Content = JsonContent.Create(new SubmitContractRequest { Comment = "Lần 3 không khóa" })
		};
		req3.Headers.Add("X-User-Role", "Sales");
		var resp3 = await _client.SendAsync(req3);
		resp3.StatusCode.Should().Be(HttpStatusCode.Conflict);
	}

	/// <summary>
	/// Kiểm tra luồng liên hoàn tuần 06: Tạo Customer -> Tạo Contract Draft thiếu tệp -> Submit lỗi 400 -> Bổ sung tệp -> Submit thành công
	/// </summary>
	[Fact]
	public async Task EndToEndFlow_CreateCustomer_CreateDraft_FailSubmit_Update_SuccessSubmit()
	{
		// 1. Tạo Customer
		var custResp = await _client.PostAsJsonAsync("/api/customers", new CreateCustomerRequest
		{
			Name = "Công ty Hải Phát Logistics",
			Status = "Active"
		});
		custResp.StatusCode.Should().Be(HttpStatusCode.Created);
		var customer = await custResp.Content.ReadFromJsonAsync<CustomerResponse>();

		// 2. Tạo Contract Draft thiếu tệp
		var contractNo = $"HD-E2E-{Guid.NewGuid():N}"[..18];
		var contractResp = await _client.PostAsJsonAsync("/api/contracts", new CreateContractRequest
		{
			ContractNo = contractNo,
			CustomerId = customer!.Id,
			StartDate = new DateOnly(2026, 1, 1),
			EndDate = new DateOnly(2026, 12, 31),
			ServiceGroup = null,
			AttachmentUrl = null
		});
		contractResp.StatusCode.Should().Be(HttpStatusCode.Created);
		var contract = await contractResp.Content.ReadFromJsonAsync<ContractResponse>();
		contract!.Status.Should().Be("Draft");

		// 3. Submit khi thiếu điều kiện -> Thất bại 400
		var submitFailResp = await _client.PostAsJsonAsync($"/api/contracts/{contract.Id}/submit", new SubmitContractRequest());
		submitFailResp.StatusCode.Should().Be(HttpStatusCode.BadRequest);

		// 4. Bổ sung dữ liệu còn thiếu
		var updateResp = await _client.PutAsJsonAsync($"/api/contracts/{contract.Id}", new UpdateContractRequest
		{
			CustomerId = customer.Id,
			StartDate = new DateOnly(2026, 1, 1),
			EndDate = new DateOnly(2026, 12, 31),
			ServiceGroup = "Dịch vụ cảng biển quốc tế",
			AttachmentUrl = "files/hd-e2e-final.pdf"
		});
		updateResp.StatusCode.Should().Be(HttpStatusCode.OK);

		// 5. Submit lại -> Thành công 200 OK chuyển sang PendingApproval
		var submitSuccessResp = await _client.PostAsJsonAsync($"/api/contracts/{contract.Id}/submit", new SubmitContractRequest
		{
			Comment = "Đã bổ sung đầy đủ hồ sơ."
		});
		submitSuccessResp.StatusCode.Should().Be(HttpStatusCode.OK);
		var finalResult = await submitSuccessResp.Content.ReadFromJsonAsync<SubmitContractResponse>();
		finalResult!.Status.Should().Be("PendingApproval");
		finalResult.ApprovalId.Should().NotBeNullOrEmpty();
	}
}

