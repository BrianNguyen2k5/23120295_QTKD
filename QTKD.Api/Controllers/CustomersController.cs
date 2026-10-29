using Microsoft.AspNetCore.Mvc;
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
}
