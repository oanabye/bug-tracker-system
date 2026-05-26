using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using BugTracker.Client.Services;
using Microsoft.Win32;
using Newtonsoft.Json;

namespace BugTracker.Client.Views
{
    public partial class AdminWindow : Window
    {
        private readonly ApiService _api;
        private List<BugViewModel> _bugs = new();

        public AdminWindow(ApiService api)
        {
            InitializeComponent();
            _api = api;
            LoadBugs();
        }

        private async void LoadBugs()
        {
            TxtStatus.Text = "Loading...";
            var json = await _api.GetBugsAsync();
            var dtos = JsonConvert.DeserializeObject<List<BugDto>>(json) ?? new();
            _bugs = dtos.Select(d => new BugViewModel(d)).ToList();
            BugsListBox.ItemsSource = _bugs;
            TxtStatus.Text = $"{_bugs.Count} bugs loaded";
        }

        // Called from XAML: SelectionChanged="BugsListBox_SelectionChanged"
        private void BugsListBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (BugsListBox.SelectedItem is not BugViewModel selected) return;
            foreach (var b in _bugs.Where(b => b != selected))
                b.IsExpanded = false;
            selected.IsExpanded = !selected.IsExpanded;
            BugsListBox.SelectedItem = null;
        }

        private void BtnRefresh_Click(object sender, RoutedEventArgs e) => LoadBugs();

        private async void BtnImportXml_Click(object sender, RoutedEventArgs e)
        {
            var dialog = new OpenFileDialog { Filter = "XML Files (*.xml)|*.xml", Title = "Select Bug Report XML" };
            if (dialog.ShowDialog() != true) return;
            TxtStatus.Text = "Importing...";
            var success = await _api.ImportXmlAsync(dialog.FileName);
            TxtStatus.Text = success ? "Import successful!" : "Import failed.";
            LoadBugs();
        }

        private void BtnLogout_Click(object sender, RoutedEventArgs e)
        {
            new LoginWindow(new ApiService()).Show();
            Close();
        }

        private void BtnAssignBug_Click(object sender, RoutedEventArgs e)
        {
            var expanded = _bugs.FirstOrDefault(b => b.IsExpanded);
            if (expanded == null) { MessageBox.Show("Please tap a bug card first.", "No selection"); return; }
            var win = new AssignBugWindow(_api, expanded.Id, expanded.Title);
            win.Owner = this;
            win.ShowDialog();
            LoadBugs();
        }

        private void BtnManageDevs_Click(object sender, RoutedEventArgs e)
        {
            var win = new ManageDevelopersWindow(_api);
            win.Owner = this;
            win.ShowDialog();
        }

        private void BtnEditBug_Click(object sender, RoutedEventArgs e)
        {
            var expanded = _bugs.FirstOrDefault(b => b.IsExpanded);
            if (expanded == null) { MessageBox.Show("Please tap a bug card first.", "No selection"); return; }
            var win = new EditBugWindow(_api, expanded.ToDto());
            win.Owner = this;
            win.ShowDialog();
            LoadBugs();
        }
    }

    public class BugViewModel : INotifyPropertyChanged
    {
        private readonly BugDto _dto;
        private bool _isExpanded;

        public BugViewModel(BugDto dto) => _dto = dto;

        public int      Id               => _dto.Id;
        public string   Title            => _dto.Title;
        public string   Description      => _dto.Description;
        public string   Severity         => _dto.Severity;
        public string   Status           => _dto.Status;
        public string   ReportedBy       => _dto.ReportedBy;
        public string?  AssignedTo       => _dto.AssignedTo;
        public DateTime CreatedAt        => _dto.CreatedAt;
        public string   StepsToReproduce => _dto.StepsToReproduce;

        public string   IdLabel           => $"#{_dto.Id}";
        public string   AssignedToDisplay => _dto.AssignedTo ?? "— unassigned";

        public Visibility AssignedVisibility =>
            string.IsNullOrEmpty(_dto.AssignedTo) ? Visibility.Collapsed : Visibility.Visible;
        public Visibility PhotoVisibility =>
            _dto.HasPhoto ? Visibility.Visible : Visibility.Collapsed;

        public string SeverityEmoji => _dto.Severity switch
        {
            "Critical" => "🔴",
            "High"     => "🟠",
            "Medium"   => "🔵",
            "Low"      => "🟢",
            _          => "⚪"
        };

        public Brush SeverityBg => _dto.Severity switch
        {
            "Critical" => new SolidColorBrush(Color.FromRgb(0x7f, 0x1d, 0x1d)),
            "High"     => new SolidColorBrush(Color.FromRgb(0x78, 0x35, 0x0f)),
            "Medium"   => new SolidColorBrush(Color.FromRgb(0x1e, 0x3a, 0x5f)),
            "Low"      => new SolidColorBrush(Color.FromRgb(0x06, 0x4e, 0x3b)),
            _          => new SolidColorBrush(Color.FromRgb(0x33, 0x41, 0x55))
        };
        public Brush SeverityFg => _dto.Severity switch
        {
            "Critical" => new SolidColorBrush(Color.FromRgb(0xfc, 0xa5, 0xa5)),
            "High"     => new SolidColorBrush(Color.FromRgb(0xfc, 0xd3, 0x4d)),
            "Medium"   => new SolidColorBrush(Color.FromRgb(0x93, 0xc5, 0xfd)),
            "Low"      => new SolidColorBrush(Color.FromRgb(0x6e, 0xe7, 0xb7)),
            _          => new SolidColorBrush(Color.FromRgb(0x94, 0xa3, 0xb8))
        };
        public Brush StatusBg => _dto.Status switch
        {
            "Open"        => new SolidColorBrush(Color.FromRgb(0x31, 0x2e, 0x81)),
            "In Progress" => new SolidColorBrush(Color.FromRgb(0x0c, 0x4a, 0x6e)),
            "Resolved"    => new SolidColorBrush(Color.FromRgb(0x06, 0x4e, 0x3b)),
            _             => new SolidColorBrush(Color.FromRgb(0x1e, 0x29, 0x3b))
        };
        public Brush StatusFg => _dto.Status switch
        {
            "Open"        => new SolidColorBrush(Color.FromRgb(0xa5, 0xb4, 0xfc)),
            "In Progress" => new SolidColorBrush(Color.FromRgb(0x7d, 0xd3, 0xfc)),
            "Resolved"    => new SolidColorBrush(Color.FromRgb(0x6e, 0xe7, 0xb7)),
            _             => new SolidColorBrush(Color.FromRgb(0x94, 0xa3, 0xb8))
        };

        public bool IsExpanded
        {
            get => _isExpanded;
            set { _isExpanded = value; OnPropertyChanged(); }
        }

        public BugDto ToDto() => _dto;

        public event PropertyChangedEventHandler? PropertyChanged;
        private void OnPropertyChanged([CallerMemberName] string? n = null)
            => PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(n));
    }

    public class BugDto
    {
        public int      Id               { get; set; }
        public string   Title            { get; set; } = "";
        public string   Description      { get; set; } = "";
        public string   Severity         { get; set; } = "";
        public string   Status           { get; set; } = "";
        public string   StepsToReproduce { get; set; } = "";
        public bool     HasPhoto         { get; set; }
        public DateTime CreatedAt        { get; set; }
        public string   ReportedBy       { get; set; } = "";
        public string?  AssignedTo       { get; set; }
    }
}
