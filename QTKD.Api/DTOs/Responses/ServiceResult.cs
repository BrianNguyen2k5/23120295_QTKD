namespace QTKD.Api.DTOs.Responses;

public class ServiceResult<T>
{
	public bool IsSuccess { get; set; }
	public int StatusCode { get; set; } = 200;
	public T? Data { get; set; }
	public string? ErrorCode { get; set; }
	public string? Message { get; set; }
	public List<string>? Errors { get; set; }

	public static ServiceResult<T> Success(T data, int statusCode = 200) => new()
	{
		IsSuccess = true,
		StatusCode = statusCode,
		Data = data
	};

	public static ServiceResult<T> Failure(int statusCode, string errorCode, string message, List<string>? errors = null) => new()
	{
		IsSuccess = false,
		StatusCode = statusCode,
		ErrorCode = errorCode,
		Message = message,
		Errors = errors
	};
}

