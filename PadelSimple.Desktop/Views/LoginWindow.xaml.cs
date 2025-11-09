using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;
using PadelSimple.Desktop.ViewModels;

namespace PadelSimple.Desktop.Views;

public partial class LoginWindow : Window
{
    public LoginViewModel VM { get; }
    public LoginWindow(LoginViewModel vm)
    {
        InitializeComponent();
        VM = vm; DataContext = VM;
    }

    private void Button_Click(object sender, RoutedEventArgs e)
    {
        VM.Password = Pwd.Password;
        VM.LoginCommand.Execute(null);
    }
}