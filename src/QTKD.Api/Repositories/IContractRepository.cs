using QTKD.Api.Models;

namespace QTKD.Api.Repositories;

public interface IContractRepository
{
	Task<List<Contract>> GetAllAsync();
	Task<Contract?> GetByIdAsync(int id, bool trackChanges = false);
	Task<bool> ExistsByContractNoAsync(string contractNo, int? excludeId = null);
	Task<Contract> CreateAsync(Contract contract);
	Task UpdateAsync(Contract contract);
}
