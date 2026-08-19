using PadelSimple.Mobile.ViewModels;

namespace PadelSimple.Mobile.Views;

public partial class NewReservationPage : ContentPage
{
    private readonly NewReservationVm _vm;

    public NewReservationPage(NewReservationVm vm)
    {
        InitializeComponent();
        BindingContext = _vm = vm;
    }

    protected override async void OnAppearing()
    {
        base.OnAppearing();
        await _vm.InitAsync();
    }
}
