using BugTracker.Client.Services;
using System.IO;
using System.Windows;
using System.Windows.Media;
using System.Windows.Media.Imaging;
namespace BugTracker.Client.Views;

public partial class BugDetailWindow : Window
{
    private readonly ApiService _api;
    private readonly BugDto _bug;

    public BugDetailWindow(ApiService api, BugDto bug)
    {
        InitializeComponent();
        _api = api;
        _bug = bug;
        PopulateFields();
        if (bug.HasPhoto) LoadPhoto();
        else TxtNoPhoto.Visibility = Visibility.Visible;
    }

    private void PopulateFields()
    {
        TxtTitle.Text = $"Bug #{_bug.Id}: {_bug.Title}";
        TxtSeverity.Text = _bug.Severity;
        TxtStatus.Text = _bug.Status;
        TxtReportedBy.Text = _bug.ReportedBy;

        TxtAssignedTo.Text =
            string.IsNullOrEmpty(_bug.AssignedTo)
                ? "Unassigned"
                : _bug.AssignedTo;

        TxtCreatedAt.Text =
            _bug.CreatedAt.ToString("dd MMM yyyy • HH:mm");

        TxtDescription.Text = _bug.Description;
        TxtSteps.Text = _bug.StepsToReproduce;

        ApplySeverityStyle();
        ApplyStatusStyle();
    }

    private async void LoadPhoto()
    {
        try
        {
            var photoBytes = await _api.GetBugPhotoAsync(_bug.Id);
            if (photoBytes == null || photoBytes.Length == 0)
            {
                TxtNoPhoto.Visibility = Visibility.Visible;
                return;
            }

            var image = new BitmapImage();
            using var ms = new MemoryStream(photoBytes);
            image.BeginInit();
            image.CacheOption = BitmapCacheOption.OnLoad;
            image.StreamSource = ms;
            image.EndInit();

            BugPhoto.Source = image;
            PhotoBorder.Visibility = Visibility.Visible;
            TxtNoPhoto.Visibility = Visibility.Collapsed;
        }
        catch (Exception ex)
        {
            TxtNoPhoto.Text = $"Could not load photo: {ex.Message}";
            TxtNoPhoto.Visibility = Visibility.Visible;
        }
    }
    private void ApplySeverityStyle()
    {
        switch (_bug.Severity)
        {
            case "Critical":
                SeverityBadge.Background =
                    new SolidColorBrush(Color.FromRgb(127, 29, 29));
                break;

            case "High":
                SeverityBadge.Background =
                    new SolidColorBrush(Color.FromRgb(120, 53, 15));
                break;

            case "Medium":
                SeverityBadge.Background =
                    new SolidColorBrush(Color.FromRgb(30, 58, 95));
                break;

            case "Low":
                SeverityBadge.Background =
                    new SolidColorBrush(Color.FromRgb(6, 78, 59));
                break;
        }
    }
     private void ApplyStatusStyle()
    {
        switch (_bug.Status)
        {
            case "Open":
                StatusBadge.Background =
                    new SolidColorBrush(Color.FromRgb(49, 46, 129));
                break;

            case "In Progress":
                StatusBadge.Background =
                    new SolidColorBrush(Color.FromRgb(12, 74, 110));
                break;

            case "Resolved":
                StatusBadge.Background =
                    new SolidColorBrush(Color.FromRgb(6, 78, 59));
                break;
        }
    }
}