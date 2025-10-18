namespace SharedKernel.Models;

public class ApiResponse<T>
{
    public bool Success { get; set; }
    public T? Data { get; set; }
    public string? Message { get; set; }
    public Dictionary<string, string[]>? Errors { get; set; }

    public ApiResponse(T? data, bool success = true, string? message = null)
    {
        Success = success;
        Data = data;
        Message = message;
    }

    public static ApiResponse<T> SuccessResponse(T data, string? message = null)
    {
        return new ApiResponse<T>(data, true, message);
    }

    public static ApiResponse<T> ErrorResponse(string message, Dictionary<string, string[]>? errors = null)
    {
        return new ApiResponse<T>(default, false, message) { Errors = errors };
    }
}

