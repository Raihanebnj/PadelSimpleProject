using PadelSimple.Mobile.ViewModels;

namespace PadelSimple.Mobile.Views;

public partial class TerreinenPagina : ContentPage
{
    private readonly TerreinenVm _vm;

    public TerreinenPagina(TerreinenVm vm)
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
