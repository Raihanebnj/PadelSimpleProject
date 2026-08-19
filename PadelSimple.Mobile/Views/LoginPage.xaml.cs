using PadelSimple.Mobile.ViewModels;
using PadelSimple.Mobile;

namespace PadelSimple.Mobile.Views;

public partial class LoginPage : ContentPage
{
    private readonly LoginVm _vm;

    public LoginPage(LoginVm vm)
    {
        InitializeComponent();
        BindingContext = vm;
        _vm = vm;
    }

    protected override async void OnAppearing()
    {
        base.OnAppearing();
        await _vm.RestoreCommand.ExecuteAsync(null);
    }
}


