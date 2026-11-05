using QTKD.Api.Models;

namespace QTKD.Api.Repositories;

public interface IPriceListRepository
{
	Task<List<PriceList>> GetAllAsync();
	Task<PriceList?> GetByIdAsync(int id);
	Task<PriceList> CreatePriceListAsync(PriceList priceList);
	Task<List<PriceItem>> GetItemsByPriceListIdAsync(int priceListId);
	Task<bool> ExistsServiceCodeInPriceListAsync(int priceListId, string serviceCode);
	Task<PriceItem> AddPriceItemAsync(PriceItem priceItem);
}

