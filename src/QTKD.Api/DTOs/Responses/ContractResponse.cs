namespace QTKD.Api.DTOs.Responses;

public class ContractResponse
{
	public int Id { get; set; }
	public string ContractNo { get; set; } = string.Empty;
	public int CustomerId { get; set; }
	public string? CustomerName { get; set; }
	public DateOnly StartDate { get; set; }
	public DateOnly EndDate { get; set; }
	public string Status { get; set; } = string.Empty;
	public string? ServiceGroup { get; set; }
	public string? AttachmentUrl { get; set; }
	public string? ApprovalId { get; set; }
	public DateTime? SubmittedAt { get; set; }
	public string? SubmitComment { get; set; }
	public DateTime CreatedAt { get; set; }
}
