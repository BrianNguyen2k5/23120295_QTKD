using QTKD.Api.DTOs.Requests;
using QTKD.Api.DTOs.Responses;
using QTKD.Api.Models;
using QTKD.Api.Repositories;

namespace QTKD.Api.Services;

public class PriceListService : IPriceListService
{
	private readonly IPriceListRepository _priceListRepository;

	public PriceListService(IPriceListRepository priceListRepository)
	{
		_priceListRepository = priceListRepository;
	}

	public async Task<List<PriceListResponse>> GetAllAsync()
	{
		var priceLists = await _priceListRepository.GetAllAsync();
		return priceLists.Select(MapToResponse).ToList();
	}

	public async Task<PriceListResponse?> GetByIdAsync(int id)
	{
		var priceList = await _priceListRepository.GetByIdAsync(id);
		return priceList == null ? null : MapToResponse(priceList);
	}

	public async Task<ServiceResult<PriceListResponse>> CreatePriceListAsync(CreatePriceListRequest request)
	{
		if (string.IsNullOrWhiteSpace(request.Name))
		{
			return ServiceResult<PriceListResponse>.Failure(
				400,
				"INVALID_PRICELIST_NAME",
				"Tên bảng giá là bắt buộc và không được để trống.");
		}

		if (request.EffectiveDate == default)
		{
			return ServiceResult<PriceListResponse>.Failure(
				400,
				"INVALID_EFFECTIVE_DATE",
				"Ngày hiệu lực của bảng giá không hợp lệ.");
		}

		var priceList = new PriceList
		{
			Name = request.Name.Trim(),
			EffectiveDate = request.EffectiveDate
		};

		var created = await _priceListRepository.CreatePriceListAsync(priceList);
		return ServiceResult<PriceListResponse>.Success(MapToResponse(created), 201);
	}

	public async Task<ServiceResult<List<PriceItemResponse>>> GetItemsByPriceListIdAsync(int priceListId)
	{
		var priceList = await _priceListRepository.GetByIdAsync(priceListId);
		if (priceList == null)
		{
			return ServiceResult<List<PriceItemResponse>>.Failure(
				404,
				"PRICELIST_NOT_FOUND",
				$"Không tìm thấy bảng giá với ID = {priceListId}");
		}

		var items = await _priceListRepository.GetItemsByPriceListIdAsync(priceListId);
		var response = items.Select(MapItemToResponse).ToList();
		return ServiceResult<List<PriceItemResponse>>.Success(response);
	}

	public async Task<ServiceResult<PriceItemResponse>> AddPriceItemAsync(int priceListId, CreatePriceItemRequest request)
	{
		// Kiểm tra liên kết PriceItem - PriceList đúng
		var priceList = await _priceListRepository.GetByIdAsync(priceListId);
		if (priceList == null)
		{
			return ServiceResult<PriceItemResponse>.Failure(
				404,
				"PRICELIST_NOT_FOUND",
				$"Không tìm thấy bảng giá với ID = {priceListId} để thêm đơn giá.");
		}

		if (string.IsNullOrWhiteSpace(request.ServiceCode))
		{
			return ServiceResult<PriceItemResponse>.Failure(
				400,
				"INVALID_SERVICE_CODE",
				"Mã dịch vụ (ServiceCode) không được để trống.");
		}

		// Kiểm tra đơn giá hợp lệ (> 0)
		if (request.UnitPrice <= 0)
		{
			return ServiceResult<PriceItemResponse>.Failure(
				400,
				"INVALID_UNIT_PRICE",
				"Đơn giá dịch vụ (UnitPrice) phải lớn hơn 0.");
		}

		var normalizedCode = request.ServiceCode.Trim().ToUpperInvariant();
		if (await _priceListRepository.ExistsServiceCodeInPriceListAsync(priceListId, normalizedCode))
		{
			return ServiceResult<PriceItemResponse>.Failure(
				409,
				"DUPLICATE_SERVICE_CODE",
				$"Mã dịch vụ '{normalizedCode}' đã tồn tại trong bảng giá ID = {priceListId}.");
		}

		var item = new PriceItem
		{
			PriceListId = priceListId,
			ServiceCode = normalizedCode,
			UnitPrice = request.UnitPrice
		};

		var created = await _priceListRepository.AddPriceItemAsync(item);
		return ServiceResult<PriceItemResponse>.Success(MapItemToResponse(created), 201);
	}

	private static PriceListResponse MapToResponse(PriceList pl) => new()
	{
		Id = pl.Id,
		Name = pl.Name,
		EffectiveDate = pl.EffectiveDate,
		Items = pl.PriceItems.Select(MapItemToResponse).ToList()
	};

	private static PriceItemResponse MapItemToResponse(PriceItem pi) => new()
	{
		Id = pi.Id,
		PriceListId = pi.PriceListId,
		ServiceCode = pi.ServiceCode,
		UnitPrice = pi.UnitPrice
	};
}

