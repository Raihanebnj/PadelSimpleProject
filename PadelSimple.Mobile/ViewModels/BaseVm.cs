using CommunityToolkit.Mvvm.ComponentModel;

namespace PadelSimple.Mobile.ViewModels;

public partial class BaseVm : ObservableObject
{
    [ObservableProperty] private bool isBusy;
    [ObservableProperty] private string? error;
    [ObservableProperty] private string? info;
}
