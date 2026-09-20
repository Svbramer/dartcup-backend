using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using DartCupBackend.Services;
using DartCupBackend.Models;

namespace DartCupBackend.Controllers;

[Authorize]
[ApiController]
[Route("api/[controller]")]
public class PlayersController : ControllerBase
{
    private readonly IPlayerService _playerService;
    private readonly IUserService _userService;
    
    public PlayersController(IPlayerService playerService, IUserService userService)
    {
        _playerService = playerService;
        _userService = userService;
    }
    
    [HttpGet]
    public async Task<IActionResult> GetAllPlayers()
    {
        var players = await _playerService.GetAllPlayersAsync();
        return Ok(players.Select(p => new 
        {
            p.Id,
            p.UserId,
            p.Name,
            p.AvatarUrl,
            p.CreatedAt
        }));
    }
    
    [HttpGet("me")]
    public async Task<IActionResult> GetMyPlayers()
    {
        var userId = User.FindFirst("sub")?.Value;
        if (userId == null || !Guid.TryParse(userId, out var guidUserId))
        {
            return Unauthorized(new { Message = "Not authenticated" });
        }
        
        var players = await _playerService.GetPlayersByUserIdAsync(guidUserId);
        return Ok(players.Select(p => new 
        {
            p.Id,
            p.Name,
            p.AvatarUrl,
            p.CreatedAt
        }));
    }
    
    [HttpGet("{id}")]
    public async Task<IActionResult> GetPlayer(Guid id)
    {
        var player = await _playerService.GetPlayerByIdAsync(id);
        if (player == null)
        {
            return NotFound(new { Message = "Player not found" });
        }
        
        return Ok(new 
        {
            player.Id,
            player.UserId,
            player.Name,
            player.AvatarUrl,
            player.CreatedAt
        });
    }
    
    [HttpPost]
    public async Task<IActionResult> CreatePlayer([FromBody] CreatePlayerRequest request)
    {
        var userId = User.FindFirst("sub")?.Value;
        if (userId == null || !Guid.TryParse(userId, out var guidUserId))
        {
            return Unauthorized(new { Message = "Not authenticated" });
        }
        
        var player = new Player
        {
            UserId = guidUserId,
            Name = request.Name,
            AvatarUrl = request.AvatarUrl
        };
        
        var createdPlayer = await _playerService.CreatePlayerAsync(player);
        
        return CreatedAtAction(nameof(GetPlayer), new { id = createdPlayer.Id }, new 
        {
            Message = "Player created successfully",
            Player = new 
            {
                createdPlayer.Id,
                createdPlayer.Name,
                createdPlayer.AvatarUrl
            }
        });
    }
    
    [HttpPut("{id}")]
    public async Task<IActionResult> UpdatePlayer(Guid id, [FromBody] UpdatePlayerRequest request)
    {
        var userId = User.FindFirst("sub")?.Value;
        if (userId == null || !Guid.TryParse(userId, out var guidUserId))
        {
            return Unauthorized(new { Message = "Not authenticated" });
        }
        
        var existingPlayer = await _playerService.GetPlayerByIdAsync(id);
        if (existingPlayer == null)
        {
            return NotFound(new { Message = "Player not found" });
        }
        
        // Check if player belongs to current user
        if (existingPlayer.UserId != guidUserId)
        {
            return Forbid();
        }
        
        existingPlayer.Name = request.Name ?? existingPlayer.Name;
        existingPlayer.AvatarUrl = request.AvatarUrl ?? existingPlayer.AvatarUrl;
        
        var updatedPlayer = await _playerService.UpdatePlayerAsync(id, existingPlayer);
        
        return Ok(new 
        {
            Message = "Player updated successfully",
            Player = new 
            {
                updatedPlayer.Id,
                updatedPlayer.Name,
                updatedPlayer.AvatarUrl
            }
        });
    }
    
    [HttpDelete("{id}")]
    public async Task<IActionResult> DeletePlayer(Guid id)
    {
        var userId = User.FindFirst("sub")?.Value;
        if (userId == null || !Guid.TryParse(userId, out var guidUserId))
        {
            return Unauthorized(new { Message = "Not authenticated" });
        }
        
        var existingPlayer = await _playerService.GetPlayerByIdAsync(id);
        if (existingPlayer == null)
        {
            return NotFound(new { Message = "Player not found" });
        }
        
        // Check if player belongs to current user
        if (existingPlayer.UserId != guidUserId)
        {
            return Forbid();
        }
        
        await _playerService.DeletePlayerAsync(id);
        
        return Ok(new { Message = "Player deleted successfully" });
    }
}

public class CreatePlayerRequest
{
    public string Name { get; set; } = string.Empty;
    public string? AvatarUrl { get; set; }
}

public class UpdatePlayerRequest
{
    public string? Name { get; set; }
    public string? AvatarUrl { get; set; }
}
