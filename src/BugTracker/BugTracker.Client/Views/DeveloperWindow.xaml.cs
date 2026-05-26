using System.Collections.Generic;
using System.Windows;
using System.Windows.Media;
using BugTracker.Client.Services;
using Newtonsoft.Json;

namespace BugTracker.Client.Views;

public partial class DeveloperWindow : Window
{
    private readonly ApiService _api;
    private bool _showingMyBugs = true;

    public DeveloperWindow(ApiService api)
    {
        InitializeComponent();
        _api = api;
        try { LoadMyBugs(); }
        catch (Exception ex) { MessageBox.Show($"Error: {ex.Message}", "Startup Error"); }
    }

    private async void LoadMyBugs()
    {
        try
        {
            _showingMyBugs = true;
            SetActiveTab(BtnTabMyBugs, BtnTabAllBugs);
            TxtStatus.Text = "Loading...";

            var json = await _api.GetMyBugsAsync();
            var bugs = JsonConvert.DeserializeObject<List<BugDto>>(json);
            BugsListBox.ItemsSource = bugs?.Select(b => new BugViewModel(b)).ToList();
            TxtStatus.Text = $"{bugs?.Count ?? 0} bugs assigned to me";
        }
        catch (Exception ex) { MessageBox.Show($"Load error: {ex.Message}", "Error"); }
    }

    private async void LoadAllBugs()
    {
        try
        {
            _showingMyBugs = false;
            SetActiveTab(BtnTabAllBugs, BtnTabMyBugs);
            TxtStatus.Text = "Loading...";

            var json = await _api.GetBugsAsync();
            var bugs = JsonConvert.DeserializeObject<List<BugDto>>(json);
            BugsListBox.ItemsSource = bugs?.Select(b => new BugViewModel(b)).ToList();
            TxtStatus.Text = $"{bugs?.Count ?? 0} total bugs";
        }
        catch (Exception ex) { MessageBox.Show($"Load error: {ex.Message}", "Error"); }
    }

    private void BugsListBox_SelectionChanged(object sender, System.Windows.Controls.SelectionChangedEventArgs e)
    {
        if (BugsListBox.SelectedItem is BugViewModel vm)
            vm.IsExpanded = !vm.IsExpanded;
    }

    private void SetActiveTab(System.Windows.Controls.Button active,
                               System.Windows.Controls.Button inactive)
    {
        active.Background = new SolidColorBrush(Color.FromRgb(124, 58, 237));
        active.Foreground = Brushes.White;
        inactive.Background = new SolidColorBrush(Color.FromRgb(46, 46, 62));
        inactive.Foreground = new SolidColorBrush(Color.FromRgb(170, 170, 170));
    }

    private void BtnRefresh_Click(object sender, RoutedEventArgs e)
    {
        if (_showingMyBugs) LoadMyBugs();
        else LoadAllBugs();
    }

    private void BtnTabMyBugs_Click(object sender, RoutedEventArgs e) => LoadMyBugs();
    private void BtnTabAllBugs_Click(object sender, RoutedEventArgs e) => LoadAllBugs();

    private void BtnNewBug_Click(object sender, RoutedEventArgs e)
    {
        var window = new SubmitBugWindow(_api);
        window.Owner = this;
        window.ShowDialog();
        BtnRefresh_Click(sender, e);
    }

    private void BtnUpdateStatus_Click(object sender, RoutedEventArgs e)
    {
        if (BugsListBox.SelectedItem is not BugViewModel selected)
        {
            MessageBox.Show("Please select a bug first.", "No selection");
            return;
        }
        var window = new UpdateStatusWindow(_api, selected.Id, selected.Status);
        window.Owner = this;
        window.ShowDialog();
        BtnRefresh_Click(sender, e);
    }

    private void BtnShareBug_Click(object sender, RoutedEventArgs e)
    {
        if (BugsListBox.SelectedItem is not BugViewModel selected)
        {
            MessageBox.Show("Selectează un bug mai întâi.", "No selection");
            return;
        }
        var window = new ShareBugWindow(_api, selected.ToDto());
        window.Owner = this;
        window.ShowDialog();
    }

    private void BtnNotifications_Click(object sender, RoutedEventArgs e)
    {
        var window = new NotificationsWindow(_api);
        window.Owner = this;
        window.ShowDialog();
        LoadMyBugs();
    }

    private void BtnLogout_Click(object sender, RoutedEventArgs e)
    {
        var login = new LoginWindow(new ApiService());
        login.Show();
        this.Close();
    }
}