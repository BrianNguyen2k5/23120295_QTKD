using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace QTKD.Api.Models;

[Table("PriceLists")]
public class PriceList
{
	[Key]
	public int Id { get; set; }

	[Required]
	[MaxLength(200)]
	public string Name { get; set; } = string.Empty; // Ví dụ: Bảng giá dịch vụ cảng 2026

	[Required]
	public DateOnly EffectiveDate { get; set; }

	// 1 Bảng giá có nhiều Đơn giá dịch vụ
	public ICollection<PriceItem> PriceItems { get; set; } = new List<PriceItem>();
}
