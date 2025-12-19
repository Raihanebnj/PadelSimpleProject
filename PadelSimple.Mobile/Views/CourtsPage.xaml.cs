using PadelSimple.Mobile.ViewModels;

namespace PadelSimple.Mobile.Views;

public partial class CourtsPage : ContentPage
{
    private readonly CourtsVm _vm;

    public CourtsPage(CourtsVm vm)
    {
        InitializeComponent();
        BindingContext = _vm = vm;
    }

    protected override async void OnAppearing()
    {
        base.OnAppearing();
        await _vm.LoadAsync();
    }
}
