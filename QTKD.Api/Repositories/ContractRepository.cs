using Microsoft.EntityFrameworkCore;
using QTKD.Api.Data;
using QTKD.Api.Models;

namespace QTKD.Api.Repositories;

public class ContractRepository : IContractRepository
{
	private readonly QtkdDbContext _context;

	public ContractRepository(QtkdDbContext context)
	{
		_context = context;
	}

	public async Task<List<Contract>> GetAllAsync()
	{
		return await _context.Contracts
			.Include(c => c.Customer)
			.AsNoTracking()
			.OrderBy(c => c.Id)
			.ToListAsync();
	}

	public async Task<Contract?> GetByIdAsync(int id, bool trackChanges = false)
	{
		var query = _context.Contracts.Include(c => c.Customer);
		return trackChanges
			? await query.FirstOrDefaultAsync(c => c.Id == id)
			: await query.AsNoTracking().FirstOrDefaultAsync(c => c.Id == id);
	}

	public async Task<bool> ExistsByContractNoAsync(string contractNo, int? excludeId = null)
	{
		return await _context.Contracts.AnyAsync(c =>
			c.ContractNo.ToLower() == contractNo.ToLower() &&
			(!excludeId.HasValue || c.Id != excludeId.Value));
	}

	public async Task<Contract> CreateAsync(Contract contract)
	{
		_context.Contracts.Add(contract);
		await _context.SaveChangesAsync();
		await _context.Entry(contract).Reference(c => c.Customer).LoadAsync();
		return contract;
	}

	public async Task UpdateAsync(Contract contract)
	{
		_context.Contracts.Update(contract);
		await _context.SaveChangesAsync();
	}
}
