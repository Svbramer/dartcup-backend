using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using DartCupBackend.Services;
using DartCupBackend.Models;

namespace DartCupBackend.Controllers;

[Authorize]
[ApiController]
[Route("api/[controller]")]
public class MatchesController : ControllerBase
{
    private readonly IMatchService _matchService;
    private readonly IPlayerService _playerService;
    
    public MatchesController(IMatchService matchService, IPlayerService playerService)
    {
        _matchService = matchService;
        _playerService = playerService;
    }
    
    [HttpGet]
    public async Task<IActionResult> GetAllMatches([FromQuery] int count = 10)
    {
        var matches = await _matchService.GetRecentMatchesAsync(count);
        return Ok(matches.Select(m => new 
        {
            m.Id,
            m.Player1Id,
            Player1 = m.Player1 != null ? new { m.Player1.Id, m.Player1.Name } : null,
            m.Player2Id,
            Player2 = m.Player2 != null ? new { m.Player2.Id, m.Player2.Name } : null,
            m.WinnerId,
            Winner = m.Winner != null ? new { m.Winner.Id, m.Winner.Name } : null,
            m.ScorePlayer1,
            m.ScorePlayer2,
            m.Date,
            m.LeagueId,
            m.GameType,
            m.IsCompleted,
            m.Notes
        }));
    }
    
    [HttpGet("me")]
    public async Task<IActionResult> GetMyMatches()
    {
        var userId = User.FindFirst("sub")?.Value;
        if (userId == null || !Guid.TryParse(userId, out var guidUserId))
        {
            return Unauthorized(new { Message = "Not authenticated" });
        }
        
        var matches = await _matchService.GetMatchesByUserIdAsync(guidUserId);
        return Ok(matches.Select(m => new 
        {
            m.Id,
            m.Player1Id,
            Player1 = m.Player1 != null ? new { m.Player1.Id, m.Player1.Name } : null,
            m.Player2Id,
            Player2 = m.Player2 != null ? new { m.Player2.Id, m.Player2.Name } : null,
            m.WinnerId,
            Winner = m.Winner != null ? new { m.Winner.Id, m.Winner.Name } : null,
            m.ScorePlayer1,
            m.ScorePlayer2,
            m.Date,
            m.LeagueId,
            m.GameType,
            m.IsCompleted,
            m.Notes
        }));
    }
    
    [HttpGet("{id}")]
    public async Task<IActionResult> GetMatch(Guid id)
    {
        var match = await _matchService.GetMatchByIdAsync(id);
        if (match == null)
        {
            return NotFound(new { Message = "Match not found" });
        }
        
        return Ok(new 
        {
            match.Id,
            match.Player1Id,
            Player1 = match.Player1 != null ? new { match.Player1.Id, match.Player1.Name } : null,
            match.Player2Id,
            Player2 = match.Player2 != null ? new { match.Player2.Id, match.Player2.Name } : null,
            match.WinnerId,
            Winner = match.Winner != null ? new { match.Winner.Id, match.Winner.Name } : null,
            match.ScorePlayer1,
            match.ScorePlayer2,
            match.Date,
            match.LeagueId,
            match.GameType,
            match.IsCompleted,
            match.Notes
        });
    }
    
    [HttpPost]
    public async Task<IActionResult> CreateMatch([FromBody] CreateMatchRequest request)
    {
        var userId = User.FindFirst("sub")?.Value;
        if (userId == null || !Guid.TryParse(userId, out var guidUserId))
        {
            return Unauthorized(new { Message = "Not authenticated" });
        }
        
        // Verify players belong to current user
        var player1 = await _playerService.GetPlayerByIdAsync(request.Player1Id);
        var player2 = await _playerService.GetPlayerByIdAsync(request.Player2Id);
        
        if (player1 == null || player2 == null)
        {
            return BadRequest(new { Message = "One or both players not found" });
        }
        
        if (player1.UserId != guidUserId || player2.UserId != guidUserId)
        {
            return Forbid();
        }
        
        var match = new Match
        {
            Player1Id = request.Player1Id,
            Player2Id = request.Player2Id,
            ScorePlayer1 = request.ScorePlayer1,
            ScorePlayer2 = request.ScorePlayer2,
            Date = DateTime.UtcNow,
            LeagueId = request.LeagueId,
            GameType = request.GameType,
            IsCompleted = request.IsCompleted,
            Notes = request.Notes
        };
        
        var createdMatch = await _matchService.CreateMatchAsync(match);
        
        return CreatedAtAction(nameof(GetMatch), new { id = createdMatch.Id }, new 
        {
            Message = "Match created successfully",
            Match = new 
            {
                createdMatch.Id,
                createdMatch.Player1Id,
                createdMatch.Player2Id,
                createdMatch.ScorePlayer1,
                createdMatch.ScorePlayer2,
                createdMatch.Date,
                createdMatch.GameType,
                createdMatch.IsCompleted
            }
        });
    }
    
    [HttpPut("{id}")]
    public async Task<IActionResult> UpdateMatch(Guid id, [FromBody] UpdateMatchRequest request)
    {
        var userId = User.FindFirst("sub")?.Value;
        if (userId == null || !Guid.TryParse(userId, out var guidUserId))
        {
            return Unauthorized(new { Message = "Not authenticated" });
        }
        
        var existingMatch = await _matchService.GetMatchByIdAsync(id);
        if (existingMatch == null)
        {
            return NotFound(new { Message = "Match not found" });
        }
        
        // Verify players belong to current user
        var player1 = await _playerService.GetPlayerByIdAsync(existingMatch.Player1Id);
        var player2 = await _playerService.GetPlayerByIdAsync(existingMatch.Player2Id);
        
        if (player1 == null || player2 == null || player1.UserId != guidUserId || player2.UserId != guidUserId)
        {
            return Forbid();
        }
        
        var matchToUpdate = new Match
        {
            Id = id,
            Player1Id = existingMatch.Player1Id,
            Player2Id = existingMatch.Player2Id,
            WinnerId = request.WinnerId,
            ScorePlayer1 = request.ScorePlayer1 ?? existingMatch.ScorePlayer1,
            ScorePlayer2 = request.ScorePlayer2 ?? existingMatch.ScorePlayer2,
            Date = existingMatch.Date,
            LeagueId = request.LeagueId ?? existingMatch.LeagueId,
            GameType = request.GameType ?? existingMatch.GameType,
            IsCompleted = request.IsCompleted ?? existingMatch.IsCompleted,
            Notes = request.Notes ?? existingMatch.Notes
        };
        
        var updatedMatch = await _matchService.UpdateMatchAsync(id, matchToUpdate);
        
        return Ok(new 
        {
            Message = "Match updated successfully",
            Match = new 
            {
                updatedMatch.Id,
                updatedMatch.Player1Id,
                updatedMatch.Player2Id,
                updatedMatch.WinnerId,
                updatedMatch.ScorePlayer1,
                updatedMatch.ScorePlayer2,
                updatedMatch.IsCompleted
            }
        });
    }
    
    [HttpDelete("{id}")]
    public async Task<IActionResult> DeleteMatch(Guid id)
    {
        var userId = User.FindFirst("sub")?.Value;
        if (userId == null || !Guid.TryParse(userId, out var guidUserId))
        {
            return Unauthorized(new { Message = "Not authenticated" });
        }
        
        var existingMatch = await _matchService.GetMatchByIdAsync(id);
        if (existingMatch == null)
        {
            return NotFound(new { Message = "Match not found" });
        }
        
        // Verify match belongs to current user
        var player1 = await _playerService.GetPlayerByIdAsync(existingMatch.Player1Id);
        var player2 = await _playerService.GetPlayerByIdAsync(existingMatch.Player2Id);
        
        if (player1 == null || player2 == null || player1.UserId != guidUserId || player2.UserId != guidUserId)
        {
            return Forbid();
        }
        
        await _matchService.DeleteMatchAsync(id);
        
        return Ok(new { Message = "Match deleted successfully" });
    }
    
    [HttpGet("league/{leagueId}")]
    public async Task<IActionResult> GetMatchesByLeague(Guid leagueId)
    {
        var matches = await _matchService.GetMatchesByLeagueIdAsync(leagueId);
        return Ok(matches.Select(m => new 
        {
            m.Id,
            m.Player1Id,
            Player1 = m.Player1 != null ? new { m.Player1.Id, m.Player1.Name } : null,
            m.Player2Id,
            Player2 = m.Player2 != null ? new { m.Player2.Id, m.Player2.Name } : null,
            m.WinnerId,
            Winner = m.Winner != null ? new { m.Winner.Id, m.Winner.Name } : null,
            m.ScorePlayer1,
            m.ScorePlayer2,
            m.Date,
            m.GameType,
            m.IsCompleted
        }));
    }
    
    [HttpGet("player/{playerId}")]
    public async Task<IActionResult> GetMatchesByPlayer(Guid playerId)
    {
        var matches = await _matchService.GetMatchesByPlayerIdAsync(playerId);
        return Ok(matches.Select(m => new 
        {
            m.Id,
            m.Player1Id,
            Player1 = m.Player1 != null ? new { m.Player1.Id, m.Player1.Name } : null,
            m.Player2Id,
            Player2 = m.Player2 != null ? new { m.Player2.Id, m.Player2.Name } : null,
            m.WinnerId,
            Winner = m.Winner != null ? new { m.Winner.Id, m.Winner.Name } : null,
            m.ScorePlayer1,
            m.ScorePlayer2,
            m.Date,
            m.GameType,
            m.IsCompleted
        }));
    }
}

public class CreateMatchRequest
{
    public Guid Player1Id { get; set; }
    public Guid Player2Id { get; set; }
    public int ScorePlayer1 { get; set; } = 0;
    public int ScorePlayer2 { get; set; } = 0;
    public Guid? LeagueId { get; set; }
    public string GameType { get; set; } = "501";
    public bool IsCompleted { get; set; } = false;
    public string? Notes { get; set; }
}

public class UpdateMatchRequest
{
    public Guid? WinnerId { get; set; }
    public int? ScorePlayer1 { get; set; }
    public int? ScorePlayer2 { get; set; }
    public Guid? LeagueId { get; set; }
    public string? GameType { get; set; }
    public bool? IsCompleted { get; set; }
    public string? Notes { get; set; }
}
