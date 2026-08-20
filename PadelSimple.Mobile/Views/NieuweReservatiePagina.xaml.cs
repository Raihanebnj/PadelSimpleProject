using PadelSimple.Mobile.ViewModels;

namespace PadelSimple.Mobile.Views;

public partial class NieuweReservatiePagina : ContentPage
{
    private readonly NieuweReservatieVm _vm;

    public NieuweReservatiePagina(NieuweReservatieVm vm)
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
