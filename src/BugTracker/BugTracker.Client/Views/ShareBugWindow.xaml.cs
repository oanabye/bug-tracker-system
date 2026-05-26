using System.Collections.Generic;
using System.Windows;
using BugTracker.Client.Services;
using Newtonsoft.Json;

namespace BugTracker.Client.Views;

public partial class ShareBugWindow : Window
{
    private readonly ApiService _api;
    private readonly int _bugId;

    public ShareBugWindow(ApiService api, BugDto bug)
    {
        InitializeComponent();
        _api = api;
        _bugId = bug.Id;
        TxtBugTitle.Text = $"Share Bug #{bug.Id}: {bug.Title}";
        LoadDevelopers();
    }

    private async void LoadDevelopers()
    {
        var json = await _api.GetDevelopersAsync();
        var devs = JsonConvert.DeserializeObject<List<DeveloperDto>>(json);
        CmbDevelopers.ItemsSource = devs;
        if (devs?.Count > 0) CmbDevelopers.SelectedIndex = 0;
    }

    private async void BtnShare_Click(object sender, RoutedEventArgs e)
    {
        if (CmbDevelopers.SelectedItem is not DeveloperDto dev)
        {
            MessageBox.Show("Selectează un developer.", "Eroare");
            return;
        }

        if (string.IsNullOrWhiteSpace(TxtMessage.Text))
        {
            MessageBox.Show("Adaugă un mesaj cu instrucțiuni.", "Eroare");
            return;
        }

        var success = await _api.SendShareRequestAsync(_bugId, dev.Id, TxtMessage.Text);
        if (success)
        {
            MessageBox.Show("Bug shared cu succes!", "Success");
            Close();
        }
        else
        {
            MessageBox.Show("A apărut o eroare.", "Eroare");
        }
    }

    private void BtnCancel_Click(object sender, RoutedEventArgs e) => Close();
}