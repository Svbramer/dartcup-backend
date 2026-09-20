using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using DartCupBackend.Services;
using DartCupBackend.Models;
using System.Security.Claims;

namespace DartCupBackend.Controllers;

[ApiController]
[Route("api/[controller]")]
public class AuthController : ControllerBase
{
    private readonly IAuthService _authService;
    private readonly IUserService _userService;
    
    public AuthController(IAuthService authService, IUserService userService)
    {
        _authService = authService;
        _userService = userService;
    }
    
    [HttpPost("sign-up")]
    public async Task<IActionResult> SignUp([FromBody] SignUpRequest request)
    {
        try
        {
            var user = await _authService.SignUpWithEmailAsync(
                request.Email,
                request.Password,
                request.FirstName,
                request.LastName
            );
            
            return Ok(new 
            {
                Message = "User created successfully",
                User = new 
                {
                    user.Id,
                    user.Email,
                    user.FirstName,
                    user.LastName,
                    user.IsPro
                }
            });
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(new { Message = ex.Message });
        }
        catch (Exception ex)
        {
            return StatusCode(500, new { Message = "An error occurred while creating user" });
        }
    }
    
    [HttpPost("sign-in")]
    public async Task<IActionResult> SignIn([FromBody] SignInRequest request)
    {
        try
        {
            var token = await _authService.SignInWithEmailAsync(
                request.Email,
                request.Password
            );
            
            var user = await _userService.GetUserByEmailAsync(request.Email);
            
            return Ok(new 
            {
                AccessToken = token,
                User = new 
                {
                    user.Id,
                    user.Email,
                    user.FirstName,
                    user.LastName,
                    user.IsPro,
                    user.ProfileImageUrl
                }
            });
        }
        catch (Exception ex)
        {
            return Unauthorized(new { Message = "Invalid email or password" });
        }
    }
    
    [Authorize]
    [HttpGet("me")]
    public async Task<IActionResult> GetCurrentUser()
    {
        try
        {
            var user = await _authService.GetCurrentUserAsync(User);
            return Ok(new 
            {
                user.Id,
                user.Email,
                user.FirstName,
                user.LastName,
                user.IsPro,
                user.ProfileImageUrl,
                user.LastLogin
            });
        }
        catch (UnauthorizedAccessException)
        {
            return Unauthorized(new { Message = "Not authenticated" });
        }
    }
    
    [Authorize]
    [HttpPost("refresh")]
    public async Task<IActionResult> RefreshToken([FromBody] RefreshTokenRequest request)
    {
        // This would be handled by NHost's auth service
        // For now, just return the same token
        return Ok(new { AccessToken = request.AccessToken });
    }
    
    [HttpPost("google")]
    public async Task<IActionResult> SignInWithGoogle([FromBody] GoogleAuthRequest request)
    {
        // This would be handled by NHost's Google OAuth
        // For now, return a placeholder
        return Ok(new { Message = "Google auth not yet configured" });
    }
}

// Request/Response models
public class SignUpRequest
{
    public string Email { get; set; } = string.Empty;
    public string Password { get; set; } = string.Empty;
    public string FirstName { get; set; } = string.Empty;
    public string LastName { get; set; } = string.Empty;
}

public class SignInRequest
{
    public string Email { get; set; } = string.Empty;
    public string Password { get; set; } = string.Empty;
}

public class RefreshTokenRequest
{
    public string AccessToken { get; set; } = string.Empty;
}

public class GoogleAuthRequest
{
    public string Token { get; set; } = string.Empty;
}
