namespace BugTracker.Server.DTOs;

public record LoginRequest(string Username, string Password);
public record LoginResponse(string Token, string Role, int UserId);

public record RegisterRequest(string Username, string Password);