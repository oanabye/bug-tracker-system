using System.Collections.Generic;
using System.Windows;
using BugTracker.Client.Services;
using Newtonsoft.Json;

namespace BugTracker.Client.Views;

public partial class NotificationsWindow : Window
{
    private readonly ApiService _api;

    public NotificationsWindow(ApiService api)
    {
        InitializeComponent();
        _api = api;
        LoadRequests();
    }

    private async void LoadRequests()
    {
        var json = await _api.GetPendingShareRequestsAsync();
        var requests = JsonConvert.DeserializeObject<List<ShareRequestDto>>(json);
        RequestsGrid.ItemsSource = requests;
    }

    private async void BtnAccept_Click(object sender, RoutedEventArgs e)
        => await Respond(true);

    private async void BtnDecline_Click(object sender, RoutedEventArgs e)
        => await Respond(false);

    private async Task Respond(bool accept)
    {
        if (RequestsGrid.SelectedItem is not ShareRequestDto selected)
        {
            MessageBox.Show("Selectează un request.", "Eroare");
            return;
        }

        var success = await _api.RespondToShareRequestAsync(selected.Id, accept);
        if (success)
        {
            MessageBox.Show(accept ? "Request acceptat! Bug-ul a fost asignat ție."
                                   : "Request refuzat.", "Info");
            LoadRequests();
        }
    }
}

public class ShareRequestDto
{
    public int Id { get; set; }
    public int BugId { get; set; }
    public string BugTitle { get; set; } = "";
    public string FromDeveloper { get; set; } = "";
    public string Message { get; set; } = "";
    public string Status { get; set; } = "";
    public DateTime CreatedAt { get; set; }
}