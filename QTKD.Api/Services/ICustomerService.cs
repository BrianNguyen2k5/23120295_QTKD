using QTKD.Api.DTOs.Responses;

namespace QTKD.Api.Services;

public interface ICustomerService
{
	Task<List<CustomerResponse>> GetAllAsync();
}
