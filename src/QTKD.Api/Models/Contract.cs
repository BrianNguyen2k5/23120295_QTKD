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

    // Thông tin bổ sung theo đặc tả US03 & CTR-02
    [MaxLength(100)]
    public string? ServiceGroup { get; set; } // Nội dung bắt buộc (Ví dụ: Dịch vụ cảng)

    [MaxLength(500)]
    public string? AttachmentUrl { get; set; } // Tệp đính kèm bắt buộc theo CTR-02 (Ví dụ: files/HD-2026-018.pdf)

    // Thông tin mô phỏng gửi trình duyệt (US03)
    [MaxLength(50)]
    public string? ApprovalId { get; set; } // Mã phiên phê duyệt sinh ra khi submit thành công

    public DateTime? SubmittedAt { get; set; }

    [MaxLength(500)]
    public string? SubmitComment { get; set; }

    [MaxLength(100)]
    public string? IdempotencyKey { get; set; } // Khóa chống gửi lặp (AC05)

    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
}
