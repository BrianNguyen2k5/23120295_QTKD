using Microsoft.EntityFrameworkCore;
using QTKD.Api.Data;
using QTKD.Api.Models;

namespace QTKD.Api.Repositories;

public class PriceListRepository : IPriceListRepository
{
	private readonly QtkdDbContext _context;

	public PriceListRepository(QtkdDbContext context)
	{
		_context = context;
	}

	public async Task<List<PriceList>> GetAllAsync()
	{
		return await _context.PriceLists
			.Include(pl => pl.PriceItems)
			.AsNoTracking()
			.OrderBy(pl => pl.Id)
			.ToListAsync();
	}

	public async Task<PriceList?> GetByIdAsync(int id)
	{
		return await _context.PriceLists
			.Include(pl => pl.PriceItems)
			.AsNoTracking()
			.FirstOrDefaultAsync(pl => pl.Id == id);
	}

	public async Task<PriceList> CreatePriceListAsync(PriceList priceList)
	{
		_context.PriceLists.Add(priceList);
		await _context.SaveChangesAsync();
		return priceList;
	}

	public async Task<List<PriceItem>> GetItemsByPriceListIdAsync(int priceListId)
	{
		return await _context.PriceItems
			.Where(pi => pi.PriceListId == priceListId)
			.AsNoTracking()
			.OrderBy(pi => pi.Id)
			.ToListAsync();
	}

	public async Task<bool> ExistsServiceCodeInPriceListAsync(int priceListId, string serviceCode)
	{
		return await _context.PriceItems.AnyAsync(pi =>
			pi.PriceListId == priceListId &&
			pi.ServiceCode.ToLower() == serviceCode.ToLower());
	}

	public async Task<PriceItem> AddPriceItemAsync(PriceItem priceItem)
	{
		_context.PriceItems.Add(priceItem);
		await _context.SaveChangesAsync();
		return priceItem;
	}
}

