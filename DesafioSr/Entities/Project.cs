using System.Text.Json.Serialization;

namespace DesafioSr.Entities;

public sealed class Project
{
    [JsonPropertyName("name")]
    public string Name { get; set; } = string.Empty;
    [JsonPropertyName("completed")]
    public bool Completed { get; set; }
}
