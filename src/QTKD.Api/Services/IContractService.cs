using QTKD.Api.DTOs.Requests;
using QTKD.Api.DTOs.Responses;

namespace QTKD.Api.Services;

public interface IContractService
{
	Task<List<ContractResponse>> GetAllAsync();
	Task<ContractResponse?> GetByIdAsync(int id);
	Task<ServiceResult<ContractResponse>> CreateAsync(CreateContractRequest request);
	Task<ServiceResult<ContractResponse>> UpdateAsync(int id, UpdateContractRequest request);
	Task<ServiceResult<SubmitContractResponse>> SubmitAsync(
		int id,
		SubmitContractRequest? request,
		string? userRole,
		string? idempotencyKey);
}
