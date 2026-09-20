using DartCupBackend.Models;

namespace DartCupBackend.Services;

public interface IUserService
{
    Task<User> GetUserByIdAsync(Guid userId);
    Task<User> GetUserByEmailAsync(string email);
    Task<User> CreateUserAsync(User user);
    Task<User> UpdateUserAsync(Guid userId, User user);
    Task<bool> DeleteUserAsync(Guid userId);
    Task<UserStats> GetUserStatsAsync(Guid userId);
    Task<UserStats> UpdateUserStatsAsync(Guid userId, UserStats stats);
    Task<bool> SetProStatusAsync(Guid userId, bool isPro);
}

public class UserService : IUserService
{
    private readonly DartCupDbContext _context;
    
    public UserService(DartCupDbContext context)
    {
        _context = context;
    }
    
    public async Task<User> GetUserByIdAsync(Guid userId)
    {
        return await _context.Users.FindAsync(userId);
    }
    
    public async Task<User> GetUserByEmailAsync(string email)
    {
        return await _context.Users.FirstOrDefaultAsync(u => u.Email == email);
    }
    
    public async Task<User> CreateUserAsync(User user)
    {
        _context.Users.Add(user);
        await _context.SaveChangesAsync();
        return user;
    }
    
    public async Task<User> UpdateUserAsync(Guid userId, User user)
    {
        var existingUser = await _context.Users.FindAsync(userId);
        if (existingUser == null)
        {
            throw new KeyNotFoundException("User not found");
        }
        
        existingUser.FirstName = user.FirstName;
        existingUser.LastName = user.LastName;
        existingUser.ProfileImageUrl = user.ProfileImageUrl;
        existingUser.IsPro = user.IsPro;
        
        await _context.SaveChangesAsync();
        return existingUser;
    }
    
    public async Task<bool> DeleteUserAsync(Guid userId)
    {
        var user = await _context.Users.FindAsync(userId);
        if (user == null)
        {
            return false;
        }
        
        _context.Users.Remove(user);
        await _context.SaveChangesAsync();
        return true;
    }
    
    public async Task<UserStats> GetUserStatsAsync(Guid userId)
    {
        return await _context.UserStats.FirstOrDefaultAsync(s => s.UserId == userId);
    }
    
    public async Task<UserStats> UpdateUserStatsAsync(Guid userId, UserStats stats)
    {
        var existingStats = await _context.UserStats.FindAsync(stats.Id);
        if (existingStats == null)
        {
            existingStats = new UserStats { UserId = userId };
            _context.UserStats.Add(existingStats);
        }
        
        existingStats.LoginCount = stats.LoginCount;
        existingStats.LastLogin = stats.LastLogin;
        existingStats.TotalMatches = stats.TotalMatches;
        existingStats.TotalWins = stats.TotalWins;
        existingStats.TotalScore = stats.TotalScore;
        existingStats.ProAccessUnlocked = stats.ProAccessUnlocked;
        
        await _context.SaveChangesAsync();
        return existingStats;
    }
    
    public async Task<bool> SetProStatusAsync(Guid userId, bool isPro)
    {
        var user = await _context.Users.FindAsync(userId);
        if (user == null)
        {
            return false;
        }
        
        user.IsPro = isPro;
        await _context.SaveChangesAsync();
        return true;
    }
}
