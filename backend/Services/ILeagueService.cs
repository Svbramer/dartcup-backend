using DartCupBackend.Models;

namespace DartCupBackend.Services;

public interface ILeagueService
{
    Task<League> GetLeagueByIdAsync(Guid leagueId);
    Task<List<League>> GetLeaguesByOwnerIdAsync(Guid ownerId);
    Task<List<League>> GetAllLeaguesAsync();
    Task<List<League>> GetActiveLeaguesAsync();
    Task<League> CreateLeagueAsync(League league);
    Task<League> UpdateLeagueAsync(Guid leagueId, League league);
    Task<bool> DeleteLeagueAsync(Guid leagueId);
    Task<List<LeagueStanding>> GetLeagueStandingsAsync(Guid leagueId);
    Task<LeagueStanding> GetPlayerStandingInLeagueAsync(Guid leagueId, Guid playerId);
}

public class LeagueService : ILeagueService
{
    private readonly DartCupDbContext _context;
    
    public LeagueService(DartCupDbContext context)
    {
        _context = context;
    }
    
    public async Task<League> GetLeagueByIdAsync(Guid leagueId)
    {
        return await _context.Leagues
            .Include(l => l.Owner)
            .Include(l => l.Matches)
            .Include(l => l.Standings)
            .FirstOrDefaultAsync(l => l.Id == leagueId);
    }
    
    public async Task<List<League>> GetLeaguesByOwnerIdAsync(Guid ownerId)
    {
        return await _context.Leagues
            .Where(l => l.OwnerId == ownerId)
            .Include(l => l.Owner)
            .OrderByDescending(l => l.StartDate)
            .ToListAsync();
    }
    
    public async Task<List<League>> GetAllLeaguesAsync()
    {
        return await _context.Leagues
            .Include(l => l.Owner)
            .OrderByDescending(l => l.StartDate)
            .ToListAsync();
    }
    
    public async Task<List<League>> GetActiveLeaguesAsync()
    {
        return await _context.Leagues
            .Where(l => l.IsActive)
            .Include(l => l.Owner)
            .OrderByDescending(l => l.StartDate)
            .ToListAsync();
    }
    
    public async Task<League> CreateLeagueAsync(League league)
    {
        _context.Leagues.Add(league);
        await _context.SaveChangesAsync();
        return league;
    }
    
    public async Task<League> UpdateLeagueAsync(Guid leagueId, League league)
    {
        var existingLeague = await _context.Leagues.FindAsync(leagueId);
        if (existingLeague == null)
        {
            throw new KeyNotFoundException("League not found");
        }
        
        existingLeague.Name = league.Name;
        existingLeague.StartDate = league.StartDate;
        existingLeague.EndDate = league.EndDate;
        existingLeague.IsActive = league.IsActive;
        existingLeague.Description = league.Description;
        
        await _context.SaveChangesAsync();
        return existingLeague;
    }
    
    public async Task<bool> DeleteLeagueAsync(Guid leagueId)
    {
        var league = await _context.Leagues.FindAsync(leagueId);
        if (league == null)
        {
            return false;
        }
        
        _context.Leagues.Remove(league);
        await _context.SaveChangesAsync();
        return true;
    }
    
    public async Task<List<LeagueStanding>> GetLeagueStandingsAsync(Guid leagueId)
    {
        return await _context.LeagueStandings
            .Where(s => s.LeagueId == leagueId)
            .Include(s => s.Player)
            .OrderBy(s => s.Position)
            .ToListAsync();
    }
    
    public async Task<LeagueStanding> GetPlayerStandingInLeagueAsync(Guid leagueId, Guid playerId)
    {
        return await _context.LeagueStandings
            .FirstOrDefaultAsync(s => s.LeagueId == leagueId && s.PlayerId == playerId);
    }
}
