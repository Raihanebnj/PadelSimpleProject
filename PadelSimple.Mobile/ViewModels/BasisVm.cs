using CommunityToolkit.Mvvm.ComponentModel;

namespace PadelSimple.Mobile.ViewModels;

public partial class BasisVm : ObservableObject
{
    [ObservableProperty] private bool isBusy;
    [ObservableProperty] private string? error;
    [ObservableProperty] private string? info;
}
