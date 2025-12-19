using PadelSimple.Mobile.ViewModels;

namespace PadelSimple.Mobile.Views;

public partial class EquipmentPage : ContentPage
{
    private readonly EquipmentVm _vm;

    public EquipmentPage(EquipmentVm vm)
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
