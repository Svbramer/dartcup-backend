using DartCupBackend.Models;

namespace DartCupBackend.Services;

public interface IPlayerService
{
    Task<Player> GetPlayerByIdAsync(Guid playerId);
    Task<List<Player>> GetPlayersByUserIdAsync(Guid userId);
    Task<Player> CreatePlayerAsync(Player player);
    Task<Player> UpdatePlayerAsync(Guid playerId, Player player);
    Task<bool> DeletePlayerAsync(Guid playerId);
    Task<List<Player>> GetAllPlayersAsync();
}

public class PlayerService : IPlayerService
{
    private readonly DartCupDbContext _context;
    
    public PlayerService(DartCupDbContext context)
    {
        _context = context;
    }
    
    public async Task<Player> GetPlayerByIdAsync(Guid playerId)
    {
        return await _context.Players.FindAsync(playerId);
    }
    
    public async Task<List<Player>> GetPlayersByUserIdAsync(Guid userId)
    {
        return await _context.Players
            .Where(p => p.UserId == userId)
            .ToListAsync();
    }
    
    public async Task<Player> CreatePlayerAsync(Player player)
    {
        _context.Players.Add(player);
        await _context.SaveChangesAsync();
        return player;
    }
    
    public async Task<Player> UpdatePlayerAsync(Guid playerId, Player player)
    {
        var existingPlayer = await _context.Players.FindAsync(playerId);
        if (existingPlayer == null)
        {
            throw new KeyNotFoundException("Player not found");
        }
        
        existingPlayer.Name = player.Name;
        existingPlayer.AvatarUrl = player.AvatarUrl;
        
        await _context.SaveChangesAsync();
        return existingPlayer;
    }
    
    public async Task<bool> DeletePlayerAsync(Guid playerId)
    {
        var player = await _context.Players.FindAsync(playerId);
        if (player == null)
        {
            return false;
        }
        
        _context.Players.Remove(player);
        await _context.SaveChangesAsync();
        return true;
    }
    
    public async Task<List<Player>> GetAllPlayersAsync()
    {
        return await _context.Players.ToListAsync();
    }
}
