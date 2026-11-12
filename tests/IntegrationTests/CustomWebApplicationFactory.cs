using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using QTKD.Api.Data;

namespace QTKD.Tests.IntegrationTests;

public class CustomWebApplicationFactory : WebApplicationFactory<Program>
{
	private readonly string _dbName = $"QTKD_IntegrationTestDb_{Guid.NewGuid()}";

	protected override void ConfigureWebHost(IWebHostBuilder builder)
	{
		builder.UseEnvironment("Testing");

		builder.ConfigureServices(services =>
		{
			// Đăng ký QtkdDbContext sử dụng InMemory Database riêng cho Test
			services.AddDbContext<QtkdDbContext>(options =>
			{
				options.UseInMemoryDatabase(_dbName);
			});

			// Khởi tạo database và nạp dữ liệu seed
			var sp = services.BuildServiceProvider();
			using var scope = sp.CreateScope();
			var db = scope.ServiceProvider.GetRequiredService<QtkdDbContext>();
			db.Database.EnsureCreated();
		});
	}
}
