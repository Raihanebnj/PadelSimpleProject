using PadelSimple.Mobile.ViewModels;

namespace PadelSimple.Mobile.Views;

public partial class ReservationsPage : ContentPage
{
    private readonly ReservationsVm _vm;

    public ReservationsPage(ReservationsVm vm)
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
