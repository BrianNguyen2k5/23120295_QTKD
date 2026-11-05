namespace QTKD.Api.DTOs.Responses;

public class PriceListResponse
{
	public int Id { get; set; }
	public string Name { get; set; } = string.Empty;
	public DateOnly EffectiveDate { get; set; }
	public List<PriceItemResponse> Items { get; set; } = new();
}

public class PriceItemResponse
{
	public int Id { get; set; }
	public int PriceListId { get; set; }
	public string ServiceCode { get; set; } = string.Empty;
	public decimal UnitPrice { get; set; }
}

