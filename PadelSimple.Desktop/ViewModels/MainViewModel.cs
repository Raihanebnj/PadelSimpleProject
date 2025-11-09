using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using PadelSimple.Desktop.Services;
using PadelSimple.Models.Domain;
using System.Collections.ObjectModel;
using System.Threading.Tasks;
using System.Windows;

namespace PadelSimple.Desktop.ViewModels;

public partial class MainViewModel : ObservableObject
{
    private readonly AuthService _auth;
    private readonly DataService _data;

    public bool IsAdmin => _auth.IsInRole("Admin");

    public ObservableCollection<Court> Courts { get; } = new();
    public ObservableCollection<Equipment> Equipment { get; } = new();
    public ObservableCollection<Reservation> Reservations { get; } = new();

    private Court _selectedCourt = new();
    public Court SelectedCourt
    {
        get => _selectedCourt;
        set => SetProperty(ref _selectedCourt, value);
    }

    private Equipment _selectedEquipment = new();
    public Equipment SelectedEquipment
    {
        get => _selectedEquipment;
        set => SetProperty(ref _selectedEquipment, value);
    }

    private Reservation _newReservation = new();
    public Reservation NewReservation
    {
        get => _newReservation;
        set => SetProperty(ref _newReservation, value);
    }

    public MainViewModel(AuthService auth, DataService data)
    {
        _auth = auth;
        _data = data;
        _ = LoadAsync();
    }

    [RelayCommand]
    private async Task LoadAsync()
    {
        try
        {
            Courts.Clear();
            foreach (var c in await _data.GetCourtsAsync()) Courts.Add(c);

            Equipment.Clear();
            foreach (var e in await _data.GetEquipmentAsync()) Equipment.Add(e);

            Reservations.Clear();
            foreach (var r in await _data.GetReservationsAsync(DateTime.Today)) Reservations.Add(r);
        }
        catch (Exception ex)
        {
            MessageBox.Show($"Laden mislukt: {ex.Message}");
        }
    }

    [RelayCommand]
    private async Task SaveCourtAsync()
    {
        await _data.AddOrUpdateCourtAsync(SelectedCourt);
        await LoadAsync();
    }

    [RelayCommand]
    private async Task SaveEquipmentAsync()
    {
        await _data.AddOrUpdateEquipmentAsync(SelectedEquipment);
        await LoadAsync();
    }

    [RelayCommand]
    private async Task DeleteEquipmentAsync()
    {
        if (SelectedEquipment == null || SelectedEquipment.Id == 0)
        {
            MessageBox.Show("Selecteer eerst materiaal.");
            return;
        }

        if (MessageBox.Show($"Verwijder '{SelectedEquipment.Name}'?", "Bevestigen",
            MessageBoxButton.YesNo) == MessageBoxResult.Yes)
        {
            await _data.SoftDeleteEquipmentAsync(SelectedEquipment);
            await LoadAsync();
        }
    }
}
