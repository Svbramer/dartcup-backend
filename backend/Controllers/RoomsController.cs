using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using DartCupBackend.Services;
using DartCupBackend.Models;

namespace DartCupBackend.Controllers;

[Authorize]
[ApiController]
[Route("api/[controller]")]
public class RoomsController : ControllerBase
{
    private readonly IRoomService _roomService;

    public RoomsController(IRoomService roomService)
    {
        _roomService = roomService;
    }

    private Guid GetUserId()
    {
        var userId = User.FindFirst("sub")?.Value;
        if (userId == null || !Guid.TryParse(userId, out var guidUserId))
        {
            throw new UnauthorizedAccessException("Not authenticated");
        }
        return guidUserId;
    }

    private static object RoomDto(Room r) => new
    {
        r.Id,
        r.Code,
        r.GameType,
        r.Player1Id,
        r.Player2Id,
        r.Status,
        r.CurrentTurn,
        r.BoardHolderSessionId,
        r.BoardRequestFromSessionId,
        r.CreatedAt,
        Sessions = r.Sessions.Select(s => new { s.Id, s.UserId, s.DeviceName, s.Roles, s.JoinedAt }),
        ScoreEntries = r.ScoreEntries.Select(e => new { e.Id, e.PlayerId, e.VisitNumber, e.Value, e.Status, e.SubmittedAt, e.ConfirmedAt })
    };

    [HttpPost]
    public async Task<IActionResult> CreateRoom([FromBody] CreateRoomRequest request)
    {
        var room = await _roomService.CreateRoomAsync(GetUserId(), request.GameType ?? "201");
        return Ok(RoomDto(room));
    }

    [HttpPost("join")]
    public async Task<IActionResult> JoinRoom([FromBody] JoinRoomRequest request)
    {
        var room = await _roomService.JoinRoomByCodeAsync(request.Code, GetUserId());
        if (room == null) return NotFound(new { Message = "Room not found or not waiting for players" });
        var full = await _roomService.GetRoomAsync(room.Id);
        return Ok(RoomDto(full!));
    }

    [HttpGet("{id}")]
    public async Task<IActionResult> GetRoom(Guid id)
    {
        var room = await _roomService.GetRoomAsync(id);
        if (room == null) return NotFound(new { Message = "Room not found" });
        return Ok(RoomDto(room));
    }

    [HttpGet("by-code/{code}")]
    public async Task<IActionResult> GetRoomByCode(string code)
    {
        var room = await _roomService.GetRoomByCodeAsync(code);
        if (room == null) return NotFound(new { Message = "Room not found" });
        return Ok(RoomDto(room));
    }

    [HttpPost("{id}/sessions")]
    public async Task<IActionResult> AddSession(Guid id, [FromBody] AddSessionRequest request)
    {
        var session = await _roomService.AddSessionAsync(id, GetUserId(), request.DeviceName, request.Roles);
        var room = await _roomService.GetRoomAsync(id);
        return Ok(new { Session = session, Room = RoomDto(room!) });
    }

    [HttpPost("{id}/scores")]
    public async Task<IActionResult> AddScore(Guid id, [FromBody] AddScoreRequest request)
    {
        try
        {
            var entry = await _roomService.AddScoreAsync(id, request.SessionId, GetUserId(), request.VisitNumber, request.Value);
            return Ok(entry);
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(new { Message = ex.Message });
        }
    }

    [HttpPatch("{id}/scores/{entryId}")]
    public async Task<IActionResult> UpdateScore(Guid id, Guid entryId, [FromBody] UpdateScoreRequest request)
    {
        try
        {
            var entry = await _roomService.UpdatePendingScoreAsync(id, entryId, GetUserId(), request.Value);
            return Ok(entry);
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(new { Message = ex.Message });
        }
    }

    [HttpPost("{id}/scores/{entryId}/confirm")]
    public async Task<IActionResult> ConfirmScore(Guid id, Guid entryId, [FromBody] ConfirmScoreRequest request)
    {
        try
        {
            var room = await _roomService.ConfirmScoreAsync(id, entryId, GetUserId(), request.EndTurn);
            return Ok(RoomDto(room));
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(new { Message = ex.Message });
        }
    }

    [HttpPost("{id}/board/request")]
    public async Task<IActionResult> RequestBoard(Guid id, [FromBody] BoardSessionRequest request)
    {
        var room = await _roomService.RequestBoardAsync(id, request.SessionId);
        return Ok(RoomDto(room));
    }

    [HttpPost("{id}/board/grant")]
    public async Task<IActionResult> GrantBoard(Guid id, [FromBody] BoardGrantRequest request)
    {
        try
        {
            var room = await _roomService.GrantBoardAsync(id, request.HolderSessionId, request.TargetSessionId);
            return Ok(RoomDto(room));
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(new { Message = ex.Message });
        }
    }

    [HttpPost("{id}/board/return")]
    public async Task<IActionResult> ReturnBoard(Guid id, [FromBody] BoardSessionRequest request)
    {
        var room = await _roomService.ReturnBoardAsync(id, request.SessionId);
        return Ok(RoomDto(room));
    }

    [HttpPost("{id}/turn/end")]
    public async Task<IActionResult> EndTurn(Guid id, [FromBody] BoardSessionRequest request)
    {
        try
        {
            var room = await _roomService.EndTurnAsync(id, request.SessionId);
            return Ok(RoomDto(room));
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(new { Message = ex.Message });
        }
    }
}

public class CreateRoomRequest
{
    public string? GameType { get; set; }
}

public class JoinRoomRequest
{
    public string Code { get; set; } = string.Empty;
}

public class AddSessionRequest
{
    public string DeviceName { get; set; } = string.Empty;
    public string Roles { get; set; } = "viewer";
}

public class AddScoreRequest
{
    public Guid SessionId { get; set; }
    public int VisitNumber { get; set; }
    public int Value { get; set; }
}

public class UpdateScoreRequest
{
    public int Value { get; set; }
}

public class ConfirmScoreRequest
{
    public bool EndTurn { get; set; } = true;
}

public class BoardSessionRequest
{
    public Guid SessionId { get; set; }
}

public class BoardGrantRequest
{
    public Guid HolderSessionId { get; set; }
    public Guid TargetSessionId { get; set; }
}
