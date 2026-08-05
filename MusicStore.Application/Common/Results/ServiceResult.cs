using System.Collections.Generic;

namespace MusicStore.Application.Common.Results;

public class ServiceResult
{
    public bool Success { get; init; }

    public string Message { get; init; } = string.Empty;

    public List<string> Errors { get; init; } = new();

    public static ServiceResult Ok(string message = "")
    {
        return new ServiceResult
        {
            Success = true,
            Message = message
        };
    }

    public static ServiceResult Fail(string message)
    {
        return new ServiceResult
        {
            Success = false,
            Message = message,
            Errors = new List<string> { message }
        };
    }

    public static ServiceResult Fail(List<string> errors)
    {
        return new ServiceResult
        {
            Success = false,
            Message = "One or more errors occurred.",
            Errors = errors
        };
    }
}