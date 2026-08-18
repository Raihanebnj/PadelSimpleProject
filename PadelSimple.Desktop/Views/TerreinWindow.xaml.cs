using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Windows;
using PadelSimple.Models.Domain;

namespace PadelSimple.Desktop.Views;

/// <summary>
/// Code-behind voor het TerreinWindow (toevoegen/bewerken van een terrein).
/// Implementeert INotifyPropertyChanged zodat de StatusBadge reageert op wijzigingen.
/// </summary>
public partial class TerreinWindow : Window, INotifyPropertyChanged
{
    public event PropertyChangedEventHandler? PropertyChanged;

    private string _naam = string.Empty;
    private int _capaciteit = 4;
    private decimal _uurtarief = 15m;
    private bool _isIndoors = false;
    private string _foutBoodschap = string.Empty;

    public string TitelTekst => Terrein?.Id == 0 ? "Nieuw Terrein" : "Terrein Bewerken";

    public string Naam
    {
        get => _naam;
        set { _naam = value; OnPropertyChanged(); }
    }

    public int Capaciteit
    {
        get => _capaciteit;
        set { _capaciteit = value; OnPropertyChanged(); }
    }

    public decimal Uurtarief
    {
        get => _uurtarief;
        set { _uurtarief = value; OnPropertyChanged(); }
    }

    public bool IsIndoors
    {
        get => _isIndoors;
        set { _isIndoors = value; OnPropertyChanged(); }
    }

    public string FoutBoodschap
    {
        get => _foutBoodschap;
        set { _foutBoodschap = value; OnPropertyChanged(); }
    }

    public Terrein? Terrein { get; private set; }

    public TerreinWindow(Terrein? terrein = null)
    {
        InitializeComponent();
        DataContext = this;

        if (terrein != null)
        {
            Terrein = terrein;
            Naam = terrein.Naam;
            Capaciteit = terrein.Capaciteit;
            Uurtarief = terrein.Uurtarief;
            IsIndoors = terrein.IsIndoors;
        }
        else
        {
            Terrein = new Terrein();
        }
    }

    private void Opslaan_Click(object sender, RoutedEventArgs e)
    {
        FoutBoodschap = string.Empty;

        if (string.IsNullOrWhiteSpace(Naam))
        {
            FoutBoodschap = "Naam is verplicht.";
            return;
        }
        if (Capaciteit < 2 || Capaciteit > 10)
        {
            FoutBoodschap = "Capaciteit moet tussen 2 en 10 liggen.";
            return;
        }
        if (Uurtarief <= 0)
        {
            FoutBoodschap = "Uurtarief moet groter zijn dan 0.";
            return;
        }

        Terrein!.Naam = Naam;
        Terrein.Capaciteit = Capaciteit;
        Terrein.Uurtarief = Uurtarief;
        Terrein.IsIndoors = IsIndoors;

        DialogResult = true;
    }

    private void Cancel_Click(object sender, RoutedEventArgs e)
    {
        DialogResult = false;
    }

    protected void OnPropertyChanged([CallerMemberName] string? name = null)
        => PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
}
