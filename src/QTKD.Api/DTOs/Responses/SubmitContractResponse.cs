namespace QTKD.Api.DTOs.Responses;

public class SubmitContractResponse
{
	public int ContractId { get; set; }
	public string ContractNo { get; set; } = string.Empty;
	public string Status { get; set; } = string.Empty;
	public string ApprovalId { get; set; } = string.Empty;
	public DateTime? SubmittedAt { get; set; }
	public string Message { get; set; } = string.Empty;
}

