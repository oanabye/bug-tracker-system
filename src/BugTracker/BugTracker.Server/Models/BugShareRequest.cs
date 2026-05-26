namespace BugTracker.Server.Models;

public enum ShareRequestStatus { Pending, Accepted, Declined }

public class BugShareRequest
{
    public int Id { get; set; }
    public int BugId { get; set; }
    public Bug Bug { get; set; } = null!;
    public int FromDeveloperId { get; set; }
    public User FromDeveloper { get; set; } = null!;
    public int ToDeveloperId { get; set; }
    public User ToDeveloper { get; set; } = null!;
    public string Message { get; set; } = string.Empty;
    public ShareRequestStatus Status { get; set; } = ShareRequestStatus.Pending;
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
}