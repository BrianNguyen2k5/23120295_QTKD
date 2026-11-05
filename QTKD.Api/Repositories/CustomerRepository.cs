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
		return await _context.Customers
			.AsNoTracking()
			.OrderBy(c => c.Id)
			.ToListAsync();
	}

	public async Task<Customer?> GetByIdAsync(int id, bool trackChanges = false)
	{
		var query = trackChanges ? _context.Customers : _context.Customers.AsNoTracking();
		return await query.FirstOrDefaultAsync(c => c.Id == id);
	}

	public async Task<Customer> CreateAsync(Customer customer)
	{
		_context.Customers.Add(customer);
		await _context.SaveChangesAsync();
		return customer;
	}

	public async Task UpdateAsync(Customer customer)
	{
		_context.Customers.Update(customer);
		await _context.SaveChangesAsync();
	}
}
