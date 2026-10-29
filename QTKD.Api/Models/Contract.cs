using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace QTKD.Api.Models;

[Table("Contracts")]
public class Contract
{
    [Key]
    public int Id { get; set; }

    [Required]
    [MaxLength(50)]
    public string ContractNo { get; set; } = string.Empty; // Ví dụ: HD-2026-018

    [Required]
    public int CustomerId { get; set; }

    // Navigation property: Khóa ngoại trỏ đến Customer
    [ForeignKey(nameof(CustomerId))]
    public Customer? Customer { get; set; }

    [Required]
    public DateOnly StartDate { get; set; }

    [Required]
    public DateOnly EndDate { get; set; }

    [Required]
    [MaxLength(50)]
    public string Status { get; set; } = "Draft"; // Draft, PendingApproval, RevisionRequested, Approved, Rejected, Effective

    // Thông tin bổ sung theo đặc tả US03
    [MaxLength(100)]
    public string? ServiceGroup { get; set; } // Ví dụ: Dịch vụ cảng

    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
}
