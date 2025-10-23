using SharedKernel.Models;

namespace SharedKernel.Models;

public static class ResultExtensions
{
    public static Result<T> ToResult<T>(this T value)
    {
        return Result.Success(value);
    }

    public static Result<T> ToResult<T>(this T value, string errorMessage)
    {
        return string.IsNullOrEmpty(errorMessage) 
            ? Result.Success(value) 
            : Result.Failure<T>(errorMessage);
    }

    public static Result<T> ToResult<T>(this Exception exception)
    {
        return Result.Failure<T>(exception.Message);
    }

    public static Result<T> ToResult<T>(this IEnumerable<string> errors)
    {
        return Result.Failure<T>(string.Join("; ", errors));
    }

    public static async Task<Result<T>> ToResultAsync<T>(this Task<T> task)
    {
        try
        {
            var result = await task;
            return Result.Success(result);
        }
        catch (Exception ex)
        {
            return Result.Failure<T>(ex.Message);
        }
    }

    public static Result<T> OnSuccess<T>(this Result<T> result, Action<T> action)
    {
        if (result.IsSuccess && result.Value != null)
        {
            action(result.Value);
        }
        return result;
    }

    public static Result<T> OnFailure<T>(this Result<T> result, Action<string> action)
    {
        if (result.IsFailure && result.Error != null)
        {
            action(result.Error);
        }
        return result;
    }

    public static Result<TNew> Map<T, TNew>(this Result<T> result, Func<T, TNew> func)
    {
        return result.IsSuccess 
            ? Result.Success(func(result.Value!)) 
            : Result.Failure<TNew>(result.Error ?? "Unknown error");
    }

    public static Result<TNew> Map<T, TNew>(this Result<T> result, Func<T, Result<TNew>> func)
    {
        return result.IsSuccess 
            ? func(result.Value!) 
            : Result.Failure<TNew>(result.Error ?? "Unknown error");
    }

    public static async Task<Result<TNew>> MapAsync<T, TNew>(this Result<T> result, Func<T, Task<TNew>> func)
    {
        if (result.IsFailure)
            return Result.Failure<TNew>(result.Error ?? "Unknown error");

        try
        {
            var newValue = await func(result.Value!);
            return Result.Success(newValue);
        }
        catch (Exception ex)
        {
            return Result.Failure<TNew>(ex.Message);
        }
    }
}
