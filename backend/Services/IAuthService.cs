using System.Security.Claims;
using NHost.Auth;
using DartCupBackend.Models;

namespace DartCupBackend.Services;

public interface IAuthService
{
    Task<User> SignUpWithEmailAsync(string email, string password, string firstName, string lastName);
    Task<string> SignInWithEmailAsync(string email, string password);
    Task<User> GetCurrentUserAsync(ClaimsPrincipal user);
    Task<string> GenerateJwtTokenAsync(User user);
    Task<bool> ValidateTokenAsync(string token);
    Task<User> GetUserFromTokenAsync(string token);
}

public class AuthService : IAuthService
{
    private readonly IUserService _userService;
    private readonly IAuthenticationService _authService;
    
    public AuthService(IUserService userService, IAuthenticationService authService)
    {
        _userService = userService;
        _authService = authService;
    }
    
    public async Task<User> SignUpWithEmailAsync(string email, string password, string firstName, string lastName)
    {
        // Check if user already exists
        var existingUser = await _userService.GetUserByEmailAsync(email);
        if (existingUser != null)
        {
            throw new InvalidOperationException("User with this email already exists");
        }
        
        // Create new user
        var user = new User
        {
            Email = email,
            FirstName = firstName,
            LastName = lastName,
            IsPro = false
        };
        
        // Create user in database
        var createdUser = await _userService.CreateUserAsync(user);
        
        // Create auth user in NHost
        var authUser = new AuthUser
        {
            Email = email,
            Password = password,
            FirstName = firstName,
            LastName = lastName
        };
        
        await _authService.SignUpWithEmailPassword(authUser);
        
        return createdUser;
    }
    
    public async Task<string> SignInWithEmailAsync(string email, string password)
    {
        // Sign in via NHost auth
        var token = await _authService.SignInWithEmailPassword(email, password);
        
        // Update user's last login
        var user = await _userService.GetUserByEmailAsync(email);
        if (user != null)
        {
            user.LastLogin = DateTime.UtcNow;
            
            // Update stats
            var stats = await _userService.GetUserStatsAsync(user.Id);
            if (stats == null)
            {
                stats = new UserStats
                {
                    UserId = user.Id,
                    LoginCount = 1,
                    LastLogin = DateTime.UtcNow
                };
            }
            else
            {
                stats.LoginCount++;
                stats.LastLogin = DateTime.UtcNow;
            }
            await _userService.UpdateUserStatsAsync(user.Id, stats);
        }
        
        return token.AccessToken;
    }
    
    public async Task<User> GetCurrentUserAsync(ClaimsPrincipal user)
    {
        var userId = user.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        if (userId == null)
        {
            throw new UnauthorizedAccessException("User not authenticated");
        }
        
        if (!Guid.TryParse(userId, out var guidUserId))
        {
            throw new UnauthorizedAccessException("Invalid user ID");
        }
        
        return await _userService.GetUserByIdAsync(guidUserId);
    }
    
    public async Task<string> GenerateJwtTokenAsync(User user)
    {
        // This would be handled by NHost's auth service
        // For custom tokens, we'd need to implement JWT generation
        throw new NotImplementedException("Use NHost's built-in JWT generation");
    }
    
    public async Task<bool> ValidateTokenAsync(string token)
    {
        // This would be handled by NHost's auth service
        try
        {
            await _authService.ValidateToken(token);
            return true;
        }
        catch
        {
            return false;
        }
    }
    
    public async Task<User> GetUserFromTokenAsync(string token)
    {
        var userId = _authService.GetUserIdFromToken(token);
        return await _userService.GetUserByIdAsync(userId);
    }
}
