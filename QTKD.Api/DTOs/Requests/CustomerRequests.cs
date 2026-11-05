namespace QTKD.Api.DTOs.Requests;

public class CreateCustomerRequest
{
	public string Name { get; set; } = string.Empty;
	public string Status { get; set; } = "Active";
}

public class UpdateCustomerRequest
{
	public string Name { get; set; } = string.Empty;
	public string Status { get; set; } = "Active";
}

