using System.Windows;

namespace BugTracker.Client.Views;

public partial class UpdatePasswordDialog : Window
{
    public string NewPassword { get; private set; } = "";

    public UpdatePasswordDialog(string username)
    {
        InitializeComponent();
        TxtTitle.Text = $"Update password for '{username}'";
    }

    private void BtnUpdate_Click(object sender, RoutedEventArgs e)
    {
        if (string.IsNullOrWhiteSpace(TxtPassword.Password))
        {
            MessageBox.Show("Please enter a new password.");
            return;
        }
        NewPassword = TxtPassword.Password;
        DialogResult = true;
        Close();
    }

    private void BtnCancel_Click(object sender, RoutedEventArgs e) => Close();
}