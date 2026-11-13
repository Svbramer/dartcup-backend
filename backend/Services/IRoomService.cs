using DartCupBackend.Database;
using DartCupBackend.Models;
using Microsoft.EntityFrameworkCore;

namespace DartCupBackend.Services;

public interface IRoomService
{
    Task<Room> CreateRoomAsync(Guid userId, string gameType);
    Task<Room?> JoinRoomByCodeAsync(string code, Guid userId);
    Task<Room?> GetRoomAsync(Guid roomId);
    Task<Room?> GetRoomByCodeAsync(string code);
    Task<RoomSession> AddSessionAsync(Guid roomId, Guid userId, string deviceName, string roles);
    Task<ScoreEntry> AddScoreAsync(Guid roomId, Guid sessionId, Guid userId, int visitNumber, int value);
    Task<ScoreEntry> UpdatePendingScoreAsync(Guid roomId, Guid entryId, Guid userId, int value);
    Task<Room> ConfirmScoreAsync(Guid roomId, Guid entryId, Guid userId);
    Task<Room> RequestBoardAsync(Guid roomId, Guid sessionId);
    Task<Room> GrantBoardAsync(Guid roomId, Guid holderSessionId, Guid targetSessionId);
    Task<Room> ReturnBoardAsync(Guid roomId, Guid sessionId);
    string GenerateCode();
}

public class RoomService : IRoomService
{
    private readonly DartCupDbContext _context;
    private static readonly Random _random = new();
    private const string CodeChars = "ABCDEFGHJKLMNPQRSTUVWXYZ23456789";

    public RoomService(DartCupDbContext context)
    {
        _context = context;
    }

    public string GenerateCode()
    {
        return new string(Enumerable.Range(0, 6).Select(_ => CodeChars[_random.Next(CodeChars.Length)]).ToArray());
    }

    public async Task<Room> CreateRoomAsync(Guid userId, string gameType)
    {
        var room = new Room
        {
            Code = GenerateCode(),
            GameType = gameType,
            Player1Id = userId,
            Status = "waiting",
            CurrentTurn = 1
        };
        _context.Rooms.Add(room);
        await _context.SaveChangesAsync();
        return room;
    }

    public async Task<Room?> JoinRoomByCodeAsync(string code, Guid userId)
    {
        var room = await _context.Rooms
            .Include(r => r.Sessions)
            .FirstOrDefaultAsync(r => r.Code == code.ToUpperInvariant());
        if (room == null || room.Status != "waiting") return null;
        if (room.Player1Id == userId) return room;

        room.Player2Id = userId;
        room.Status = "active";
        await _context.SaveChangesAsync();
        return room;
    }

    public async Task<Room?> GetRoomAsync(Guid roomId)
    {
        return await _context.Rooms
            .Include(r => r.Sessions)
            .Include(r => r.ScoreEntries.OrderBy(e => e.SubmittedAt))
            .FirstOrDefaultAsync(r => r.Id == roomId);
    }

    public async Task<Room?> GetRoomByCodeAsync(string code)
    {
        return await _context.Rooms
            .Include(r => r.Sessions)
            .Include(r => r.ScoreEntries.OrderBy(e => e.SubmittedAt))
            .FirstOrDefaultAsync(r => r.Code == code.ToUpperInvariant());
    }

    public async Task<RoomSession> AddSessionAsync(Guid roomId, Guid userId, string deviceName, string roles)
    {
        var room = await GetRoomAsync(roomId) ?? throw new KeyNotFoundException("Room not found");

        var session = new RoomSession
        {
            RoomId = roomId,
            UserId = userId,
            DeviceName = deviceName,
            Roles = roles
        };
        _context.RoomSessions.Add(session);

        if (room.Status == "active" && room.BoardHolderSessionId == null)
        {
            var turnUserId = room.CurrentTurn == 1 ? room.Player1Id : room.Player2Id!.Value;
            var turnSession = room.Sessions.FirstOrDefault(s => s.UserId == turnUserId);
            if (turnSession != null)
            {
                room.BoardHolderSessionId = turnSession.Id;
            }
        }

        await _context.SaveChangesAsync();
        return session;
    }

    public async Task<ScoreEntry> AddScoreAsync(Guid roomId, Guid sessionId, Guid userId, int visitNumber, int value)
    {
        var room = await GetRoomAsync(roomId) ?? throw new KeyNotFoundException("Room not found");
        if (room.Status != "active") throw new InvalidOperationException("Room is not active");
        if (room.BoardHolderSessionId != sessionId) throw new InvalidOperationException("You do not hold the board");
        var turnUserId = room.CurrentTurn == 1 ? room.Player1Id : room.Player2Id!.Value;
        if (turnUserId != userId) throw new InvalidOperationException("Not your turn");

        var entry = new ScoreEntry
        {
            RoomId = roomId,
            PlayerId = userId,
            VisitNumber = visitNumber,
            Value = value,
            Status = "pending",
            SubmittedAt = DateTime.UtcNow
        };
        _context.ScoreEntries.Add(entry);
        await _context.SaveChangesAsync();
        return entry;
    }

    public async Task<ScoreEntry> UpdatePendingScoreAsync(Guid roomId, Guid entryId, Guid userId, int value)
    {
        var entry = await _context.ScoreEntries
            .Include(e => e.Room)
            .FirstOrDefaultAsync(e => e.Id == entryId && e.RoomId == roomId);
        if (entry == null) throw new KeyNotFoundException("Score entry not found");
        if (entry.Status != "pending") throw new InvalidOperationException("Entry already confirmed");
        if (entry.PlayerId != userId) throw new InvalidOperationException("Not your entry");

        entry.Value = value;
        entry.SubmittedAt = DateTime.UtcNow; // rettelsesvinduet starter forfra
        await _context.SaveChangesAsync();
        return entry;
    }

    public async Task<Room> ConfirmScoreAsync(Guid roomId, Guid entryId, Guid userId)
    {
        var room = await GetRoomAsync(roomId) ?? throw new KeyNotFoundException("Room not found");
        var entry = room.ScoreEntries.FirstOrDefault(e => e.Id == entryId)
            ?? throw new KeyNotFoundException("Score entry not found");
        if (entry.Status != "pending") throw new InvalidOperationException("Entry already confirmed");
        if (entry.PlayerId != userId) throw new InvalidOperationException("Not your entry");

        entry.Status = "confirmed";
        entry.ConfirmedAt = DateTime.UtcNow;

        room.CurrentTurn = room.CurrentTurn == 1 ? 2 : 1;
        room.BoardRequestFromSessionId = null;
        var nextUserId = room.CurrentTurn == 1 ? room.Player1Id : room.Player2Id!.Value;
        var nextSession = room.Sessions.FirstOrDefault(s => s.UserId == nextUserId);
        room.BoardHolderSessionId = nextSession?.Id;

        await _context.SaveChangesAsync();
        return room;
    }

    public async Task<Room> RequestBoardAsync(Guid roomId, Guid sessionId)
    {
        var room = await GetRoomAsync(roomId) ?? throw new KeyNotFoundException("Room not found");
        if (room.BoardHolderSessionId == sessionId) throw new InvalidOperationException("You already hold the board");
        var session = room.Sessions.FirstOrDefault(s => s.Id == sessionId)
            ?? throw new KeyNotFoundException("Session not found");
        room.BoardRequestFromSessionId = session.Id;
        await _context.SaveChangesAsync();
        return room;
    }

    public async Task<Room> GrantBoardAsync(Guid roomId, Guid holderSessionId, Guid targetSessionId)
    {
        var room = await GetRoomAsync(roomId) ?? throw new KeyNotFoundException("Room not found");
        if (room.BoardHolderSessionId != holderSessionId) throw new InvalidOperationException("Only the board holder can grant access");
        if (room.BoardRequestFromSessionId != targetSessionId) throw new InvalidOperationException("No pending request from that session");

        room.BoardHolderSessionId = targetSessionId;
        room.BoardRequestFromSessionId = null;
        await _context.SaveChangesAsync();
        return room;
    }

    public async Task<Room> ReturnBoardAsync(Guid roomId, Guid sessionId)
    {
        var room = await GetRoomAsync(roomId) ?? throw new KeyNotFoundException("Room not found");
        if (room.BoardHolderSessionId != sessionId) throw new InvalidOperationException("You do not hold the board");

        var turnUserId = room.CurrentTurn == 1 ? room.Player1Id : room.Player2Id!.Value;
        var turnSession = room.Sessions.FirstOrDefault(s => s.UserId == turnUserId);
        room.BoardHolderSessionId = turnSession?.Id;
        room.BoardRequestFromSessionId = null;
        await _context.SaveChangesAsync();
        return room;
    }
}
