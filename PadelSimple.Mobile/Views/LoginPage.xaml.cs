using PadelSimple.Mobile.ViewModels;
using PadelSimple.Mobile;

namespace PadelSimple.Mobile.Views;

public partial class LoginPage : ContentPage
{
    public LoginPage(LoginVm vm)
    {
        InitializeComponent();
        BindingContext = vm;
    }
}


