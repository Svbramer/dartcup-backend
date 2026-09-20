using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using DartCupBackend.Services;
using DartCupBackend.Models;

namespace DartCupBackend.Controllers;

[Authorize]
[ApiController]
[Route("api/[controller]")]
public class UsersController : ControllerBase
{
    private readonly IUserService _userService;
    
    public UsersController(IUserService userService)
    {
        _userService = userService;
    }
    
    [HttpGet("{id}")]
    public async Task<IActionResult> GetUser(Guid id)
    {
        var user = await _userService.GetUserByIdAsync(id);
        if (user == null)
        {
            return NotFound(new { Message = "User not found" });
        }
        
        return Ok(new 
        {
            user.Id,
            user.Email,
            user.FirstName,
            user.LastName,
            user.IsPro,
            user.ProfileImageUrl,
            user.CreatedAt,
            user.LastLogin
        });
    }
    
    [HttpGet("me")]
    public async Task<IActionResult> GetCurrentUser()
    {
        var userId = User.FindFirst("sub")?.Value;
        if (userId == null || !Guid.TryParse(userId, out var guidUserId))
        {
            return Unauthorized(new { Message = "Not authenticated" });
        }
        
        var user = await _userService.GetUserByIdAsync(guidUserId);
        if (user == null)
        {
            return NotFound(new { Message = "User not found" });
        }
        
        return Ok(new 
        {
            user.Id,
            user.Email,
            user.FirstName,
            user.LastName,
            user.IsPro,
            user.ProfileImageUrl,
            user.CreatedAt,
            user.LastLogin
        });
    }
    
    [HttpPut("me")]
    public async Task<IActionResult> UpdateCurrentUser([FromBody] UpdateUserRequest request)
    {
        var userId = User.FindFirst("sub")?.Value;
        if (userId == null || !Guid.TryParse(userId, out var guidUserId))
        {
            return Unauthorized(new { Message = "Not authenticated" });
        }
        
        var existingUser = await _userService.GetUserByIdAsync(guidUserId);
        if (existingUser == null)
        {
            return NotFound(new { Message = "User not found" });
        }
        
        existingUser.FirstName = request.FirstName ?? existingUser.FirstName;
        existingUser.LastName = request.LastName ?? existingUser.LastName;
        existingUser.ProfileImageUrl = request.ProfileImageUrl ?? existingUser.ProfileImageUrl;
        
        var updatedUser = await _userService.UpdateUserAsync(guidUserId, existingUser);
        
        return Ok(new 
        {
            Message = "User updated successfully",
            User = new 
            {
                updatedUser.Id,
                updatedUser.Email,
                updatedUser.FirstName,
                updatedUser.LastName,
                updatedUser.IsPro,
                updatedUser.ProfileImageUrl
            }
        });
    }
    
    [HttpGet("{id}/stats")]
    public async Task<IActionResult> GetUserStats(Guid id)
    {
        var stats = await _userService.GetUserStatsAsync(id);
        if (stats == null)
        {
            return Ok(new 
            {
                LoginCount = 0,
                TotalMatches = 0,
                TotalWins = 0,
                TotalScore = 0,
                ProAccessUnlocked = false
            });
        }
        
        return Ok(new 
        {
            stats.LoginCount,
            stats.LastLogin,
            stats.TotalMatches,
            stats.TotalWins,
            stats.TotalScore,
            stats.ProAccessUnlocked
        });
    }
    
    [HttpGet("stats")]
    public async Task<IActionResult> GetCurrentUserStats()
    {
        var userId = User.FindFirst("sub")?.Value;
        if (userId == null || !Guid.TryParse(userId, out var guidUserId))
        {
            return Unauthorized(new { Message = "Not authenticated" });
        }
        
        var stats = await _userService.GetUserStatsAsync(guidUserId);
        if (stats == null)
        {
            return Ok(new 
            {
                LoginCount = 0,
                TotalMatches = 0,
                TotalWins = 0,
                TotalScore = 0,
                ProAccessUnlocked = false
            });
        }
        
        return Ok(new 
        {
            stats.LoginCount,
            stats.LastLogin,
            stats.TotalMatches,
            stats.TotalWins,
            stats.TotalScore,
            stats.ProAccessUnlocked
        });
    }
}

public class UpdateUserRequest
{
    public string? FirstName { get; set; }
    public string? LastName { get; set; }
    public string? ProfileImageUrl { get; set; }
}
