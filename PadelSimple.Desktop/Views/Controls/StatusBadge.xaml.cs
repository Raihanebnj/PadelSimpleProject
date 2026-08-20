using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;

namespace PadelSimple.Desktop.Controls;

/// <summary>
/// Custom UserControl die een gekleurde statusbadge toont.
/// Gebruik: Text = "Overdekt", IsActief = true/false
/// </summary>
public partial class StatusBadge : UserControl
{
    public static readonly DependencyProperty TextProperty =
        DependencyProperty.Register(nameof(Text), typeof(string), typeof(StatusBadge),
            new PropertyMetadata("Status"));

    public static readonly DependencyProperty IsActiefProperty =
        DependencyProperty.Register(nameof(IsActief), typeof(bool), typeof(StatusBadge),
            new PropertyMetadata(true, OnIsActiefGewijzigd));

    public static readonly DependencyProperty BadgeAchtergrondProperty =
        DependencyProperty.Register(nameof(BadgeAchtergrond), typeof(Brush), typeof(StatusBadge),
            new PropertyMetadata(new SolidColorBrush(Color.FromRgb(0x2E, 0x7D, 0x32))));

    public string Text
    {
        get => (string)GetValue(TextProperty);
        set => SetValue(TextProperty, value);
    }

    public bool IsActief
    {
        get => (bool)GetValue(IsActiefProperty);
        set => SetValue(IsActiefProperty, value);
    }

    public Brush BadgeAchtergrond
    {
        get => (Brush)GetValue(BadgeAchtergrondProperty);
        set => SetValue(BadgeAchtergrondProperty, value);
    }

    public StatusBadge()
    {
        InitializeComponent();
    }

    private static void OnIsActiefGewijzigd(DependencyObject d, DependencyPropertyChangedEventArgs e)
    {
        if (d is StatusBadge badge)
        {
            badge.BadgeAchtergrond = (bool)e.NewValue
                ? new SolidColorBrush(Color.FromRgb(0x2E, 0x7D, 0x32))  // groen
                : new SolidColorBrush(Color.FromRgb(0x78, 0x90, 0x9C));  // grijs
        }
    }
}
