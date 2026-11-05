namespace QTKD.Api.DTOs.Requests;

public class CreatePriceListRequest
{
	public string Name { get; set; } = string.Empty;
	public DateOnly EffectiveDate { get; set; }
}

public class CreatePriceItemRequest
{
	public string ServiceCode { get; set; } = string.Empty;
	public decimal UnitPrice { get; set; }
}

