using System.Windows;
using BugTracker.Client.Services;

namespace BugTracker.Client.Views;

public partial class LoginWindow : Window
{
    private readonly ApiService _api;

    public LoginWindow(ApiService api)
    {
        InitializeComponent();
        _api = api;
    }

    private async void BtnLogin_Click(object sender, RoutedEventArgs e)
    {
        TxtError.Text = "";
        var username = TxtUsername.Text.Trim();
        var password = TxtPassword.Password;

        if (string.IsNullOrEmpty(username) || string.IsNullOrEmpty(password))
        {
            TxtError.Text = "Please enter username and password.";
            return;
        }

        var success = await _api.LoginAsync(username, password);

        if (!success)
        {
            TxtError.Text = "Invalid username or password.";
            return;
        }

        // Route to correct window based on role
        Window next = _api.Role == "Administrator"
            ? new AdminWindow(_api)
            : new DeveloperWindow(_api);

        next.Show();
        this.Close();
    }
    private void BtnRegister_Click(object sender, RoutedEventArgs e)
    {
        var window = new RegisterWindow(_api);
        window.Owner = this;
        window.ShowDialog();
    }
}