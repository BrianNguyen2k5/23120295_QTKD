using QTKD.Api.Models;

namespace QTKD.Api.Repositories;

public interface IContractRepository
{
	Task<Contract?> GetByIdAsync(int id);
}
