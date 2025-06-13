using System.Text.Json.Serialization;

namespace DesafioSr.Entities;

public sealed class Team
{
    [JsonPropertyName("name")]
    public string Name { get; set; } = string.Empty;
    [JsonPropertyName("leader")]
    public bool Leader { get; set; }
    [JsonPropertyName("projects")]
    public Project[] Projects { get; set; } = [];
}
