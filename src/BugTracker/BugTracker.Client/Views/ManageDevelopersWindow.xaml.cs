using System.Collections.Generic;
using System.Windows;
using BugTracker.Client.Services;
using Newtonsoft.Json;

namespace BugTracker.Client.Views;

public partial class ManageDevelopersWindow : Window
{
    private readonly ApiService _api;

    public ManageDevelopersWindow(ApiService api)
    {
        InitializeComponent();
        _api = api;
        LoadDevelopers();
    }

    private async void LoadDevelopers()
    {
        var json = await _api.GetAdminDevelopersAsync();
        var devs = JsonConvert.DeserializeObject<List<DeveloperDto>>(json);
        DevsGrid.ItemsSource = devs;
    }

    private async void BtnAdd_Click(object sender, RoutedEventArgs e)
    {
        TxtAddError.Text = "";
        if (string.IsNullOrWhiteSpace(TxtNewUsername.Text) ||
            string.IsNullOrWhiteSpace(TxtNewPassword.Password))
        {
            TxtAddError.Text = "Please enter username and password.";
            return;
        }

        var success = await _api.AdminAddDeveloperAsync(
            TxtNewUsername.Text, TxtNewPassword.Password);

        if (success)
        {
            TxtNewUsername.Text = "";
            TxtNewPassword.Password = "";
            LoadDevelopers();
        }
        else
        {
            TxtAddError.Text = "Username already exists.";
        }
    }

    private async void BtnDelete_Click(object sender, RoutedEventArgs e)
    {
        if (DevsGrid.SelectedItem is not DeveloperDto selected)
        {
            MessageBox.Show("Please select a developer first.", "No selection");
            return;
        }

        var confirm = MessageBox.Show(
            $"Delete developer '{selected.Username}'?",
            "Confirm Delete", MessageBoxButton.YesNo, MessageBoxImage.Warning);

        if (confirm != MessageBoxResult.Yes) return;

        var success = await _api.AdminDeleteDeveloperAsync(selected.Id);
        if (success) LoadDevelopers();
        else MessageBox.Show("Failed to delete developer.", "Error");
    }

    private async void BtnUpdatePassword_Click(object sender, RoutedEventArgs e)
    {
        try
        {
            if (DevsGrid.SelectedItem is not DeveloperDto selected)
            {
                MessageBox.Show("Please select a developer first.", "No selection");
                return;
            }

            var dialog = new UpdatePasswordDialog(selected.Username);
            dialog.Owner = this;
            if (dialog.ShowDialog() != true) return;

            var success = await _api.AdminUpdatePasswordAsync(selected.Id, dialog.NewPassword);
            MessageBox.Show(success ? "Password updated!" : "Failed to update password.");
        }
        catch (Exception ex)
        {
            MessageBox.Show($"Error: {ex.Message}\n\n{ex.StackTrace}", "Exception");
        }
    }
}