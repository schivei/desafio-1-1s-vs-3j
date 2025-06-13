using System.Text.Json.Serialization;

namespace DesafioSr.ViewModels;

public readonly struct ResponseViewModel()
{
    [JsonPropertyName("data")]
    public object? Data { get; init; }

    [JsonPropertyName("elapsed")]
    public required TimeSpan Elapsed { get; init; }

    [JsonPropertyName("timestamp")]
    public DateTimeOffset Timestamp { get; init; } = DateTimeOffset.UtcNow;

    [JsonPropertyName("success")]
    public bool Success { get; init; } = true;

    [JsonPropertyName("message")]
    public string? Message { get; init; } = null;

    public static ResponseViewModel Create(object data, TimeSpan elapsed, string? message = null)
    {
        return new ResponseViewModel
        {
            Data = data,
            Elapsed = elapsed,
            Message = message
        };
    }

    public static ResponseViewModel CreateError(string message, TimeSpan elapsed)
    {
        return new ResponseViewModel
        {
            Success = false,
            Message = message,
            Elapsed = elapsed
        };
    }
}
