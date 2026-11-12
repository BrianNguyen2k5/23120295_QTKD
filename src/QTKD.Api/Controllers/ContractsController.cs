using Microsoft.AspNetCore.Mvc;
using QTKD.Api.DTOs.Requests;
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

	[HttpGet]
	public async Task<ActionResult<List<ContractResponse>>> GetAll()
	{
		var contracts = await _contractService.GetAllAsync();
		return Ok(contracts);
	}

	[HttpGet("{id}")]
	public async Task<ActionResult<ContractResponse>> GetById(int id)
	{
		var contract = await _contractService.GetByIdAsync(id);
		if (contract == null)
		{
			return NotFound(new
			{
				code = "CONTRACT_NOT_FOUND",
				message = $"Không tìm thấy hợp đồng với ID = {id}"
			});
		}

		return Ok(contract);
	}

	[HttpPost]
	public async Task<ActionResult<ContractResponse>> Create([FromBody] CreateContractRequest request)
	{
		var result = await _contractService.CreateAsync(request);
		if (!result.IsSuccess)
		{
			return StatusCode(result.StatusCode, new
			{
				code = result.ErrorCode,
				message = result.Message
			});
		}

		return CreatedAtAction(nameof(GetById), new { id = result.Data!.Id }, result.Data);
	}

	[HttpPut("{id}")]
	public async Task<ActionResult<ContractResponse>> Update(int id, [FromBody] UpdateContractRequest request)
	{
		var result = await _contractService.UpdateAsync(id, request);
		if (!result.IsSuccess)
		{
			return StatusCode(result.StatusCode, new
			{
				code = result.ErrorCode,
				message = result.Message
			});
		}

		return Ok(result.Data);
	}

	// US03: Gửi hợp đồng trình duyệt (Draft -> PendingApproval)
	[HttpPost("{id}/submit")]
	public async Task<ActionResult<SubmitContractResponse>> Submit(
		int id,
		[FromBody] SubmitContractRequest? request = null,
		[FromHeader(Name = "X-User-Role")] string? userRole = null,
		[FromHeader(Name = "Idempotency-Key")] string? idempotencyKey = null)
	{
		var result = await _contractService.SubmitAsync(id, request, userRole, idempotencyKey);
		if (!result.IsSuccess)
		{
			return StatusCode(result.StatusCode, new
			{
				code = result.ErrorCode,
				message = result.Message,
				errors = result.Errors
			});
		}

		return Ok(result.Data);
	}
}
