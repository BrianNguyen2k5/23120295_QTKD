using FluentAssertions;
using Moq;
using QTKD.Api.DTOs.Requests;
using QTKD.Api.Models;
using QTKD.Api.Repositories;
using QTKD.Api.Services;

namespace QTKD.Tests.UnitTests;

public class CustomerServiceTests
{
	private readonly Mock<ICustomerRepository> _customerRepoMock;
	private readonly CustomerService _customerService;

	public CustomerServiceTests()
	{
		_customerRepoMock = new Mock<ICustomerRepository>();
		_customerService = new CustomerService(_customerRepoMock.Object);
	}

	/// <summary>
	/// TC01: Tạo Customer hợp lệ -> Tạo được và đọc lại thông tin
	/// </summary>
	[Fact]
	public async Task TC01_CreateCustomer_WithValidData_ReturnsSuccess()
	{
		// Arrange
		var request = new CreateCustomerRequest
		{
			Name = "Công ty TNHH Minh Phát",
			Status = "Active"
		};

		_customerRepoMock.Setup(r => r.CreateAsync(It.IsAny<Customer>()))
			.ReturnsAsync((Customer c) =>
			{
				c.Id = 10;
				return c;
			});

		// Act
		var result = await _customerService.CreateAsync(request);

		// Assert
		result.IsSuccess.Should().BeTrue();
		result.StatusCode.Should().Be(201);
		result.Data.Should().NotBeNull();
		result.Data!.Id.Should().Be(10);
		result.Data.Name.Should().Be("Công ty TNHH Minh Phát");
		result.Data.Status.Should().Be("Active");
		_customerRepoMock.Verify(r => r.CreateAsync(It.Is<Customer>(c => c.Name == request.Name && c.Status == "Active")), Times.Once);
	}

	/// <summary>
	/// TC02: Tạo Customer thiếu thông tin bắt buộc (Name rỗng) -> Bị từ chối (400 Bad Request)
	/// </summary>
	[Theory]
	[InlineData("")]
	[InlineData("   ")]
	[InlineData(null)]
	public async Task TC02_CreateCustomer_MissingRequiredName_ReturnsBadRequest(string? emptyName)
	{
		// Arrange
		var request = new CreateCustomerRequest
		{
			Name = emptyName!,
			Status = "Active"
		};

		// Act
		var result = await _customerService.CreateAsync(request);

		// Assert
		result.IsSuccess.Should().BeFalse();
		result.StatusCode.Should().Be(400);
		result.ErrorCode.Should().Be("INVALID_CUSTOMER_NAME");
		result.Message.Should().Contain("Tên khách hàng là trường bắt buộc");
		_customerRepoMock.Verify(r => r.CreateAsync(It.IsAny<Customer>()), Times.Never);
	}

	[Fact]
	public async Task CreateCustomer_InvalidStatus_ReturnsBadRequest()
	{
		// Arrange
		var request = new CreateCustomerRequest
		{
			Name = "Công ty ABC",
			Status = "UnknownStatus"
		};

		// Act
		var result = await _customerService.CreateAsync(request);

		// Assert
		result.IsSuccess.Should().BeFalse();
		result.StatusCode.Should().Be(400);
		result.ErrorCode.Should().Be("INVALID_CUSTOMER_STATUS");
	}

	[Fact]
	public async Task UpdateCustomer_ValidData_ReturnsSuccess()
	{
		// Arrange
		var existingCustomer = new Customer { Id = 1, Name = "Cũ", Status = "Active" };
		_customerRepoMock.Setup(r => r.GetByIdAsync(1, true)).ReturnsAsync(existingCustomer);

		var request = new UpdateCustomerRequest
		{
			Name = "Mới",
			Status = "Inactive"
		};

		// Act
		var result = await _customerService.UpdateAsync(1, request);

		// Assert
		result.IsSuccess.Should().BeTrue();
		result.Data!.Name.Should().Be("Mới");
		result.Data.Status.Should().Be("Inactive");
		_customerRepoMock.Verify(r => r.UpdateAsync(existingCustomer), Times.Once);
	}

	[Fact]
	public async Task UpdateCustomer_NonExisting_ReturnsNotFound()
	{
		// Arrange
		_customerRepoMock.Setup(r => r.GetByIdAsync(999, true)).ReturnsAsync((Customer?)null);

		var request = new UpdateCustomerRequest
		{
			Name = "Tên",
			Status = "Active"
		};

		// Act
		var result = await _customerService.UpdateAsync(999, request);

		// Assert
		result.IsSuccess.Should().BeFalse();
		result.StatusCode.Should().Be(404);
		result.ErrorCode.Should().Be("CUSTOMER_NOT_FOUND");
	}
}

