using Microsoft.AspNetCore.Mvc;
using QTKD.Api.DTOs.Requests;
using QTKD.Api.DTOs.Responses;
using QTKD.Api.Services;

namespace QTKD.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
public class CustomersController : ControllerBase
{
	private readonly ICustomerService _customerService;

	public CustomersController(ICustomerService customerService)
	{
		_customerService = customerService;
	}

	[HttpGet]
	public async Task<ActionResult<List<CustomerResponse>>> GetAll()
	{
		var customers = await _customerService.GetAllAsync();
		return Ok(customers);
	}

	[HttpGet("{id}")]
	public async Task<ActionResult<CustomerResponse>> GetById(int id)
	{
		var customer = await _customerService.GetByIdAsync(id);
		if (customer == null)
		{
			return NotFound(new
			{
				code = "CUSTOMER_NOT_FOUND",
				message = $"Không tìm thấy khách hàng với ID = {id}"
			});
		}

		return Ok(customer);
	}

	[HttpPost]
	public async Task<ActionResult<CustomerResponse>> Create([FromBody] CreateCustomerRequest request)
	{
		var result = await _customerService.CreateAsync(request);
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
	public async Task<ActionResult<CustomerResponse>> Update(int id, [FromBody] UpdateCustomerRequest request)
	{
		var result = await _customerService.UpdateAsync(id, request);
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
}
