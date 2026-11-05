using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text.Json.Serialization;

namespace QTKD.Api.Models;

[Table("Customers")]
public class Customer
{
	[Key]
	public int Id { get; set; }

	[Required]
	[MaxLength(200)]
	public string Name { get; set; } = string.Empty;

	[Required]
	[MaxLength(50)]
	public string Status { get; set; } = "Active"; // Active, Inactive

	// 1 Khách hàng có nhiều Hợp đồng
	[JsonIgnore]
	public ICollection<Contract> Contracts { get; set; } = new List<Contract>();
}