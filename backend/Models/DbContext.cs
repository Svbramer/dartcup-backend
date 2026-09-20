using Microsoft.EntityFrameworkCore;
using DartCupBackend.Models;

namespace DartCupBackend.Database;

public class DartCupDbContext : DbContext
{
    public DartCupDbContext(DbContextOptions<DartCupDbContext> options) : base(options)
    {
    }
    
    public DbSet<User> Users { get; set; }
    public DbSet<UserStats> UserStats { get; set; }
    public DbSet<Player> Players { get; set; }
    public DbSet<Match> Matches { get; set; }
    public DbSet<League> Leagues { get; set; }
    public DbSet<LeagueStanding> LeagueStandings { get; set; }
    public DbSet<TrainingImage> TrainingImages { get; set; }
    
    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        // Configure User -> UserStats relationship
        modelBuilder.Entity<User>()
            .HasOne(u => u.Stats)
            .WithOne(s => s.User)
            .HasForeignKey<UserStats>(s => s.UserId);
        
        // Configure User -> Player relationship
        modelBuilder.Entity<User>()
            .HasMany(u => u.Players)
            .WithOne(p => p.User)
            .HasForeignKey(p => p.UserId);
        
        // Configure User -> League relationship (Owner)
        modelBuilder.Entity<User>()
            .HasMany(u => u.Leagues)
            .WithOne(l => l.Owner)
            .HasForeignKey(l => l.OwnerId);
        
        // Configure Match relationships
        modelBuilder.Entity<Match>()
            .HasOne(m => m.Player1)
            .WithMany(p => p.MatchesAsPlayer1)
            .HasForeignKey(m => m.Player1Id);
        
        modelBuilder.Entity<Match>()
            .HasOne(m => m.Player2)
            .WithMany(p => p.MatchesAsPlayer2)
            .HasForeignKey(m => m.Player2Id);
        
        modelBuilder.Entity<Match>()
            .HasOne(m => m.Winner)
            .WithMany(p => p.MatchesWon)
            .HasForeignKey(m => m.WinnerId);
        
        // Configure League relationships
        modelBuilder.Entity<League>()
            .HasMany(l => l.Matches)
            .WithOne(m => m.League)
            .HasForeignKey(m => m.LeagueId);
        
        modelBuilder.Entity<League>()
            .HasMany(l => l.Standings)
            .WithOne(s => s.League)
            .HasForeignKey(s => s.LeagueId);
        
        modelBuilder.Entity<LeagueStanding>()
            .HasOne(s => s.Player)
            .WithMany(p => p.LeagueStandings)
            .HasForeignKey(s => s.PlayerId);
        
        // Configure TrainingImage relationship
        modelBuilder.Entity<TrainingImage>()
            .HasOne(t => t.User)
            .WithMany()
            .HasForeignKey(t => t.UserId);
    }
}
