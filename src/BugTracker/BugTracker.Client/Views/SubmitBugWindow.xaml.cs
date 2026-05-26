using System.IO;
using System.Windows;
using BugTracker.Client.Services;
using Microsoft.Win32;

namespace BugTracker.Client.Views;

public partial class SubmitBugWindow : Window
{
    private readonly ApiService _api;
    private string? _photoPath;

    public SubmitBugWindow(ApiService api)
    {
        InitializeComponent();
        _api = api;
    }

    private void BtnAttachPhoto_Click(object sender, RoutedEventArgs e)
    {
        var dialog = new OpenFileDialog
        {
            Filter = "Image Files (*.png;*.jpg;*.jpeg)|*.png;*.jpg;*.jpeg",
            Title = "Select a Photo"
        };

        if (dialog.ShowDialog() != true) return;
        _photoPath = dialog.FileName;
        TxtPhotoName.Text = Path.GetFileName(_photoPath);
    }

    private async void BtnSubmit_Click(object sender, RoutedEventArgs e)
    {
        TxtError.Text = "";

        if (string.IsNullOrWhiteSpace(TxtTitle.Text) ||
            string.IsNullOrWhiteSpace(TxtDescription.Text) ||
            string.IsNullOrWhiteSpace(TxtSteps.Text))
        {
            TxtError.Text = "Please fill in all required fields.";
            return;
        }

        var severity = (CmbSeverity.SelectedItem as System.Windows.Controls.ComboBoxItem)?.Content?.ToString() ?? "Low";

        var success = await _api.SubmitBugAsync(
            TxtTitle.Text,
            TxtDescription.Text,
            severity,
            TxtSteps.Text,
            _photoPath);

        if (success)
        {
            MessageBox.Show("Bug submitted successfully!", "Success");
            this.Close();
        }
        else
        {
            TxtError.Text = "Failed to submit bug. Please try again.";
        }
    }

    private void BtnCancel_Click(object sender, RoutedEventArgs e) => this.Close();
}