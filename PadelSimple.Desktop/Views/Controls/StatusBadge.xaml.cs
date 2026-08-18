using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;

namespace PadelSimple.Desktop.Controls;

/// <summary>
/// Custom UserControl die een gekleurde statusbadge toont.
/// Gebruik: Text = "Overdekt", IsActive = true/false
/// </summary>
public partial class StatusBadge : UserControl
{
    public static readonly DependencyProperty TextProperty =
        DependencyProperty.Register(nameof(Text), typeof(string), typeof(StatusBadge),
            new PropertyMetadata("Status"));

    public static readonly DependencyProperty IsActiveProperty =
        DependencyProperty.Register(nameof(IsActive), typeof(bool), typeof(StatusBadge),
            new PropertyMetadata(true, OnIsActiveChanged));

    public static readonly DependencyProperty BadgeBackgroundProperty =
        DependencyProperty.Register(nameof(BadgeBackground), typeof(Brush), typeof(StatusBadge),
            new PropertyMetadata(new SolidColorBrush(Color.FromRgb(0x2E, 0x7D, 0x32))));

    public string Text
    {
        get => (string)GetValue(TextProperty);
        set => SetValue(TextProperty, value);
    }

    public bool IsActive
    {
        get => (bool)GetValue(IsActiveProperty);
        set => SetValue(IsActiveProperty, value);
    }

    public Brush BadgeBackground
    {
        get => (Brush)GetValue(BadgeBackgroundProperty);
        set => SetValue(BadgeBackgroundProperty, value);
    }

    public StatusBadge()
    {
        InitializeComponent();
    }

    private static void OnIsActiveChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
    {
        if (d is StatusBadge badge)
        {
            badge.BadgeBackground = (bool)e.NewValue
                ? new SolidColorBrush(Color.FromRgb(0x2E, 0x7D, 0x32))  // groen
                : new SolidColorBrush(Color.FromRgb(0x78, 0x90, 0x9C));  // grijs
        }
    }
}
