using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using Newtonsoft.Json;

namespace BugTracker.Client.Services;

public class ApiService
{
    private readonly HttpClient _http = new();
    private const string Base = "https://localhost:7299/api";

    public string? Token { get; private set; }
    public string? Role { get; private set; }
    public int UserId { get; private set; }

    public async Task<bool> LoginAsync(string username, string password)
    {
        var body = JsonConvert.SerializeObject(new { username, password });
        var response = await _http.PostAsync($"{Base}/auth/login",
            new StringContent(body, Encoding.UTF8, "application/json"));

        if (!response.IsSuccessStatusCode) return false;

        var json = await response.Content.ReadAsStringAsync();
        dynamic result = JsonConvert.DeserializeObject(json)!;
        Token = result.token;
        Role = result.role;
        UserId = result.userId;

        _http.DefaultRequestHeaders.Authorization =
            new AuthenticationHeaderValue("Bearer", Token);
        return true;
    }

    public async Task<string> GetBugsAsync()
    {
        var response = await _http.GetAsync($"{Base}/bugs");
        return await response.Content.ReadAsStringAsync();
    }

    public async Task<bool> ImportXmlAsync(string filePath)
    {
        using var form = new MultipartFormDataContent();
        var fileBytes = System.IO.File.ReadAllBytes(filePath);
        var fileContent = new ByteArrayContent(fileBytes);
        fileContent.Headers.ContentType = new MediaTypeHeaderValue("application/xml");
        form.Add(fileContent, "file", System.IO.Path.GetFileName(filePath));

        var response = await _http.PostAsync($"{Base}/bugs/import-xml", form);
        return response.IsSuccessStatusCode;
    }
    public async Task<bool> SubmitBugAsync(string title, string description,
    string severity, string steps, string? photoPath)
    {
        using var form = new MultipartFormDataContent();
        form.Add(new StringContent(title), "Title");
        form.Add(new StringContent(description), "Description");
        form.Add(new StringContent(severity), "Severity");
        form.Add(new StringContent(steps), "StepsToReproduce");

        if (photoPath != null)
        {
            var photoBytes = System.IO.File.ReadAllBytes(photoPath);
            var photoContent = new ByteArrayContent(photoBytes);
            photoContent.Headers.ContentType = new MediaTypeHeaderValue("image/jpeg");
            form.Add(photoContent, "Photo", System.IO.Path.GetFileName(photoPath));
        }

        var response = await _http.PostAsync($"{Base}/bugs", form);
        return response.IsSuccessStatusCode;
    }

    public async Task<bool> UpdateBugStatusAsync(int bugId, string status)
    {
        // Map string to int enum value
        var statusMap = new Dictionary<string, int>
    {
        { "New", 0 }, { "InProgress", 1 }, { "Fixed", 2 }, { "CannotReproduce", 3 }
    };

        var body = JsonConvert.SerializeObject(new { status = statusMap[status] });
        var response = await _http.PutAsync($"{Base}/bugs/{bugId}/status",
            new StringContent(body, Encoding.UTF8, "application/json"));
        return response.IsSuccessStatusCode;
    }

    public async Task<bool> RegisterAsync(string username, string password)
    {
        var body = JsonConvert.SerializeObject(new { username, password });
        var response = await _http.PostAsync($"{Base}/auth/register",
            new StringContent(body, Encoding.UTF8, "application/json"));
        return response.IsSuccessStatusCode;
    }

    public async Task<string> GetDevelopersAsync()
    {
        var response = await _http.GetAsync($"{Base}/bugs/developers");
        return await response.Content.ReadAsStringAsync();
    }

    public async Task<bool> AssignBugAsync(int bugId, int developerId)
    {
        var body = JsonConvert.SerializeObject(new { developerId });
        var response = await _http.PutAsync($"{Base}/bugs/{bugId}/assign",
            new StringContent(body, Encoding.UTF8, "application/json"));
        return response.IsSuccessStatusCode;
    }
    public async Task<string> GetMyBugsAsync()
    {
        var response = await _http.GetAsync($"{Base}/bugs/my-bugs");
        return await response.Content.ReadAsStringAsync();
    }
    public async Task<byte[]?> GetBugPhotoAsync(int bugId)
    {
        var response = await _http.GetAsync($"{Base}/bugs/{bugId}/photo");
        if (!response.IsSuccessStatusCode) return null;
        return await response.Content.ReadAsByteArrayAsync();
    }
    public async Task<string> GetAdminDevelopersAsync()
    {
        var response = await _http.GetAsync($"{Base}/admin/developers");
        return await response.Content.ReadAsStringAsync();
    }

    public async Task<bool> AdminAddDeveloperAsync(string username, string password)
    {
        var body = JsonConvert.SerializeObject(new { username, password });
        var response = await _http.PostAsync($"{Base}/admin/developers",
            new StringContent(body, Encoding.UTF8, "application/json"));
        return response.IsSuccessStatusCode;
    }

    public async Task<bool> AdminDeleteDeveloperAsync(int id)
    {
        var response = await _http.DeleteAsync($"{Base}/admin/developers/{id}");
        return response.IsSuccessStatusCode;
    }

    public async Task<bool> AdminUpdatePasswordAsync(int id, string newPassword)
    {
        var body = JsonConvert.SerializeObject(new { newPassword });
        var response = await _http.PutAsync($"{Base}/admin/developers/{id}/password",
            new StringContent(body, Encoding.UTF8, "application/json"));
        return response.IsSuccessStatusCode;
    }

    public async Task<bool> EditBugAsync(int bugId, string title,
        string description, int severity, string steps)
    {
        var body = JsonConvert.SerializeObject(new { title, description, severity, stepsToReproduce = steps });
        var response = await _http.PutAsync($"{Base}/admin/bugs/{bugId}",
            new StringContent(body, Encoding.UTF8, "application/json"));
        return response.IsSuccessStatusCode;
    }
    public async Task<bool> SendShareRequestAsync(int bugId, int toDeveloperId, string message)
    {
        var body = JsonConvert.SerializeObject(new { bugId, toDeveloperId, message });
        var response = await _http.PostAsync($"{Base}/sharerequest",
            new StringContent(body, Encoding.UTF8, "application/json"));
        return response.IsSuccessStatusCode;
    }

    public async Task<string> GetPendingShareRequestsAsync()
    {
        var response = await _http.GetAsync($"{Base}/sharerequest/pending");
        return await response.Content.ReadAsStringAsync();
    }

    public async Task<bool> RespondToShareRequestAsync(int requestId, bool accept)
    {
        var body = JsonConvert.SerializeObject(accept);
        var response = await _http.PutAsync($"{Base}/sharerequest/{requestId}/respond",
            new StringContent(body, Encoding.UTF8, "application/json"));
        return response.IsSuccessStatusCode;
    }
}