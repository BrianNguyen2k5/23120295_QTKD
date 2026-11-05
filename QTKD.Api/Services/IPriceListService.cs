using QTKD.Api.DTOs.Requests;
using QTKD.Api.DTOs.Responses;

namespace QTKD.Api.Services;

public interface IPriceListService
{
	Task<List<PriceListResponse>> GetAllAsync();
	Task<PriceListResponse?> GetByIdAsync(int id);
	Task<ServiceResult<PriceListResponse>> CreatePriceListAsync(CreatePriceListRequest request);
	Task<ServiceResult<List<PriceItemResponse>>> GetItemsByPriceListIdAsync(int priceListId);
	Task<ServiceResult<PriceItemResponse>> AddPriceItemAsync(int priceListId, CreatePriceItemRequest request);
}

