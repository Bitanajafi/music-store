namespace MusicStore.Application.Common.Results;

public class ServiceResult<T> : ServiceResult
{
    public T? Data { get; init; }

    public static ServiceResult<T> Ok(T data, string message = "")
    {
        return new ServiceResult<T>
        {
            Success = true,
            Message = message,
            Data = data
        };
    }

    public static ServiceResult<T> Fail(string message)
    {
        return new ServiceResult<T>
        {
            Success = false,
            Message = message,
            Errors = new List<string> { message }
        };
    }

    public static ServiceResult<T> Fail(List<string> errors)
    {
        return new ServiceResult<T>
        {
            Success = false,
            Message = "One or more errors occurred.",
            Errors = errors
        };
    }
}