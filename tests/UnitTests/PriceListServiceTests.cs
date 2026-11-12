using FluentAssertions;
using Moq;
using QTKD.Api.DTOs.Requests;
using QTKD.Api.Models;
using QTKD.Api.Repositories;
using QTKD.Api.Services;

namespace QTKD.Tests.UnitTests;

public class PriceListServiceTests
{
	private readonly Mock<IPriceListRepository> _priceListRepoMock;
	private readonly PriceListService _priceListService;

	public PriceListServiceTests()
	{
		_priceListRepoMock = new Mock<IPriceListRepository>();
		_priceListService = new PriceListService(_priceListRepoMock.Object);
	}

	[Fact]
	public async Task CreatePriceList_ValidData_ReturnsCreated()
	{
		// Arrange
		var request = new CreatePriceListRequest
		{
			Name = "Bảng giá 2026",
			EffectiveDate = new DateOnly(2026, 1, 1)
		};

		_priceListRepoMock.Setup(r => r.CreatePriceListAsync(It.IsAny<PriceList>()))
			.ReturnsAsync((PriceList pl) =>
			{
				pl.Id = 1;
				return pl;
			});

		// Act
		var result = await _priceListService.CreatePriceListAsync(request);

		// Assert
		result.IsSuccess.Should().BeTrue();
		result.StatusCode.Should().Be(201);
		result.Data!.Name.Should().Be("Bảng giá 2026");
	}

	[Theory]
	[InlineData("")]
	[InlineData("   ")]
	[InlineData(null)]
	public async Task CreatePriceList_MissingName_ReturnsBadRequest(string? emptyName)
	{
		// Arrange
		var request = new CreatePriceListRequest
		{
			Name = emptyName!,
			EffectiveDate = new DateOnly(2026, 1, 1)
		};

		// Act
		var result = await _priceListService.CreatePriceListAsync(request);

		// Assert
		result.IsSuccess.Should().BeFalse();
		result.StatusCode.Should().Be(400);
		result.ErrorCode.Should().Be("INVALID_PRICELIST_NAME");
	}

	[Fact]
	public async Task AddPriceItem_ValidData_ReturnsCreated()
	{
		// Arrange
		var priceList = new PriceList { Id = 1, Name = "Bảng giá cảng" };
		_priceListRepoMock.Setup(r => r.GetByIdAsync(1)).ReturnsAsync(priceList);
		_priceListRepoMock.Setup(r => r.ExistsServiceCodeInPriceListAsync(1, "STORAGE")).ReturnsAsync(false);
		_priceListRepoMock.Setup(r => r.AddPriceItemAsync(It.IsAny<PriceItem>()))
			.ReturnsAsync((PriceItem item) =>
			{
				item.Id = 10;
				return item;
			});

		var request = new CreatePriceItemRequest
		{
			ServiceCode = "STORAGE",
			UnitPrice = 150000m
		};

		// Act
		var result = await _priceListService.AddPriceItemAsync(1, request);

		// Assert
		result.IsSuccess.Should().BeTrue();
		result.StatusCode.Should().Be(201);
		result.Data!.UnitPrice.Should().Be(150000m);
	}

	[Theory]
	[InlineData(0)]
	[InlineData(-5000)]
	public async Task AddPriceItem_InvalidUnitPrice_ReturnsBadRequest(decimal invalidPrice)
	{
		// Arrange
		var priceList = new PriceList { Id = 1, Name = "Bảng giá cảng" };
		_priceListRepoMock.Setup(r => r.GetByIdAsync(1)).ReturnsAsync(priceList);

		var request = new CreatePriceItemRequest
		{
			ServiceCode = "STORAGE",
			UnitPrice = invalidPrice
		};

		// Act
		var result = await _priceListService.AddPriceItemAsync(1, request);

		// Assert
		result.IsSuccess.Should().BeFalse();
		result.StatusCode.Should().Be(400);
		result.ErrorCode.Should().Be("INVALID_UNIT_PRICE");
	}

	[Fact]
	public async Task AddPriceItem_DuplicateServiceCode_ReturnsConflict()
	{
		// Arrange
		var priceList = new PriceList { Id = 1, Name = "Bảng giá cảng" };
		_priceListRepoMock.Setup(r => r.GetByIdAsync(1)).ReturnsAsync(priceList);
		_priceListRepoMock.Setup(r => r.ExistsServiceCodeInPriceListAsync(1, "STORAGE")).ReturnsAsync(true);

		var request = new CreatePriceItemRequest
		{
			ServiceCode = "STORAGE",
			UnitPrice = 200000m
		};

		// Act
		var result = await _priceListService.AddPriceItemAsync(1, request);

		// Assert
		result.IsSuccess.Should().BeFalse();
		result.StatusCode.Should().Be(409);
		result.ErrorCode.Should().Be("DUPLICATE_SERVICE_CODE");
	}
}

