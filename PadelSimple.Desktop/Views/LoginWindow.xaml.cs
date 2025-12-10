using System.Windows;
using PadelSimple.Desktop.ViewModels;

namespace PadelSimple.Desktop.Views;

public partial class LoginWindow : Window
{
    public LoginWindow(LoginViewModel vm)
    {
        InitializeComponent();
        DataContext = vm;

        vm.LoginSucceeded += OnLoginSucceeded;
    }

    private void OnLoginSucceeded()
    {
        var main = App.GetService<MainWindow>();
        Application.Current.MainWindow = main;
        main.Show();
        this.Close();
    }
}
