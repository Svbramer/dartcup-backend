using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using DartCupBackend.Services;
using DartCupBackend.Models;

namespace DartCupBackend.Controllers;

[Authorize]
[ApiController]
[Route("api/[controller]")]
public class LeaguesController : ControllerBase
{
    private readonly ILeagueService _leagueService;
    private readonly IUserService _userService;
    
    public LeaguesController(ILeagueService leagueService, IUserService userService)
    {
        _leagueService = leagueService;
        _userService = userService;
    }
    
    [HttpGet]
    public async Task<IActionResult> GetAllLeagues([FromQuery] bool activeOnly = false)
    {
        var leagues = activeOnly 
            ? await _leagueService.GetActiveLeaguesAsync()
            : await _leagueService.GetAllLeaguesAsync();
        
        return Ok(leagues.Select(l => new 
        {
            l.Id,
            l.Name,
            l.OwnerId,
            Owner = l.Owner != null ? new { l.Owner.Id, l.Owner.FirstName, l.Owner.LastName } : null,
            l.StartDate,
            l.EndDate,
            l.IsActive,
            l.Description,
            MatchCount = l.Matches != null ? l.Matches.Count : 0
        }));
    }
    
    [HttpGet("me")]
    public async Task<IActionResult> GetMyLeagues()
    {
        var userId = User.FindFirst("sub")?.Value;
        if (userId == null || !Guid.TryParse(userId, out var guidUserId))
        {
            return Unauthorized(new { Message = "Not authenticated" });
        }
        
        var leagues = await _leagueService.GetLeaguesByOwnerIdAsync(guidUserId);
        return Ok(leagues.Select(l => new 
        {
            l.Id,
            l.Name,
            l.StartDate,
            l.EndDate,
            l.IsActive,
            l.Description,
            MatchCount = l.Matches != null ? l.Matches.Count : 0
        }));
    }
    
    [HttpGet("{id}")]
    public async Task<IActionResult> GetLeague(Guid id)
    {
        var league = await _leagueService.GetLeagueByIdAsync(id);
        if (league == null)
        {
            return NotFound(new { Message = "League not found" });
        }
        
        return Ok(new 
        {
            league.Id,
            league.Name,
            league.OwnerId,
            Owner = league.Owner != null ? new { league.Owner.Id, league.Owner.FirstName, league.Owner.LastName } : null,
            league.StartDate,
            league.EndDate,
            league.IsActive,
            league.Description,
            Matches = league.Matches?.Select(m => new 
            {
                m.Id,
                m.Player1Id,
                m.Player2Id,
                m.WinnerId,
                m.ScorePlayer1,
                m.ScorePlayer2,
                m.Date,
                m.GameType,
                m.IsCompleted
            }).ToList() ?? new List<object>(),
            Standings = league.Standings?.Select(s => new 
            {
                s.Id,
                s.PlayerId,
                Player = s.Player != null ? new { s.Player.Id, s.Player.Name } : null,
                s.Position,
                s.Points,
                s.Wins,
                s.Losses,
                s.WeeklyRank,
                s.MonthlyRank
            }).ToList() ?? new List<object>()
        });
    }
    
    [HttpPost]
    public async Task<IActionResult> CreateLeague([FromBody] CreateLeagueRequest request)
    {
        var userId = User.FindFirst("sub")?.Value;
        if (userId == null || !Guid.TryParse(userId, out var guidUserId))
        {
            return Unauthorized(new { Message = "Not authenticated" });
        }
        
        var league = new League
        {
            Name = request.Name,
            OwnerId = guidUserId,
            StartDate = request.StartDate,
            EndDate = request.EndDate,
            IsActive = request.IsActive,
            Description = request.Description
        };
        
        var createdLeague = await _leagueService.CreateLeagueAsync(league);
        
        return CreatedAtAction(nameof(GetLeague), new { id = createdLeague.Id }, new 
        {
            Message = "League created successfully",
            League = new 
            {
                createdLeague.Id,
                createdLeague.Name,
                createdLeague.StartDate,
                createdLeague.EndDate,
                createdLeague.IsActive
            }
        });
    }
    
    [HttpPut("{id}")]
    public async Task<IActionResult> UpdateLeague(Guid id, [FromBody] UpdateLeagueRequest request)
    {
        var userId = User.FindFirst("sub")?.Value;
        if (userId == null || !Guid.TryParse(userId, out var guidUserId))
        {
            return Unauthorized(new { Message = "Not authenticated" });
        }
        
        var existingLeague = await _leagueService.GetLeagueByIdAsync(id);
        if (existingLeague == null)
        {
            return NotFound(new { Message = "League not found" });
        }
        
        // Check if league belongs to current user
        if (existingLeague.OwnerId != guidUserId)
        {
            return Forbid();
        }
        
        var leagueToUpdate = new League
        {
            Id = id,
            Name = request.Name ?? existingLeague.Name,
            OwnerId = existingLeague.OwnerId,
            StartDate = request.StartDate ?? existingLeague.StartDate,
            EndDate = request.EndDate ?? existingLeague.EndDate,
            IsActive = request.IsActive ?? existingLeague.IsActive,
            Description = request.Description ?? existingLeague.Description
        };
        
        var updatedLeague = await _leagueService.UpdateLeagueAsync(id, leagueToUpdate);
        
        return Ok(new 
        {
            Message = "League updated successfully",
            League = new 
            {
                updatedLeague.Id,
                updatedLeague.Name,
                updatedLeague.StartDate,
                updatedLeague.EndDate,
                updatedLeague.IsActive
            }
        });
    }
    
    [HttpDelete("{id}")]
    public async Task<IActionResult> DeleteLeague(Guid id)
    {
        var userId = User.FindFirst("sub")?.Value;
        if (userId == null || !Guid.TryParse(userId, out var guidUserId))
        {
            return Unauthorized(new { Message = "Not authenticated" });
        }
        
        var existingLeague = await _leagueService.GetLeagueByIdAsync(id);
        if (existingLeague == null)
        {
            return NotFound(new { Message = "League not found" });
        }
        
        // Check if league belongs to current user
        if (existingLeague.OwnerId != guidUserId)
        {
            return Forbid();
        }
        
        await _leagueService.DeleteLeagueAsync(id);
        
        return Ok(new { Message = "League deleted successfully" });
    }
    
    [HttpGet("{id}/standings")]
    public async Task<IActionResult> GetLeagueStandings(Guid id)
    {
        var standings = await _leagueService.GetLeagueStandingsAsync(id);
        return Ok(standings.Select(s => new 
        {
            s.Id,
            s.LeagueId,
            s.PlayerId,
            Player = s.Player != null ? new { s.Player.Id, s.Player.Name } : null,
            s.Position,
            s.Points,
            s.Wins,
            s.Losses,
            s.WeeklyRank,
            s.MonthlyRank,
            s.UpdatedAt
        }));
    }
    
    [HttpGet("{id}/standings/me")]
    public async Task<IActionResult> GetMyStandingInLeague(Guid id)
    {
        var userId = User.FindFirst("sub")?.Value;
        if (userId == null || !Guid.TryParse(userId, out var guidUserId))
        {
            return Unauthorized(new { Message = "Not authenticated" });
        }
        
        // Get all players for this user
        var players = await _userService.GetUserByIdAsync(guidUserId);
        if (players == null)
        {
            return NotFound(new { Message = "User not found" });
        }
        
        // For simplicity, just get the first player (you might want to handle multiple players differently)
        var playerIds = new List<Guid>(); // Would be players.Players.Select(p => p.Id).ToList()
        
        foreach (var player in players.Players)
        {
            var standing = await _leagueService.GetPlayerStandingInLeagueAsync(id, player.Id);
            if (standing != null)
            {
                return Ok(new 
                {
                    standing.Id,
                    standing.LeagueId,
                    standing.PlayerId,
                    Player = standing.Player != null ? new { standing.Player.Id, standing.Player.Name } : null,
                    standing.Position,
                    standing.Points,
                    standing.Wins,
                    standing.Losses,
                    standing.WeeklyRank,
                    standing.MonthlyRank
                });
            }
        }
        
        return NotFound(new { Message = "No standing found for this user in the league" });
    }
}

public class CreateLeagueRequest
{
    public string Name { get; set; } = string.Empty;
    public DateTime StartDate { get; set; } = DateTime.UtcNow;
    public DateTime EndDate { get; set; } = DateTime.UtcNow.AddMonths(1);
    public bool IsActive { get; set; } = true;
    public string? Description { get; set; }
}

public class UpdateLeagueRequest
{
    public string? Name { get; set; }
    public DateTime? StartDate { get; set; }
    public DateTime? EndDate { get; set; }
    public bool? IsActive { get; set; }
    public string? Description { get; set; }
}
