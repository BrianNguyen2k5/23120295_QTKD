using QTKD.Api.DTOs.Requests;
using QTKD.Api.DTOs.Responses;
using QTKD.Api.Models;
using QTKD.Api.Repositories;

namespace QTKD.Api.Services;

public class CustomerService : ICustomerService
{
	private static readonly HashSet<string> ValidStatuses = new(StringComparer.OrdinalIgnoreCase)
	{
		"Active",
		"Inactive"
	};

	private readonly ICustomerRepository _customerRepository;

	public CustomerService(ICustomerRepository customerRepository)
	{
		_customerRepository = customerRepository;
	}

	public async Task<List<CustomerResponse>> GetAllAsync()
	{
		var customers = await _customerRepository.GetAllAsync();
		return customers.Select(MapToResponse).ToList();
	}

	public async Task<CustomerResponse?> GetByIdAsync(int id)
	{
		var customer = await _customerRepository.GetByIdAsync(id);
		return customer == null ? null : MapToResponse(customer);
	}

	public async Task<ServiceResult<CustomerResponse>> CreateAsync(CreateCustomerRequest request)
	{
		if (string.IsNullOrWhiteSpace(request.Name))
		{
			return ServiceResult<CustomerResponse>.Failure(
				400,
				"INVALID_CUSTOMER_NAME",
				"Tên khách hàng là trường bắt buộc và không được để trống.");
		}

		var status = string.IsNullOrWhiteSpace(request.Status) ? "Active" : request.Status.Trim();
		if (!ValidStatuses.Contains(status))
		{
			return ServiceResult<CustomerResponse>.Failure(
				400,
				"INVALID_CUSTOMER_STATUS",
				"Trạng thái khách hàng chỉ nhận giá trị 'Active' hoặc 'Inactive'.");
		}

		var normalizedStatus = status.Equals("Active", StringComparison.OrdinalIgnoreCase) ? "Active" : "Inactive";

		var customer = new Customer
		{
			Name = request.Name.Trim(),
			Status = normalizedStatus
		};

		var created = await _customerRepository.CreateAsync(customer);
		return ServiceResult<CustomerResponse>.Success(MapToResponse(created), 201);
	}

	public async Task<ServiceResult<CustomerResponse>> UpdateAsync(int id, UpdateCustomerRequest request)
	{
		var customer = await _customerRepository.GetByIdAsync(id, trackChanges: true);
		if (customer == null)
		{
			return ServiceResult<CustomerResponse>.Failure(
				404,
				"CUSTOMER_NOT_FOUND",
				$"Không tìm thấy khách hàng với ID = {id}");
		}

		if (string.IsNullOrWhiteSpace(request.Name))
		{
			return ServiceResult<CustomerResponse>.Failure(
				400,
				"INVALID_CUSTOMER_NAME",
				"Tên khách hàng là trường bắt buộc và không được để trống.");
		}

		if (string.IsNullOrWhiteSpace(request.Status) || !ValidStatuses.Contains(request.Status.Trim()))
		{
			return ServiceResult<CustomerResponse>.Failure(
				400,
				"INVALID_CUSTOMER_STATUS",
				"Trạng thái khách hàng chỉ nhận giá trị 'Active' hoặc 'Inactive'.");
		}

		customer.Name = request.Name.Trim();
		customer.Status = request.Status.Trim().Equals("Active", StringComparison.OrdinalIgnoreCase)
			? "Active"
			: "Inactive";

		await _customerRepository.UpdateAsync(customer);
		return ServiceResult<CustomerResponse>.Success(MapToResponse(customer));
	}

	private static CustomerResponse MapToResponse(Customer c) => new()
	{
		Id = c.Id,
		Name = c.Name,
		Status = c.Status
	};
}
