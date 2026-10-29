using QTKD.Api.DTOs.Responses;

namespace QTKD.Api.Services;

public interface IContractService
{
	Task<ContractResponse?> GetByIdAsync(int id);
}
