using QTKD.Api.Models;

namespace QTKD.Api.Repositories;

public interface ICustomerRepository
{
	Task<List<Customer>> GetAllAsync();
	Task<Customer?> GetByIdAsync(int id, bool trackChanges = false);
	Task<Customer> CreateAsync(Customer customer);
	Task UpdateAsync(Customer customer);
}
