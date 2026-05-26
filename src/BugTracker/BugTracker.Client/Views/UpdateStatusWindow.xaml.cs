using System.Windows;
using System.Windows.Controls;
using BugTracker.Client.Services;

namespace BugTracker.Client.Views;

public partial class UpdateStatusWindow : Window
{
    private readonly ApiService _api;
    private readonly int _bugId;

    public UpdateStatusWindow(ApiService api, int bugId, string currentStatus)
    {
        InitializeComponent();
        _api = api;
        _bugId = bugId;
        TxtBugTitle.Text = $"Bug #{bugId} — Current status: {currentStatus}";

        // Pre-select current status
        foreach (ComboBoxItem item in CmbStatus.Items)
        {
            if (item.Content.ToString() == currentStatus)
            {
                item.IsSelected = true;
                break;
            }
        }
    }

    private async void BtnUpdate_Click(object sender, RoutedEventArgs e)
    {
        var status = (CmbStatus.SelectedItem as ComboBoxItem)?.Content?.ToString();
        if (status == null) return;

        var success = await _api.UpdateBugStatusAsync(_bugId, status);

        if (success)
        {
            MessageBox.Show("Status updated!", "Success");
            this.Close();
        }
        else
        {
            MessageBox.Show(
                "Nu poți modifica statusul acestui bug.\nPoți modifica doar bug-urile asignate ție de către admin.",
                "Acces interzis",
                MessageBoxButton.OK,
                MessageBoxImage.Warning);
        }
    }

    private void BtnCancel_Click(object sender, RoutedEventArgs e) => this.Close();
}