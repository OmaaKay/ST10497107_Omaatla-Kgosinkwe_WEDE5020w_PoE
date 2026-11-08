namespace RaceDay.Mvc.Services;

/// <summary>The outcome of one call to the RaceDay API, already translated into friendly terms.</summary>
public class ApiResult
{
    public bool Success { get; init; }
    public int StatusCode { get; init; }

    /// <summary>A user-friendly error message (never a raw exception or stack trace).</summary>
    public string Error { get; init; } = string.Empty;

    /// <summary>Per-field validation messages returned by the API (property name → messages).</summary>
    public Dictionary<string, string[]> FieldErrors { get; init; } = new();

    public bool IsUnauthorized => StatusCode == 401;
    public bool IsForbidden => StatusCode == 403;
    public bool IsNotFound => StatusCode == 404;
}

public class ApiResult<T> : ApiResult
{
    public T? Data { get; init; }
}
