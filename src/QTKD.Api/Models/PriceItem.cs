using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text.Json.Serialization;

namespace QTKD.Api.Models;

[Table("PriceItems")]
public class PriceItem
{
	[Key]
	public int Id { get; set; }

	[Required]
	public int PriceListId { get; set; }

	// Khóa ngoại trỏ đến PriceList
	[ForeignKey(nameof(PriceListId))]
	[JsonIgnore]
	public PriceList? PriceList { get; set; }

	[Required]
	[MaxLength(50)]
	public string ServiceCode { get; set; } = string.Empty; // Mã dịch vụ (ví dụ: PORT_STORAGE, PORT_HANDLING)

	[Required]
	[Column(TypeName = "decimal(18,2)")]
	public decimal UnitPrice { get; set; }
}
