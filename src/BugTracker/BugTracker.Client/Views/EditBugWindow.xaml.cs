using System.Windows;
using System.Windows.Controls;
using BugTracker.Client.Services;

namespace BugTracker.Client.Views;

public partial class EditBugWindow : Window
{
    private readonly ApiService _api;
    private readonly int _bugId;

    public EditBugWindow(ApiService api, BugDto bug)
    {
        InitializeComponent();
        _api = api;
        _bugId = bug.Id;

        TxtTitle.Text = bug.Title;
        TxtDescription.Text = bug.Description;
        TxtSteps.Text = bug.StepsToReproduce;

        foreach (ComboBoxItem item in CmbSeverity.Items)
            if (item.Content.ToString() == bug.Severity)
            { item.IsSelected = true; break; }
    }

    private async void BtnSave_Click(object sender, RoutedEventArgs e)
    {
        if (string.IsNullOrWhiteSpace(TxtTitle.Text))
        {
            MessageBox.Show("Title is required.", "Validation");
            return;
        }

        var severity = (CmbSeverity.SelectedItem as ComboBoxItem)?.Content?.ToString() ?? "Low";
        var severityMap = new Dictionary<string, int>
        {
            { "Low", 0 }, { "Medium", 1 }, { "High", 2 }, { "Critical", 3 }
        };

        var success = await _api.EditBugAsync(_bugId, TxtTitle.Text,
            TxtDescription.Text, severityMap[severity], TxtSteps.Text);

        if (success) { MessageBox.Show("Bug updated!"); Close(); }
        else MessageBox.Show("Failed to update bug.", "Error");
    }

    private void BtnCancel_Click(object sender, RoutedEventArgs e) => Close();
}