using System.Windows;
using BugTracker.Client.Services;
using BugTracker.Client.Views;

namespace BugTracker.Client;

public partial class App : Application
{
    protected override void OnStartup(StartupEventArgs e)
    {
        base.OnStartup(e);
        var api = new ApiService();
        var login = new LoginWindow(api);
        login.Show();
    }
}