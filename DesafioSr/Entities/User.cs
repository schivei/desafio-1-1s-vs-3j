using System.Text.Json.Serialization;

namespace DesafioSr.Entities;

public sealed class User
{
    [JsonPropertyName("id")]
    public Guid Id { get; set; }

    [JsonPropertyName("name")]
    public string Name { get; set; } = string.Empty;

    [JsonPropertyName("age")]
    public int Age { get; set; }

    [JsonPropertyName("score")]
    public int Score { get; set; }

    [JsonPropertyName("active")]
    public bool Active { get; set; } = true;

    [JsonPropertyName("coutry")]
    public string Country { get; set; } = string.Empty;

    [JsonPropertyName("team")]
    public Team? Team { get; set; }

    [JsonPropertyName("logs")]
    public Log[] Logs { get; set; } = [];
}
