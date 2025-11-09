using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;
using PadelSimple.Desktop.Services;

namespace PadelSimple.Desktop.ViewModels;

public class LoginViewModel
{
    private readonly AuthService _auth;
    private readonly IServiceProvider _sp;

    public string Email { get; set; } = string.Empty;
    public string Password { private get; set; } = string.Empty;

    public ICommand LoginCommand { get; }

    public LoginViewModel(AuthService auth, IServiceProvider sp)
    {
        _auth = auth; _sp = sp;
        LoginCommand = new RelayCommand(async _ => await LoginAsync());
    }

    private async Task LoginAsync()
    {
        try
        {
            if (await _auth.LoginAsync(Email, Password))
            {
                var main = (System.Windows.Window)_sp.GetService(typeof(Views.MainWindow))!;
                main.Show();
                Application.Current.Windows.OfType<Views.LoginWindow>().First().Close();
            }
            else
            {
                MessageBox.Show("Aanmelding mislukt", "Login", MessageBoxButton.OK, MessageBoxImage.Warning);
            }
        }
        catch (Exception ex)
        {
            MessageBox.Show($"Fout bij aanmelden: {ex.Message}");
        }
    }
}