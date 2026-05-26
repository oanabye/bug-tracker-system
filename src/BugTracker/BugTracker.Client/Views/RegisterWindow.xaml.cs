using System.Windows;
using BugTracker.Client.Services;

namespace BugTracker.Client.Views;

public partial class RegisterWindow : Window
{
    private readonly ApiService _api;

    public RegisterWindow(ApiService api)
    {
        InitializeComponent();
        _api = api;
    }

    private async void BtnRegister_Click(object sender, RoutedEventArgs e)
    {
        TxtError.Text = "";

        if (string.IsNullOrWhiteSpace(TxtUsername.Text) ||
            string.IsNullOrWhiteSpace(TxtPassword.Password))
        {
            TxtError.Text = "Please fill in all fields.";
            return;
        }

        var success = await _api.RegisterAsync(TxtUsername.Text, TxtPassword.Password);
        if (success)
        {
            MessageBox.Show("Account created! You can now log in.", "Success");
            this.Close();
        }
        else
        {
            TxtError.Text = "Username already exists. Try another.";
        }
    }

    private void BtnCancel_Click(object sender, RoutedEventArgs e) => this.Close();
}