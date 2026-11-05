using Microsoft.AspNetCore.Mvc;
using QTKD.Api.DTOs.Requests;
using QTKD.Api.DTOs.Responses;
using QTKD.Api.Services;

namespace QTKD.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
public class PriceListsController : ControllerBase
{
	private readonly IPriceListService _priceListService;

	public PriceListsController(IPriceListService priceListService)
	{
		_priceListService = priceListService;
	}

	[HttpGet]
	public async Task<ActionResult<List<PriceListResponse>>> GetAll()
	{
		var priceLists = await _priceListService.GetAllAsync();
		return Ok(priceLists);
	}

	[HttpGet("{id}")]
	public async Task<ActionResult<PriceListResponse>> GetById(int id)
	{
		var priceList = await _priceListService.GetByIdAsync(id);
		if (priceList == null)
		{
			return NotFound(new
			{
				code = "PRICELIST_NOT_FOUND",
				message = $"Không tìm thấy bảng giá với ID = {id}"
			});
		}

		return Ok(priceList);
	}

	[HttpPost]
	public async Task<ActionResult<PriceListResponse>> Create([FromBody] CreatePriceListRequest request)
	{
		var result = await _priceListService.CreatePriceListAsync(request);
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

	[HttpGet("{id}/items")]
	public async Task<ActionResult<List<PriceItemResponse>>> GetItems(int id)
	{
		var result = await _priceListService.GetItemsByPriceListIdAsync(id);
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

	[HttpPost("{id}/items")]
	public async Task<ActionResult<PriceItemResponse>> AddItem(int id, [FromBody] CreatePriceItemRequest request)
	{
		var result = await _priceListService.AddPriceItemAsync(id, request);
		if (!result.IsSuccess)
		{
			return StatusCode(result.StatusCode, new
			{
				code = result.ErrorCode,
				message = result.Message
			});
		}

		return StatusCode(201, result.Data);
	}
}

