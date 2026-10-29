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

	public async Task<Contract?> GetByIdAsync(int id)
	{
		return await _context.Contracts
			.Include(c => c.Customer)
			.AsNoTracking()
			.FirstOrDefaultAsync(c => c.Id == id);
	}
}
