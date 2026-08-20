using PadelSimple.Mobile.ViewModels;

namespace PadelSimple.Mobile.Views;

public partial class MateriaalPagina : ContentPage
{
    private readonly MateriaalVm _vm;

    public MateriaalPagina(MateriaalVm vm)
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
