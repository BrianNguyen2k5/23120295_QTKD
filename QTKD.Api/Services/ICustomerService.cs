using QTKD.Api.DTOs.Requests;
using QTKD.Api.DTOs.Responses;

namespace QTKD.Api.Services;

public interface ICustomerService
{
	Task<List<CustomerResponse>> GetAllAsync();
	Task<CustomerResponse?> GetByIdAsync(int id);
	Task<ServiceResult<CustomerResponse>> CreateAsync(CreateCustomerRequest request);
	Task<ServiceResult<CustomerResponse>> UpdateAsync(int id, UpdateCustomerRequest request);
}
