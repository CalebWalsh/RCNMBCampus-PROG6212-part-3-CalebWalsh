namespace RaceDay.Mvc.Services;

public class ApiResult
{
    public bool Success { get; init; }
    public int StatusCode { get; init; }
    public string Error { get; init; } = string.Empty;
    public bool Unauthorized => StatusCode == 401;
}

public class ApiResult<T> : ApiResult
{
    public T? Data { get; init; }
}
