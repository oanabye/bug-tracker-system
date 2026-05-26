namespace BugTracker.Server.Models;

public enum Severity { Low, Medium, High, Critical }
public enum BugStatus { New, InProgress, Fixed, CannotReproduce }

public class Bug
{
    public int Id { get; set; }
    public string Title { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public Severity Severity { get; set; }
    public BugStatus Status { get; set; } = BugStatus.New;
    public string StepsToReproduce { get; set; } = string.Empty;
    public byte[]? Photo { get; set; }
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    public int ReportedById { get; set; }
    public User ReportedBy { get; set; } = null!;

    public int? AssignedToId { get; set; }
    public User? AssignedTo { get; set; }
}