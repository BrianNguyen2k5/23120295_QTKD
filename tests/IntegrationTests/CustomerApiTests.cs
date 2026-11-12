using System.Net;
using System.Net.Http.Json;
using FluentAssertions;
using Microsoft.Extensions.DependencyInjection;
using QTKD.Api.Data;
using QTKD.Api.DTOs.Requests;
using QTKD.Api.DTOs.Responses;

namespace QTKD.Tests.IntegrationTests;

public class CustomerApiTests : IClassFixture<CustomWebApplicationFactory>
{
	private readonly CustomWebApplicationFactory _factory;
	private readonly HttpClient _client;

	public CustomerApiTests(CustomWebApplicationFactory factory)
	{
		_factory = factory;
		_client = factory.CreateClient();
	}

	// TC01: Tạo Customer hợp lệ -> Tạo được và đọc lại từ DB
	[Fact]
	public async Task TC01_CreateCustomer_WithValidData_ReturnsCreated_AndPersistsToDatabase()
	{
		// Arrange
		var request = new CreateCustomerRequest
		{
			Name = "Công ty Cổ phần Vận tải Á Châu",
			Status = "Active"
		};

		// Act: 1. Gọi API tạo mới
		var response = await _client.PostAsJsonAsync("/api/customers", request);

		// Assert: Kiểm tra HTTP status và nội dung trả về
		response.StatusCode.Should().Be(HttpStatusCode.Created);
		var createdCustomer = await response.Content.ReadFromJsonAsync<CustomerResponse>();
		createdCustomer.Should().NotBeNull();
		createdCustomer!.Id.Should().BeGreaterThan(0);
		createdCustomer.Name.Should().Be("Công ty Cổ phần Vận tải Á Châu");
		createdCustomer.Status.Should().Be("Active");

		// Act 2: Đọc lại từ API GET /api/customers/{id}
		var getResponse = await _client.GetAsync($"/api/customers/{createdCustomer.Id}");
		getResponse.StatusCode.Should().Be(HttpStatusCode.OK);
		var fetchedCustomer = await getResponse.Content.ReadFromJsonAsync<CustomerResponse>();
		fetchedCustomer.Should().NotBeNull();
		fetchedCustomer!.Name.Should().Be("Công ty Cổ phần Vận tải Á Châu");

		// Act 3: Đọc trực tiếp từ Database kiểm thử để đảm bảo dữ liệu đã lưu thực sự
		using var scope = _factory.Services.CreateScope();
		var db = scope.ServiceProvider.GetRequiredService<QtkdDbContext>();
		var dbCustomer = await db.Customers.FindAsync(createdCustomer.Id);
		dbCustomer.Should().NotBeNull();
		dbCustomer!.Name.Should().Be("Công ty Cổ phần Vận tải Á Châu");
		dbCustomer.Status.Should().Be("Active");
	}

	// TC02: Tạo Customer thiếu thông tin bắt buộc -> Bị từ chối (400 Bad Request), có thông báo lỗi
	[Theory]
	[InlineData("")]
	[InlineData("   ")]
	public async Task TC02_CreateCustomer_MissingName_ReturnsBadRequest_WithErrorDetail(string invalidName)
	{
		// Arrange
		var request = new CreateCustomerRequest
		{
			Name = invalidName,
			Status = "Active"
		};

		// Act
		var response = await _client.PostAsJsonAsync("/api/customers", request);

		// Assert: HTTP Status và nội dung lỗi rõ ràng
		response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
		var content = await response.Content.ReadAsStringAsync();
		content.Should().Contain("INVALID_CUSTOMER_NAME");
		content.Should().Contain("Tên khách hàng là trường bắt buộc");
	}

	[Fact]
	public async Task GetAllCustomers_ReturnsInitialSeedCustomers()
	{
		// Act
		var response = await _client.GetAsync("/api/customers");

		// Assert
		response.StatusCode.Should().Be(HttpStatusCode.OK);
		var list = await response.Content.ReadFromJsonAsync<List<CustomerResponse>>();
		list.Should().NotBeNull();
		list!.Count.Should().BeGreaterThanOrEqualTo(2); // Có ít nhất 2 customer nạp từ seed data
	}
}

