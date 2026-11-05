namespace QTKD.Api.DTOs.Requests;

public class CreateContractRequest
{
	public string ContractNo { get; set; } = string.Empty;
	public int CustomerId { get; set; }
	public DateOnly StartDate { get; set; }
	public DateOnly EndDate { get; set; }
	public string? Status { get; set; } // Nếu client truyền Approved sẽ bị chặn bởi CTR-03
	public string? ServiceGroup { get; set; }
	public string? AttachmentUrl { get; set; }
}

public class UpdateContractRequest
{
	public int CustomerId { get; set; }
	public DateOnly StartDate { get; set; }
	public DateOnly EndDate { get; set; }
	public string? Status { get; set; } // Kiểm tra CTR-01 & CTR-03
	public string? ServiceGroup { get; set; }
	public string? AttachmentUrl { get; set; }
}

public class SubmitContractRequest
{
	public string? Comment { get; set; }
}

