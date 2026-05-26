# 🐛 BugTracker

A two-tier client-server bug tracking application built with **WPF** (client) and **ASP.NET Core** (server), using **SQLite** as the database via Entity Framework Core.

---

## Tech Stack

| Layer | Technology |
|-------|-----------|
| Client UI | WPF / XAML (.NET, C#) |
| Server API | ASP.NET Core 8 Web API |
| Database | SQLite via Entity Framework Core |
| Auth | JWT Bearer Tokens |
| API Docs | Swagger (https://localhost:7299/swagger) |

---

## Screenshots

### Login Page
![Login Page](screenshots/login.png)

### Admin Dashboard
![Admin Dashboard](screenshots/admin.png)

### Developer Dashboard
![Developer Dashboard](screenshots/developer.png)

---

## Project Structure

```
BugTracker/
├── BugTracker.Client/
│   ├── Views/           # XAML windows + code-behind
│   ├── ViewModels/      # BugViewModel (wraps BugDto for UI binding)
│   └── Services/
│       └── ApiService.cs  # All HTTP calls to the server
│
└── BugTracker.Server/
    ├── Controllers/     # HTTP endpoints
    ├── Services/        # Business logic
    ├── Repositories/    # Data access
    ├── Models/          # Domain entities
    ├── DTOs/            # Request/response shapes
    └── Data/            # AppDbContext (EF Core)
```

---

## How the Server Works — From Model to Database

### 1. Models

Models are plain C# classes that describe the data structure. Entity Framework reads them and **automatically creates the SQLite tables**.

```csharp
public class Bug
{
    public int Id { get; set; }
    public string Title { get; set; }
    public Severity Severity { get; set; }   // enum: Low, Medium, High, Critical
    public BugStatus Status { get; set; }    // enum: New, InProgress, Fixed, CannotReproduce
    public int ReportedById { get; set; }
    public User ReportedBy { get; set; }
    public int? AssignedToId { get; set; }
    public User? AssignedTo { get; set; }
}
```

No SQL is written manually — EF Core handles schema creation from these classes.

---

### 2. AppDbContext

`AppDbContext` is the bridge between C# code and the SQLite database.

```csharp
public DbSet<Bug> Bugs => Set<Bug>();
public DbSet<User> Users => Set<User>();
public DbSet<BugShareRequest> BugShareRequests => Set<BugShareRequest>();
```

Each `DbSet` corresponds to a table. In `OnModelCreating`, relationships and delete behaviors are configured:

| Relationship | Behavior |
|-------------|----------|
| Bug → ReportedBy | `Restrict` — cannot delete a user who has reported bugs |
| Bug → AssignedTo | `SetNull` — if developer is deleted, bug becomes unassigned |
| BugShareRequest → Bug | `Cascade` — share requests are deleted with the bug |
| BugShareRequest → Developer | `Restrict` — cannot delete developer with active requests |

---

### 3. Repository

The repository layer **hides the data access details** from the rest of the application. Instead of writing `_ctx.Bugs.Include(...).ToListAsync()` everywhere, it's written once and exposed through a clean interface.

```csharp
public interface IBugRepository
{
    Task<List<Bug>> GetAllAsync();
    Task<Bug?> GetByIdAsync(int id);
    Task AddAsync(Bug bug);
    Task SaveChangesAsync();
}
```

The concrete `BugRepository` implements this using `AppDbContext`. The key benefit: if the database changes (e.g. SQLite → PostgreSQL), only the repository changes — nothing else.

---

### 4. Service

The service layer contains the **business logic**. The repository knows how to read/write data; the service knows *what to do* with it.

Example — `UpdateStatusAsync` in `BugService`:
1. Fetch the bug from the repository
2. Check if the requesting user has permission (Admin can update any bug; Developer only their own assigned bugs)
3. Update the status and save

```csharp
public async Task<bool> UpdateStatusAsync(int bugId, BugStatus newStatus, int userId, string role)
{
    var bug = await _bugRepo.GetByIdAsync(bugId);
    if (bug == null) return false;

    if (role == "Administrator")
    {
        bug.Status = newStatus;
        await _bugRepo.SaveChangesAsync();
        return true;
    }

    if (bug.AssignedToId != userId) return false;

    bug.Status = newStatus;
    await _bugRepo.SaveChangesAsync();
    return true;
}
```

The service has no knowledge of HTTP or the database — it only depends on the repository interface.

---

### 5. Controller

The controller is the HTTP entry point. It:
1. Receives the HTTP request
2. Checks authentication/authorization (`[Authorize]`)
3. Calls the service
4. Returns the HTTP response

```csharp
[HttpPut("{id}/status")]
[Authorize]
public async Task<IActionResult> UpdateStatus(int id, UpdateBugStatusDto dto)
{
    var userId = int.Parse(User.FindFirst("userId")!.Value);
    var role = User.FindFirst(ClaimTypes.Role)!.Value;
    var success = await _bugService.UpdateStatusAsync(id, dto.Status, userId, role);
    return success ? NoContent() : NotFound();
}
```

---

### Full Request Flow

When the WPF client clicks **"Update Status"**, the full chain is:

```
ApiService.cs  (WPF client)
    └─► PUT /api/bugs/{id}/status
            └─► BugsController.UpdateStatus()
                    └─► BugService.UpdateStatusAsync()
                            └─► BugRepository.GetByIdAsync()
                                    └─► AppDbContext ──► SQLite (SELECT)
                            ◄── returns Bug
                        applies business rules (role check)
                            └─► BugRepository.SaveChangesAsync()
                                    └─► AppDbContext ──► SQLite (UPDATE)
                    ◄── returns true
            ◄── 204 No Content
    ◄── success
```

Each layer only knows about the layer directly below it:
- **Controller** knows Service
- **Service** knows Repository
- **Repository** knows DbContext
- **DbContext** knows SQLite

No layer skips another.

---

## API Endpoints

### Auth — `/api/auth`
| Method | Route | Description |
|--------|-------|-------------|
| POST | `/login` | Authenticate, returns JWT + role + userId |
| POST | `/register` | Register new developer account |

### Bugs — `/api/bugs`
| Method | Route | Description | Auth |
|--------|-------|-------------|------|
| GET | `/bugs` | All bugs | ✅ |
| GET | `/bugs/my-bugs` | Bugs assigned to current user | ✅ |
| GET | `/bugs/{id}/photo` | Bug photo as bytes | ✅ |
| GET | `/bugs/developers` | List all developers | ✅ |
| POST | `/bugs` | Submit new bug (multipart with optional photo) | ✅ |
| POST | `/bugs/import-xml` | Import bugs from XML | ✅ |
| PUT | `/bugs/{id}/status` | Update bug status | ✅ |
| PUT | `/bugs/{id}/assign` | Assign bug to developer | ✅ |

### Admin — `/api/admin` *(Administrator only)*
| Method | Route | Description |
|--------|-------|-------------|
| GET | `/admin/developers` | List developers |
| POST | `/admin/developers` | Add developer |
| DELETE | `/admin/developers/{id}` | Remove developer |
| PUT | `/admin/developers/{id}/password` | Change password |
| PUT | `/admin/bugs/{id}` | Edit bug details |

### Share Requests — `/api/sharerequest`
| Method | Route | Description |
|--------|-------|-------------|
| POST | `/sharerequest` | Send share request |
| GET | `/sharerequest/pending` | Get pending requests |
| PUT | `/sharerequest/{id}/respond` | Accept or reject |

---

## XML Import Format

```xml
<Bugs>
  <Bug>
    <Title>Login button unresponsive</Title>
    <Description>Button does not respond on iOS Safari</Description>
    <Severity>High</Severity>
    <StepsToReproduce>1. Open Safari  2. Go to login  3. Tap button</StepsToReproduce>
  </Bug>
</Bugs>
```

Severity values: `Low`, `Medium`, `High`, `Critical`

---

## Setup & Running

### Prerequisites
- .NET 8 SDK or later
- Visual Studio 2022 (recommended)
- No database setup needed — SQLite file is created automatically

### Run the Server
Set `BugTracker.Server` as the startup project and run with the **https** profile.

```
Server:  https://localhost:7299
Swagger: https://localhost:7299/swagger
```

### Run the Client
Set `BugTracker.Client` as the startup project and run. It connects to `https://localhost:7299/api` (configured in `ApiService.cs`).

---

## Features

### Admin Dashboard
- View all bugs as expandable cards with severity/status color coding
- Import bugs from XML
- Assign bugs to developers
- Edit bug details
- Manage developer accounts (add, delete, change password)

### Developer Dashboard
- Switch between **My Bugs** and **All Bugs** tabs
- Submit new bugs with optional photo attachment
- Update status of assigned bugs
- Share bugs with other developers
- View and respond to share request notifications
