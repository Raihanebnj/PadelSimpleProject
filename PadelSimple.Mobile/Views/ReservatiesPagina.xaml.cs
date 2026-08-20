using PadelSimple.Mobile.ViewModels;

namespace PadelSimple.Mobile.Views;

public partial class ReservatiesPagina : ContentPage
{
    private readonly ReservatiesVm _vm;

    public ReservatiesPagina(ReservatiesVm vm)
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
