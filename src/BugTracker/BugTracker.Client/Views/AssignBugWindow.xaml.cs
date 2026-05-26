using System.Collections.Generic;
using System.Windows;
using BugTracker.Client.Services;
using Newtonsoft.Json;

namespace BugTracker.Client.Views;

public partial class AssignBugWindow : Window
{
    private readonly ApiService _api;
    private readonly int _bugId;

    public AssignBugWindow(ApiService api, int bugId, string bugTitle)
    {
        InitializeComponent();
        _api = api;
        _bugId = bugId;
        TxtBugTitle.Text = $"Bug #{bugId}: {bugTitle}";
        LoadDevelopers();
    }

    private async void LoadDevelopers()
    {
        var json = await _api.GetDevelopersAsync();
        var devs = JsonConvert.DeserializeObject<List<DeveloperDto>>(json);
        CmbDevelopers.ItemsSource = devs;
        if (devs?.Count > 0) CmbDevelopers.SelectedIndex = 0;
    }

    private async void BtnAssign_Click(object sender, RoutedEventArgs e)
    {
        if (CmbDevelopers.SelectedItem is not DeveloperDto dev) return;

        var success = await _api.AssignBugAsync(_bugId, dev.Id);
        if (success)
        {
            MessageBox.Show($"Bug assigned to {dev.Username}!", "Success");
            this.Close();
        }
        else
        {
            MessageBox.Show("Failed to assign bug.", "Error");
        }
    }

    private void BtnCancel_Click(object sender, RoutedEventArgs e) => this.Close();
}

public class DeveloperDto
{
    public int Id { get; set; }
    public string Username { get; set; } = "";
}