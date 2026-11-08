using System.Net;
using System.Net.Http.Headers;
using System.Text.Json;

namespace RaceDay.Mvc.Services;

/// <summary>
/// The only way the MVC application reaches data: a thin wrapper over HttpClient for the Part 2 API.
/// The API authenticates with a server-side session cookie. This client captures that cookie at login,
/// keeps it in the MVC session, and replays it on every call, so a user is signed in on both sides.
/// All file uploads (banners, profile pictures) are also sent here - the MVC app never talks to Azure.
/// </summary>
public class ApiClient
{
    public const string ApiCookieSessionKey = "RaceDay_ApiCookie";
    private const string ApiCookieName = "RaceDay.Session";

    private static readonly JsonSerializerOptions JsonOptions = new(JsonSerializerDefaults.Web);

    private readonly HttpClient _http;
    private readonly IHttpContextAccessor _accessor;
    private readonly ILogger<ApiClient> _logger;

    public ApiClient(HttpClient http, IHttpContextAccessor accessor, ILogger<ApiClient> logger)
    {
        _http = http;
        _accessor = accessor;
        _logger = logger;
    }

    public Task<ApiResult<T>> GetAsync<T>(string url) => SendAsync<T>(HttpMethod.Get, url, null);

    public Task<ApiResult<T>> PostAsync<T>(string url, object? body = null) =>
        SendAsync<T>(HttpMethod.Post, url, ToJson(body));

    public Task<ApiResult<T>> PutAsync<T>(string url, object? body = null) =>
        SendAsync<T>(HttpMethod.Put, url, ToJson(body));

    public Task<ApiResult> PostAsync(string url, object? body = null) =>
        SendAsync(HttpMethod.Post, url, ToJson(body));

    public Task<ApiResult> PutAsync(string url, object? body = null) =>
        SendAsync(HttpMethod.Put, url, ToJson(body));

    public Task<ApiResult> DeleteAsync(string url) => SendAsync(HttpMethod.Delete, url, null);

    /// <summary>Streams an uploaded file to the API as multipart/form-data (field name "file").</summary>
    public async Task<ApiResult<T>> UploadAsync<T>(string url, IFormFile file)
    {
        var content = new MultipartFormDataContent();
        var stream = file.OpenReadStream();
        var fileContent = new StreamContent(stream);
        fileContent.Headers.ContentType = new MediaTypeHeaderValue(
            string.IsNullOrWhiteSpace(file.ContentType) ? "application/octet-stream" : file.ContentType);
        content.Add(fileContent, "file", Path.GetFileName(file.FileName));

        try
        {
            return await SendAsync<T>(HttpMethod.Post, url, content);
        }
        finally
        {
            content.Dispose();
            await stream.DisposeAsync();
        }
    }

    // ------------------------------------------------------------------

    private async Task<ApiResult<T>> SendAsync<T>(HttpMethod method, string url, HttpContent? content)
    {
        var raw = await SendRawAsync(method, url, content);
        if (!raw.Success)
        {
            return new ApiResult<T>
            {
                Success = false,
                StatusCode = raw.StatusCode,
                Error = raw.Error,
                FieldErrors = raw.FieldErrors
            };
        }

        try
        {
            var data = string.IsNullOrWhiteSpace(raw.Body)
                ? default
                : JsonSerializer.Deserialize<T>(raw.Body, JsonOptions);

            return new ApiResult<T> { Success = true, StatusCode = raw.StatusCode, Data = data };
        }
        catch (JsonException ex)
        {
            _logger.LogError(ex, "Could not read the API response for {Url}", url);
            return new ApiResult<T>
            {
                Success = false,
                StatusCode = 502,
                Error = "The RaceDay service returned an unexpected response. Please try again."
            };
        }
    }

    private async Task<ApiResult> SendAsync(HttpMethod method, string url, HttpContent? content)
    {
        var raw = await SendRawAsync(method, url, content);
        return new ApiResult
        {
            Success = raw.Success,
            StatusCode = raw.StatusCode,
            Error = raw.Error,
            FieldErrors = raw.FieldErrors
        };
    }

    private sealed record RawResponse(
        bool Success, int StatusCode, string Body, string Error, Dictionary<string, string[]> FieldErrors);

    private async Task<RawResponse> SendRawAsync(HttpMethod method, string url, HttpContent? content)
    {
        var session = _accessor.HttpContext?.Session;

        using var request = new HttpRequestMessage(method, url.TrimStart('/')) { Content = content };

        var cookie = session?.GetString(ApiCookieSessionKey);
        if (!string.IsNullOrEmpty(cookie))
        {
            request.Headers.TryAddWithoutValidation("Cookie", cookie);
        }

        try
        {
            using var response = await _http.SendAsync(request);
            CaptureApiCookie(response, session);

            var body = await response.Content.ReadAsStringAsync();
            var status = (int)response.StatusCode;

            if (response.IsSuccessStatusCode)
            {
                return new RawResponse(true, status, body, string.Empty, new());
            }

            var (message, fields) = ParseError(body);
            return new RawResponse(false, status, body, message ?? FriendlyMessage(response.StatusCode), fields);
        }
        catch (Exception ex) when (ex is HttpRequestException or TaskCanceledException or InvalidOperationException)
        {
            _logger.LogError(ex, "The RaceDay API could not be reached ({Method} {Url})", method, url);
            return new RawResponse(false, 503, string.Empty,
                "We couldn't reach the RaceDay service right now. Please try again in a moment.", new());
        }
    }

    private static StringContent? ToJson(object? body) =>
        body is null
            ? null
            : new StringContent(JsonSerializer.Serialize(body, JsonOptions), System.Text.Encoding.UTF8, "application/json");

    private static void CaptureApiCookie(HttpResponseMessage response, ISession? session)
    {
        if (session is null || !response.Headers.TryGetValues("Set-Cookie", out var values))
        {
            return;
        }

        foreach (var header in values)
        {
            if (header.StartsWith(ApiCookieName + "=", StringComparison.Ordinal))
            {
                var pair = header.Split(';', 2)[0];
                session.SetString(ApiCookieSessionKey, pair);
                return;
            }
        }
    }

    /// <summary>Understands the API's three error shapes: {message}, ProblemDetails {errors}, and a bare ModelState object.</summary>
    private static (string? Message, Dictionary<string, string[]> Fields) ParseError(string body)
    {
        var fields = new Dictionary<string, string[]>();
        if (string.IsNullOrWhiteSpace(body))
        {
            return (null, fields);
        }

        try
        {
            using var doc = JsonDocument.Parse(body);
            if (doc.RootElement.ValueKind != JsonValueKind.Object)
            {
                return (null, fields);
            }

            string? message = null;
            JsonElement errorsElement = default;
            var hasErrors = false;

            foreach (var prop in doc.RootElement.EnumerateObject())
            {
                if (prop.NameEquals("message") && prop.Value.ValueKind == JsonValueKind.String)
                {
                    message = prop.Value.GetString();
                }
                else if (prop.NameEquals("errors") && prop.Value.ValueKind == JsonValueKind.Object)
                {
                    errorsElement = prop.Value;
                    hasErrors = true;
                }
            }

            IEnumerable<JsonProperty> candidates = hasErrors
                ? errorsElement.EnumerateObject()
                : doc.RootElement.EnumerateObject().Where(p => p.Value.ValueKind == JsonValueKind.Array);

            foreach (var prop in candidates)
            {
                if (prop.Value.ValueKind != JsonValueKind.Array)
                {
                    continue;
                }

                var messages = prop.Value.EnumerateArray()
                    .Where(v => v.ValueKind == JsonValueKind.String)
                    .Select(v => v.GetString()!)
                    .ToArray();

                if (messages.Length > 0)
                {
                    fields[prop.Name] = messages;
                }
            }

            if (message is null && fields.Count > 0)
            {
                message = "Please correct the highlighted details and try again.";
            }

            return (message, fields);
        }
        catch (JsonException)
        {
            return (null, fields);
        }
    }

    private static string FriendlyMessage(HttpStatusCode status) => (int)status switch
    {
        400 => "Some of the details you entered were not valid. Please check them and try again.",
        401 => "Your session has expired. Please log in again.",
        403 => "You don't have permission to do that.",
        404 => "We couldn't find what you were looking for.",
        409 => "That conflicts with something that already exists.",
        >= 500 => "The RaceDay service ran into a problem. Please try again shortly.",
        _ => "Something went wrong. Please try again."
    };
}
