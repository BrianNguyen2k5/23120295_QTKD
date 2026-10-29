using Microsoft.EntityFrameworkCore;
using QTKD.Api.Data;
using QTKD.Api.Models;

namespace QTKD.Api.Repositories;

public class CustomerRepository : ICustomerRepository
{
	private readonly QtkdDbContext _context;

	public CustomerRepository(QtkdDbContext context)
	{
		_context = context;
	}

	public async Task<List<Customer>> GetAllAsync()
	{
		return await _context.Customers.AsNoTracking().ToListAsync();
	}
}
