using System.Text.Json.Serialization;

namespace DesafioSr.Entities;

public sealed class Log
{
    [JsonPropertyName("date")]
    public DateOnly Date { get; set; } = DateOnly.FromDateTime(DateTime.UtcNow);
    [JsonPropertyName("action")]
    [JsonConverter(typeof(JsonStringEnumConverter))]
    public ActionLevel Action { get; set; }
}
