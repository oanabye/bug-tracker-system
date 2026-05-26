namespace BugTracker.Server.Models;

public enum UserRole { Developer, Administrator }

public class User
{
    public int Id { get; set; }
    public string Username { get; set; } = string.Empty;
    public string PasswordHash { get; set; } = string.Empty;
    public UserRole Role { get; set; }

    public ICollection<Bug> ReportedBugs { get; set; } = new List<Bug>();
    public ICollection<Bug> AssignedBugs { get; set; } = new List<Bug>();
}