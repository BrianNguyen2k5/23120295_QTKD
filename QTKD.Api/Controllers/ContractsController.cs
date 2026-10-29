using Microsoft.AspNetCore.Mvc;
using QTKD.Api.DTOs.Responses;
using QTKD.Api.Services;

namespace QTKD.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
public class ContractsController : ControllerBase
{
	private readonly IContractService _contractService;

	public ContractsController(IContractService contractService)
	{
		_contractService = contractService;
	}

	[HttpGet("{id}")]
	public async Task<ActionResult<ContractResponse>> GetById(int id)
	{
		var contract = await _contractService.GetByIdAsync(id);
		if (contract == null)
		{
			return NotFound(new { message = $"Không tìm thấy hợp đồng với ID = {id}" });
		}

		return Ok(contract);
	}
}
