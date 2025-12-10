using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using PadelSimple.Desktop.Services;

namespace PadelSimple.Desktop.ViewModels;

public class LoginViewModel : INotifyPropertyChanged
{
    private readonly AuthService _authService;

    private string _username = "";
    public string Username
    {
        get => _username;
        set { _username = value; OnPropertyChanged(); }
    }

    public ICommand LoginCommand { get; }

    public event Action? LoginSucceeded;

    public LoginViewModel(AuthService authService)
    {
        _authService = authService;

        LoginCommand = new RelayCommand(async param =>
        {
            try
            {
                if (param is not PasswordBox pb)
                {
                    MessageBox.Show("Onverwachte fout met passwordbox.");
                    return;
                }

                var ok = await _authService.LoginAsync(Username, pb.Password);

                if (!ok)
                {
                    MessageBox.Show("Login mislukt. Controleer je gegevens.");
                    return;
                }

                LoginSucceeded?.Invoke();
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message, "Fout", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        });
    }

    public event PropertyChangedEventHandler? PropertyChanged;
    protected void OnPropertyChanged([CallerMemberName] string? name = null) =>
        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
}
