using QTKD.Api.DTOs.Responses;
using QTKD.Api.Models;
using QTKD.Api.Repositories;

namespace QTKD.Api.Services;

public class CustomerService : ICustomerService
{
	private readonly ICustomerRepository _customerRepository;

	public CustomerService(ICustomerRepository customerRepository)
	{
		_customerRepository = customerRepository;
	}

	public async Task<List<CustomerResponse>> GetAllAsync()
	{
		var customers = await _customerRepository.GetAllAsync();
		return customers.Select(c => new CustomerResponse
		{
			Id = c.Id,
			Name = c.Name,
			Status = c.Status
		}).ToList();
	}
}
