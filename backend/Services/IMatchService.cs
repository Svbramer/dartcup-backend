using DartCupBackend.Models;

namespace DartCupBackend.Services;

public interface IMatchService
{
    Task<Match> GetMatchByIdAsync(Guid matchId);
    Task<List<Match>> GetMatchesByPlayerIdAsync(Guid playerId);
    Task<List<Match>> GetMatchesByLeagueIdAsync(Guid leagueId);
    Task<List<Match>> GetRecentMatchesAsync(int count = 10);
    Task<Match> CreateMatchAsync(Match match);
    Task<Match> UpdateMatchAsync(Guid matchId, Match match);
    Task<bool> DeleteMatchAsync(Guid matchId);
    Task<List<Match>> GetMatchesByUserIdAsync(Guid userId);
    Task<int> GetUserWinCountAsync(Guid userId);
    Task<int> GetUserMatchCountAsync(Guid userId);
}

public class MatchService : IMatchService
{
    private readonly DartCupDbContext _context;
    
    public MatchService(DartCupDbContext context)
    {
        _context = context;
    }
    
    public async Task<Match> GetMatchByIdAsync(Guid matchId)
    {
        return await _context.Matches
            .Include(m => m.Player1)
            .Include(m => m.Player2)
            .Include(m => m.Winner)
            .Include(m => m.League)
            .FirstOrDefaultAsync(m => m.Id == matchId);
    }
    
    public async Task<List<Match>> GetMatchesByPlayerIdAsync(Guid playerId)
    {
        return await _context.Matches
            .Where(m => m.Player1Id == playerId || m.Player2Id == playerId)
            .Include(m => m.Player1)
            .Include(m => m.Player2)
            .Include(m => m.Winner)
            .Include(m => m.League)
            .OrderByDescending(m => m.Date)
            .ToListAsync();
    }
    
    public async Task<List<Match>> GetMatchesByLeagueIdAsync(Guid leagueId)
    {
        return await _context.Matches
            .Where(m => m.LeagueId == leagueId)
            .Include(m => m.Player1)
            .Include(m => m.Player2)
            .Include(m => m.Winner)
            .OrderByDescending(m => m.Date)
            .ToListAsync();
    }
    
    public async Task<List<Match>> GetRecentMatchesAsync(int count = 10)
    {
        return await _context.Matches
            .Include(m => m.Player1)
            .Include(m => m.Player2)
            .Include(m => m.Winner)
            .OrderByDescending(m => m.Date)
            .Take(count)
            .ToListAsync();
    }
    
    public async Task<Match> CreateMatchAsync(Match match)
    {
        _context.Matches.Add(match);
        await _context.SaveChangesAsync();
        
        // Update league standings if this match is part of a league
        if (match.LeagueId.HasValue && match.IsCompleted)
        {
            await UpdateLeagueStandingsAsync(match.LeagueId.Value);
        }
        
        return match;
    }
    
    public async Task<Match> UpdateMatchAsync(Guid matchId, Match match)
    {
        var existingMatch = await _context.Matches.FindAsync(matchId);
        if (existingMatch == null)
        {
            throw new KeyNotFoundException("Match not found");
        }
        
        existingMatch.Player1Id = match.Player1Id;
        existingMatch.Player2Id = match.Player2Id;
        existingMatch.WinnerId = match.WinnerId;
        existingMatch.ScorePlayer1 = match.ScorePlayer1;
        existingMatch.ScorePlayer2 = match.ScorePlayer2;
        existingMatch.Date = match.Date;
        existingMatch.LeagueId = match.LeagueId;
        existingMatch.GameType = match.GameType;
        existingMatch.IsCompleted = match.IsCompleted;
        existingMatch.Notes = match.Notes;
        
        await _context.SaveChangesAsync();
        
        // Update league standings if this match is part of a league
        if (match.LeagueId.HasValue && match.IsCompleted)
        {
            await UpdateLeagueStandingsAsync(match.LeagueId.Value);
        }
        
        return existingMatch;
    }
    
    public async Task<bool> DeleteMatchAsync(Guid matchId)
    {
        var match = await _context.Matches.FindAsync(matchId);
        if (match == null)
        {
            return false;
        }
        
        _context.Matches.Remove(match);
        await _context.SaveChangesAsync();
        return true;
    }
    
    public async Task<List<Match>> GetMatchesByUserIdAsync(Guid userId)
    {
        var playerIds = await _context.Players
            .Where(p => p.UserId == userId)
            .Select(p => p.Id)
            .ToListAsync();
        
        return await _context.Matches
            .Where(m => playerIds.Contains(m.Player1Id) || playerIds.Contains(m.Player2Id))
            .Include(m => m.Player1)
            .Include(m => m.Player2)
            .Include(m => m.Winner)
            .Include(m => m.League)
            .OrderByDescending(m => m.Date)
            .ToListAsync();
    }
    
    public async Task<int> GetUserWinCountAsync(Guid userId)
    {
        var playerIds = await _context.Players
            .Where(p => p.UserId == userId)
            .Select(p => p.Id)
            .ToListAsync();
        
        return await _context.Matches
            .CountAsync(m => playerIds.Contains(m.WinnerId ?? Guid.Empty));
    }
    
    public async Task<int> GetUserMatchCountAsync(Guid userId)
    {
        var playerIds = await _context.Players
            .Where(p => p.UserId == userId)
            .Select(p => p.Id)
            .ToListAsync();
        
        return await _context.Matches
            .CountAsync(m => playerIds.Contains(m.Player1Id) || playerIds.Contains(m.Player2Id));
    }
    
    private async Task UpdateLeagueStandingsAsync(Guid leagueId)
    {
        var league = await _context.Leagues
            .Include(l => l.Matches)
            .FirstOrDefaultAsync(l => l.Id == leagueId);
        
        if (league == null) return;
        
        // Get all players in the league
        var playerIds = league.Matches
            .SelectMany(m => new[] { m.Player1Id, m.Player2Id })
            .Distinct()
            .ToList();
        
        // Calculate standings for each player
        var standings = new List<LeagueStanding>();
        foreach (var playerId in playerIds)
        {
            var playerMatches = league.Matches
                .Where(m => m.Player1Id == playerId || m.Player2Id == playerId)
                .ToList();
            
            var wins = playerMatches.Count(m => m.WinnerId == playerId);
            var losses = playerMatches.Count - wins;
            var points = wins * 3 + losses * 1; // Example: 3 points for win, 1 for loss
            
            standings.Add(new LeagueStanding
            {
                LeagueId = leagueId,
                PlayerId = playerId,
                Wins = wins,
                Losses = losses,
                Points = points,
                UpdatedAt = DateTime.UtcNow
            });
        }
        
        // Sort by points (descending)
        standings = standings.OrderByDescending(s => s.Points).ToList();
        
        // Update positions
        for (int i = 0; i < standings.Count; i++)
        {
            standings[i].Position = i + 1;
        }
        
        // Clear existing standings for this league
        var existingStandings = await _context.LeagueStandings
            .Where(s => s.LeagueId == leagueId)
            .ToListAsync();
        
        _context.LeagueStandings.RemoveRange(existingStandings);
        
        // Add new standings
        _context.LeagueStandings.AddRange(standings);
        
        await _context.SaveChangesAsync();
    }
}
