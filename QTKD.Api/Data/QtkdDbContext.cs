using Microsoft.EntityFrameworkCore;
using QTKD.Api.Models;

namespace QTKD.Api.Data;

public class QtkdDbContext : DbContext
{
	public QtkdDbContext(DbContextOptions<QtkdDbContext> options) : base(options)
	{
	}

	public DbSet<Customer> Customers => Set<Customer>();
	public DbSet<Contract> Contracts => Set<Contract>();
	public DbSet<PriceList> PriceLists => Set<PriceList>();
	public DbSet<PriceItem> PriceItems => Set<PriceItem>();

	protected override void OnModelCreating(ModelBuilder modelBuilder)
	{
		base.OnModelCreating(modelBuilder);

		// 1. Cấu hình quan hệ Contract - Customer (1 - N)
		modelBuilder.Entity<Contract>(entity =>
		{
			entity.HasOne(c => c.Customer)
				  .WithMany(cust => cust.Contracts)
				  .HasForeignKey(c => c.CustomerId)
				  .OnDelete(DeleteBehavior.Restrict);

			// ContractNo là duy nhất
			entity.HasIndex(c => c.ContractNo)
				  .IsUnique();
		});

		// 2. Cấu hình quan hệ PriceList - PriceItem (1 - N)
		modelBuilder.Entity<PriceItem>(entity =>
		{
			entity.HasOne(pi => pi.PriceList)
				  .WithMany(pl => pl.PriceItems)
				  .HasForeignKey(pi => pi.PriceListId)
				  .OnDelete(DeleteBehavior.Cascade);
		});

		// 3. Dữ liệu mẫu (Seed Data) theo yêu cầu Tuần 05 & Case Study US03
		// Ít nhất 2 Customer
		modelBuilder.Entity<Customer>().HasData(
			new Customer
			{
				Id = 1,
				Name = "Công ty TNHH MTV Minh Phát",
				Status = "Active"
			},
			new Customer
			{
				Id = 2,
				Name = "Công ty Cổ phần Vận tải Biển Đông",
				Status = "Active"
			}
		);

		// 1 Contract ở trạng thái Draft liên kết với Customer Minh Phát
		modelBuilder.Entity<Contract>().HasData(
			new Contract
			{
				Id = 1,
				ContractNo = "HD-2026-018",
				CustomerId = 1,
				StartDate = new DateOnly(2026, 10, 1),
				EndDate = new DateOnly(2027, 9, 30),
				Status = "Draft",
				ServiceGroup = "Dịch vụ cảng",
				CreatedAt = new DateTime(2026, 10, 1, 0, 0, 0, DateTimeKind.Utc)
			}
		);

		// 1 PriceList
		modelBuilder.Entity<PriceList>().HasData(
			new PriceList
			{
				Id = 1,
				Name = "Bảng giá Dịch vụ Cảng 2026",
				EffectiveDate = new DateOnly(2026, 1, 1)
			}
		);

		// 2 PriceItem thuộc PriceList trên
		modelBuilder.Entity<PriceItem>().HasData(
			new PriceItem
			{
				Id = 1,
				PriceListId = 1,
				ServiceCode = "PORT_STORAGE",
				UnitPrice = 150000m
			},
			new PriceItem
			{
				Id = 2,
				PriceListId = 1,
				ServiceCode = "PORT_HANDLING",
				UnitPrice = 350000m
			}
		);
	}
}
