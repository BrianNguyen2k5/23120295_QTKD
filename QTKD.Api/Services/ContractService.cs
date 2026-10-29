using QTKD.Api.DTOs.Responses;
using QTKD.Api.Models;
using QTKD.Api.Repositories;

namespace QTKD.Api.Services;

public class ContractService : IContractService
{
	private readonly IContractRepository _contractRepository;

	public ContractService(IContractRepository contractRepository)
	{
		_contractRepository = contractRepository;
	}

	public async Task<ContractResponse?> GetByIdAsync(int id)
	{
		var contract = await _contractRepository.GetByIdAsync(id);
		if (contract == null)
		{
			return null;
		}

		return MapToResponse(contract);
	}

	private static ContractResponse MapToResponse(Contract contract)
	{
		return new ContractResponse
		{
			Id = contract.Id,
			ContractNo = contract.ContractNo,
			CustomerId = contract.CustomerId,
			CustomerName = contract.Customer?.Name,
			StartDate = contract.StartDate,
			EndDate = contract.EndDate,
			Status = contract.Status,
			ServiceGroup = contract.ServiceGroup,
			CreatedAt = contract.CreatedAt
		};
	}
}
