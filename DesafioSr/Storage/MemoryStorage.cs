using DesafioSr.Entities;

namespace DesafioSr.Storage;

public sealed class MemoryStorage
{
    private static readonly MemoryStorage _instance;

    static MemoryStorage()
    {
        _instance = new();
    }

    public static MemoryStorage Instance => _instance;

    private ReadOnlyMemory<User> _users;

    public IEnumerable<User> Users => _users.ToArray();

    public int Size => _users.Length;

    public async Task LoadFromFileAsync(IFormFile file)
    {
        if (file == null || file.Length == 0)
        {
            throw new ArgumentException("File is required.", nameof(file));
        }

        using var stream = file.OpenReadStream();
        using var reader = new StreamReader(stream);
        var content = await reader.ReadToEndAsync();

        _users = System.Text.Json.JsonSerializer.Deserialize<User[]>(content) ?? [];
    }
}
