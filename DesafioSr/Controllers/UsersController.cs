using DesafioSr.Entities;
using DesafioSr.Storage;
using DesafioSr.ViewModels;
using Microsoft.AspNetCore.Mvc;
using System.Diagnostics;
using System.Text.Json;
using System.Threading.Tasks;

namespace DesafioSr.Controllers;

public class UsersController(MemoryStorage memoryStorage) : Controller
{
    private readonly Stopwatch _stopwatch = new();
    private readonly MemoryStorage _memoryStorage = memoryStorage ?? throw new ArgumentNullException(nameof(memoryStorage));

    [HttpPost("/users")]
    [DisableRequestSizeLimit, RequestFormLimits(MultipartBodyLengthLimit = int.MaxValue, ValueLengthLimit = int.MaxValue)]
    public async Task<IActionResult> PostUsersAsync([FromForm] IFormFile file)
    {
        _stopwatch.Start();

        try
        {
            if (file.Length == 0)
                return BadRequest(ResponseViewModel.CreateError("File is empty.", _stopwatch.Elapsed));

            if (!file.FileName.EndsWith(".json", StringComparison.OrdinalIgnoreCase))
                return BadRequest(ResponseViewModel.CreateError("Invalid file format. Only JSON files are accepted.", _stopwatch.Elapsed));

            await _memoryStorage.LoadFromFileAsync(file);

            var totalUsers = _memoryStorage.Size;

            _stopwatch.Stop();
            return Ok(ResponseViewModel.Create(new { total_users = totalUsers }, _stopwatch.Elapsed, "File processed successfully."));
        }
        catch (Exception ex)
        {
            _stopwatch.Stop();
            return BadRequest(ResponseViewModel.CreateError(ex.Message, _stopwatch.Elapsed));
        }
    }

    [HttpGet("/superusers")]
    public IActionResult GetSuperUsers([FromServices] MemoryStorage memoryStorage)
    {
        _stopwatch.Start();
        try
        {
            var users = memoryStorage.Users
                .Where(u => u.Score >= 900 && u.Active)
                .Select(u => new
                {
                    u.Id,
                    u.Name,
                    u.Active,
                    u.Age,
                    u.Score,
                    u.Country,
                    u.Team
                });

            _stopwatch.Stop();
            return Ok(ResponseViewModel.Create(users, _stopwatch.Elapsed));
        }
        catch (Exception ex)
        {
            _stopwatch.Stop();
            return BadRequest(ResponseViewModel.CreateError(ex.Message, _stopwatch.Elapsed));
        }
    }

    [HttpGet("/top-countries")]
    public IActionResult GetTopCountries([FromServices] MemoryStorage memoryStorage)
    {
        _stopwatch.Start();
        try
        {
            var topCountries = memoryStorage.Users
                .GroupBy(u => u.Country)
                .Select(g => new { country = g.Key, total_users = g.Count() })
                .OrderByDescending(g => g.total_users)
                .Take(5);

            _stopwatch.Stop();
            return Ok(ResponseViewModel.Create(topCountries, _stopwatch.Elapsed));
        }
        catch (Exception ex)
        {
            _stopwatch.Stop();
            return BadRequest(ResponseViewModel.CreateError(ex.Message, _stopwatch.Elapsed));
        }
    }

    [HttpGet("/team-insights")]
    public IActionResult GetTeamInsights([FromServices] MemoryStorage memoryStorage)
    {
        _stopwatch.Start();
        try
        {
            var maxDate = memoryStorage.Users.SelectMany(u => u.Logs).Select(l => l.Date).OrderByDescending(d => d).FirstOrDefault();

            var teamInsights = memoryStorage.Users
                .GroupBy(u => u.Team!.Name)
                .Select(g => new
                {
                    team = g.Key,
                    total_users = g.Count(),
                    average_score = g.Average(u => u.Score * 1m),
                    active_users = g.Count(u => u.Active) * 1m / g.Count(),
                    leaders = g.Count(u => u.Team!.Leader),
                    completed_projects = g.Sum(u => u.Team!.Projects.Count(p => p.Completed)),
                    engaged_users = g.Count(u => u.Active && u.Logs.Any(l => l.Action == ActionLevel.login && l.Date >= maxDate.AddDays(-30))) * 1m / g.Count()
                })
                .OrderByDescending(g => g.engaged_users)
                .ThenByDescending(g => g.active_users)
                .ThenByDescending(g => g.total_users)
                .ToDictionary(g => g.team, g => new
                {
                    g.total_users,
                    g.average_score,
                    g.active_users,
                    g.leaders,
                    g.completed_projects,
                    g.engaged_users
                });
            _stopwatch.Stop();
            return Ok(ResponseViewModel.Create(teamInsights, _stopwatch.Elapsed));
        }
        catch (Exception ex)
        {
            _stopwatch.Stop();
            return BadRequest(ResponseViewModel.CreateError(ex.Message, _stopwatch.Elapsed));
        }
    }

    [HttpGet("/active-users-per-day")]
    public IActionResult GetActiveUsersPerDay([FromServices] MemoryStorage memoryStorage, [FromQuery] int min = 3000)
    {
        _stopwatch.Start();
        try
        {
            var activeUsersPerDay = memoryStorage.Users
                .SelectMany(u => u.Logs)
                .GroupBy(log => log.Date)
                .Select(g => new
                {
                    date = g.Key.ToString("yyyy-MM-dd"),
                    total_users = g.Count(l => l.Action == ActionLevel.login)
                })
                .Where(g => g.total_users >= min)
                .OrderByDescending(g => g.total_users)
                .ToList();

            _stopwatch.Stop();
            return Ok(ResponseViewModel.Create(activeUsersPerDay, _stopwatch.Elapsed));
        }
        catch (Exception ex)
        {
            _stopwatch.Stop();
            return BadRequest(ResponseViewModel.CreateError(ex.Message, _stopwatch.Elapsed));
        }
    }

    [HttpGet("/evaluation")]
    public async Task<IActionResult> GetEvaluation()
    {
        _stopwatch.Start();

        var baseUri = Request.Scheme + "://" + Request.Host.Value + "/";
        var evaluationResults = new List<object>();

        var endpoints = new Dictionary<string, string>
        {
            { "GetSuperUsers", baseUri + "superusers" },
            { "GetTopCountries", baseUri + "top-countries" },
            { "GetTeamInsights", baseUri + "team-insights" },
            { "GetActiveUsersPerDay", baseUri + "active-users-per-day?min=3000" }
        };

        try
        {
            var requests = endpoints.AsParallel().Select(async endpoint =>
            {
                var client = new HttpClient();
                var response = await client.GetAsync(endpoint.Value);
                var content = await response.Content.ReadAsStringAsync();
                evaluationResults.Add(new
                {
                    endpoint = endpoint.Key,
                    status = response.IsSuccessStatusCode,
                    elapsedMilliseconds = response.Headers.Date.HasValue ? (DateTime.UtcNow - response.Headers.Date.Value).TotalMilliseconds : 0,
                    content = JsonSerializer.Deserialize<object>(content),
                });
            }).ToArray();

            await Task.WhenAll(requests);

            _stopwatch.Stop();

            return Ok(ResponseViewModel.Create(evaluationResults, _stopwatch.Elapsed, "Evaluation completed successfully."));
        }
        catch (Exception ex)
        {
            _stopwatch.Stop();
            return BadRequest(ResponseViewModel.CreateError(ex.Message, _stopwatch.Elapsed));
        }
    }
}
