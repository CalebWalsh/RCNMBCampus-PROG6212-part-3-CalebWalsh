using System.Net.Http.Headers;
using System.Text.Json;

namespace RaceDay.Mvc.Services;

public class RaceDayApiClient
{
    private const string CookieKey = "ApiCookies";
    private const string Unavailable = "RaceDay could not reach the server. Please try again shortly.";
    private static readonly JsonSerializerOptions Json = new(JsonSerializerDefaults.Web);

    private readonly IHttpClientFactory _factory;
    private readonly IHttpContextAccessor _accessor;
    private readonly ILogger<RaceDayApiClient> _logger;

    public RaceDayApiClient(IHttpClientFactory factory, IHttpContextAccessor accessor, ILogger<RaceDayApiClient> logger)
    {
        _factory = factory;
        _accessor = accessor;
        _logger = logger;
    }

    private ISession Session => _accessor.HttpContext!.Session;

    public Task<ApiResult<T>> GetAsync<T>(string url) =>
        CallAsync<T>(HttpMethod.Get, url, null);

    public Task<ApiResult<T>> PostAsync<T>(string url, object body) =>
        CallAsync<T>(HttpMethod.Post, url, Body(body));

    public async Task<ApiResult> PostAsync(string url, object? body = null) =>
        await CallAsync<object>(HttpMethod.Post, url, body == null ? null : Body(body));

    public async Task<ApiResult> PutAsync(string url, object body) =>
        await CallAsync<object>(HttpMethod.Put, url, Body(body));

    public async Task<ApiResult> DeleteAsync(string url) =>
        await CallAsync<object>(HttpMethod.Delete, url, null);

    public Task<ApiResult<T>> UploadAsync<T>(string url, IFormFile file)
    {
        var form = new MultipartFormDataContent();
        var content = new StreamContent(file.OpenReadStream());
        string contentType = string.IsNullOrWhiteSpace(file.ContentType) ? "application/octet-stream" : file.ContentType;
        content.Headers.ContentType = new MediaTypeHeaderValue(contentType);
        form.Add(content, "file", file.FileName);
        return CallAsync<T>(HttpMethod.Post, url, form);
    }

    private static JsonContent Body(object body) =>
        JsonContent.Create(body, body.GetType(), null, Json);

    private async Task<ApiResult<T>> CallAsync<T>(HttpMethod method, string url, HttpContent? content)
    {
        try
        {
            using var request = new HttpRequestMessage(method, url) { Content = content };
            var cookies = LoadCookies();
            if (cookies.Count > 0)
                request.Headers.TryAddWithoutValidation("Cookie",
                    string.Join("; ", cookies.Select(c => $"{c.Key}={c.Value}")));

            var client = _factory.CreateClient("RaceDayApi");
            using var response = await client.SendAsync(request);
            StoreCookies(response);

            int status = (int)response.StatusCode;
            if (!response.IsSuccessStatusCode)
                return new ApiResult<T> { Success = false, StatusCode = status, Error = await ReadErrorAsync(response) };

            T? data = default;
            if (typeof(T) != typeof(object))
            {
                string body = await response.Content.ReadAsStringAsync();
                if (!string.IsNullOrWhiteSpace(body))
                    data = JsonSerializer.Deserialize<T>(body, Json);
            }
            return new ApiResult<T> { Success = true, StatusCode = status, Data = data };
        }
        catch (HttpRequestException ex)
        {
            _logger.LogError(ex, "API request failed: {Method} {Url}", method, url);
            return new ApiResult<T> { Success = false, StatusCode = 503, Error = Unavailable };
        }
        catch (TaskCanceledException ex)
        {
            _logger.LogError(ex, "API request timed out: {Method} {Url}", method, url);
            return new ApiResult<T> { Success = false, StatusCode = 504, Error = Unavailable };
        }
        catch (JsonException ex)
        {
            _logger.LogError(ex, "API returned unexpected data: {Method} {Url}", method, url);
            return new ApiResult<T> { Success = false, StatusCode = 502, Error = "RaceDay received an unexpected response. Please try again." };
        }
    }

    private Dictionary<string, string> LoadCookies()
    {
        string? raw = Session.GetString(CookieKey);
        if (string.IsNullOrEmpty(raw)) return new Dictionary<string, string>();
        return JsonSerializer.Deserialize<Dictionary<string, string>>(raw) ?? new Dictionary<string, string>();
    }

    private void StoreCookies(HttpResponseMessage response)
    {
        if (!response.Headers.TryGetValues("Set-Cookie", out var headers)) return;

        var cookies = LoadCookies();
        foreach (string header in headers)
        {
            string pair = header.Split(';', 2)[0];
            int split = pair.IndexOf('=');
            if (split <= 0) continue;

            string name = pair[..split].Trim();
            string value = pair[(split + 1)..].Trim();
            if (string.IsNullOrEmpty(value)) cookies.Remove(name);
            else cookies[name] = value;
        }
        Session.SetString(CookieKey, JsonSerializer.Serialize(cookies));
    }

    private static async Task<string> ReadErrorAsync(HttpResponseMessage response)
    {
        int status = (int)response.StatusCode;
        if (status == 401) return "Your session has expired. Please log in again.";
        if (status == 403) return "You do not have permission to do that.";
        if (status == 404) return "That item could not be found.";
        if (status >= 500) return "RaceDay could not complete the request. Please try again.";

        const string generic = "The request could not be completed. Please check your details and try again.";
        string body = await response.Content.ReadAsStringAsync();
        if (string.IsNullOrWhiteSpace(body)) return generic;

        try
        {
            using var doc = JsonDocument.Parse(body);
            var root = doc.RootElement;

            if (root.ValueKind == JsonValueKind.String)
                return Clean(root.GetString()) ?? generic;

            if (root.ValueKind == JsonValueKind.Object)
            {
                foreach (string key in new[] { "message", "detail", "error" })
                {
                    string? text = FindString(root, key);
                    if (Clean(text) is { } found) return found;
                }

                foreach (var prop in root.EnumerateObject())
                {
                    if (!prop.NameEquals("errors") && !prop.Name.Equals("errors", StringComparison.OrdinalIgnoreCase)) continue;
                    if (prop.Value.ValueKind != JsonValueKind.Object) continue;
                    foreach (var field in prop.Value.EnumerateObject())
                    {
                        if (field.Value.ValueKind == JsonValueKind.Array && field.Value.GetArrayLength() > 0)
                            if (Clean(field.Value[0].GetString()) is { } first) return first;
                    }
                }

                if (Clean(FindString(root, "title")) is { } title) return title;
            }
        }
        catch (JsonException)
        {
            if (body.Length <= 200 && !body.Contains('<')) return body.Trim();
        }

        return generic;
    }

    private static string? FindString(JsonElement obj, string name)
    {
        foreach (var prop in obj.EnumerateObject())
            if (prop.Name.Equals(name, StringComparison.OrdinalIgnoreCase) && prop.Value.ValueKind == JsonValueKind.String)
                return prop.Value.GetString();
        return null;
    }

    private static string? Clean(string? text)
    {
        if (string.IsNullOrWhiteSpace(text)) return null;
        text = text.Trim();
        return text.Length > 200 || text.Contains('<') ? null : text;
    }
}
