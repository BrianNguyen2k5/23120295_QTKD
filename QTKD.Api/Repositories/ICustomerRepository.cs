using QTKD.Api.Models;

namespace QTKD.Api.Repositories;

public interface ICustomerRepository
{
	Task<List<Customer>> GetAllAsync();
}
