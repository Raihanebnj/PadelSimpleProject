using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Windows;
using System.Windows.Input;
using PadelSimple.Desktop.Services;
using PadelSimple.Desktop.Views;
using PadelSimple.Models.Domain;

namespace PadelSimple.Desktop.ViewModels;

public class MainViewModel : INotifyPropertyChanged
{
    private readonly DataService _dataService;
    private readonly AuthService _authService;

    public ObservableCollection<Court> Courts { get; } = new();
    public ObservableCollection<Equipment> Equipment { get; } = new();
    public ObservableCollection<Reservation> Reservations { get; } = new();

    private Reservation? _selectedReservation;
    public Reservation? SelectedReservation
    {
        get => _selectedReservation;
        set { _selectedReservation = value; OnPropertyChanged(); }
    }

    public bool IsAdmin => _authService.IsInRole("Admin");

    public ICommand RefreshCommand { get; }
    public ICommand NewReservationCommand { get; }
    public ICommand DeleteReservationCommand { get; }
    public ICommand LogoutCommand { get; }
    public ICommand ExitCommand { get; }

    public MainViewModel(DataService dataService, AuthService authService)
    {
        _dataService = dataService;
        _authService = authService;

        RefreshCommand = new RelayCommand(async _ => await LoadDataAsync());
        NewReservationCommand = new RelayCommand(_ => OpenNewReservationDialog());
        DeleteReservationCommand = new RelayCommand(async _ => await DeleteSelectedReservationAsync(),
            _ => SelectedReservation != null);
        LogoutCommand = new RelayCommand(_ => Logout());
        ExitCommand = new RelayCommand(_ => Application.Current.Shutdown());
    }

    public async Task LoadDataAsync()
    {
        Courts.Clear();
        foreach (var c in await _dataService.GetCourtsAsync())
            Courts.Add(c);

        Equipment.Clear();
        foreach (var e in await _dataService.GetEquipmentAsync())
            Equipment.Add(e);

        Reservations.Clear();
        foreach (var r in await _dataService.GetReservationsAsync())
            Reservations.Add(r);
    }

    private void OpenNewReservationDialog()
    {
        var dlg = App.GetService<ReservationDialog>();

        dlg.DataContext = new ReservationDialogViewModel(
            _dataService,
            _authService,
            Courts.ToList(),
            Equipment.ToList());

        dlg.Owner = Application.Current.MainWindow;

        if (dlg.ShowDialog() == true)
        {
            _ = LoadDataAsync();
        }
    }

    private async Task DeleteSelectedReservationAsync()
    {
        if (SelectedReservation == null) return;

        if (MessageBox.Show("Ben je zeker dat je deze reservatie wil verwijderen?",
                "Bevestig",
                MessageBoxButton.YesNo,
                MessageBoxImage.Question) != MessageBoxResult.Yes)
            return;

        try
        {
            await _dataService.SoftDeleteReservationAsync(SelectedReservation.Id);
            await LoadDataAsync();
        }
        catch (Exception ex)
        {
            MessageBox.Show(ex.Message, "Fout", MessageBoxButton.OK, MessageBoxImage.Error);
        }
    }

    private void Logout()
    {
        _authService.Logout();

        var login = App.GetService<LoginWindow>();
        Application.Current.MainWindow = login;
        login.Show();

        // huidige main window sluiten
        foreach (Window w in Application.Current.Windows)
        {
            if (w is MainWindow)
            {
                w.Close();
                break;
            }
        }
    }

    public event PropertyChangedEventHandler? PropertyChanged;
    protected void OnPropertyChanged([CallerMemberName] string? name = null) =>
        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
}
