using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Windows;
using System.Windows.Input;
using PadelSimple.Desktop.Services;
using PadelSimple.Models.Domain;

namespace PadelSimple.Desktop.ViewModels;

public class ReservationDialogViewModel : INotifyPropertyChanged
{
    private readonly DataService _dataService;
    private readonly AuthService _authService;

    public IList<Court> Courts { get; }
    public IList<Equipment> Equipment { get; }

    public Court? SelectedCourt { get; set; }
    public Equipment? SelectedEquipment { get; set; }

    public DateTime Date { get; set; } = DateTime.Today;
    public string StartTimeString { get; set; } = "18:00";
    public string EndTimeString { get; set; } = "19:00";
    public int NumberOfPlayers { get; set; } = 4;
    public int EquipmentQuantity { get; set; } = 0;

    public ICommand SaveCommand { get; }

    public ReservationDialogViewModel(
        DataService dataService,
        AuthService authService,
        IList<Court> courts,
        IList<Equipment> equipment)
    {
        _dataService = dataService;
        _authService = authService;
        Courts = courts;
        Equipment = equipment;

        SaveCommand = new RelayCommand(async param =>
        {
            try
            {
                if (SelectedCourt == null)
                {
                    MessageBox.Show("Selecteer een terrein.");
                    return;
                }

                if (!TimeSpan.TryParse(StartTimeString, out var start) ||
                    !TimeSpan.TryParse(EndTimeString, out var end))
                {
                    MessageBox.Show("Ongeldige tijd.");
                    return;
                }

                var user = _authService.CurrentUser;
                if (user == null)
                {
                    MessageBox.Show("Geen gebruiker ingelogd.");
                    return;
                }

                var res = new Reservation
                {
                    CourtId = SelectedCourt.Id,
                    Date = Date.Date,
                    StartTime = start,
                    EndTime = end,
                    NumberOfPlayers = NumberOfPlayers,
                    UserId = user.Id,
                    EquipmentId = SelectedEquipment?.Id,
                    EquipmentQuantity = SelectedEquipment != null && EquipmentQuantity > 0
                        ? EquipmentQuantity
                        : null
                };

                await _dataService.CreateReservationAsync(res);

                if (param is Window win)
                    win.DialogResult = true;
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message, "Fout", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        });
    }

    public event PropertyChangedEventHandler? PropertyChanged;
    protected void OnPropertyChanged([CallerMemberName] string? name = null) =>
        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
}
