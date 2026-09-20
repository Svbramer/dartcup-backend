using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using DartCupBackend.Services;
using DartCupBackend.Models;

namespace DartCupBackend.Controllers;

[Authorize]
[ApiController]
[Route("api/[controller]")]
public class TrainingImagesController : ControllerBase
{
    private readonly IUserService _userService;
    
    public TrainingImagesController(IUserService userService)
    {
        _userService = userService;
    }
    
    [HttpGet("pro")]
    public async Task<IActionResult> GetProTrainingImages()
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
        
        // Check if user is Pro
        if (!user.IsPro)
        {
            return Forbid();
        }
        
        // This would query the database for Pro training images
        // For now, return a placeholder
        return Ok(new 
        {
            Message = "Pro training images endpoint",
            Images = new List<object> { 
                new { Id = Guid.NewGuid(), Title = "Example Pro Training", ImageUrl = "/images/pro1.jpg" }
            }
        });
    }
    
    [HttpGet("public")]
    public async Task<IActionResult> GetPublicTrainingImages()
    {
        // Public training images available to all users
        return Ok(new 
        {
            Message = "Public training images endpoint",
            Images = new List<object> { 
                new { Id = Guid.NewGuid(), Title = "Example Public Training", ImageUrl = "/images/public1.jpg" }
            }
        });
    }
    
    [Authorize]
    [HttpPost]
    public async Task<IActionResult> UploadTrainingImage([FromBody] UploadTrainingImageRequest request)
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
        
        // Check if user is Pro (if IsProOnly is true)
        if (request.IsProOnly && !user.IsPro)
        {
            return Forbid();
        }
        
        // This would save the image to storage and create a database record
        // For now, return a placeholder
        return Ok(new 
        {
            Message = "Training image uploaded successfully",
            Image = new 
            {
                Id = Guid.NewGuid(),
                Title = request.Title,
                ImageUrl = request.ImageUrl,
                IsProOnly = request.IsProOnly
            }
        });
    }
}

public class UploadTrainingImageRequest
{
    public string Title { get; set; } = string.Empty;
    public string ImageUrl { get; set; } = string.Empty;
    public string? Description { get; set; }
    public bool IsProOnly { get; set; } = true;
    public int? HighScore { get; set; }
    public string? GameType { get; set; }
}
