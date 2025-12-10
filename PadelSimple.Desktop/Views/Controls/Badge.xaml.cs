using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;

namespace PadelSimple.Desktop.Controls;

public partial class Badge : UserControl
{
    public Badge()
    {
        InitializeComponent();
    }

    public static readonly DependencyProperty TextProperty =
        DependencyProperty.Register(nameof(Text), typeof(string), typeof(Badge),
            new PropertyMetadata("Badge"));

    public string Text
    {
        get => (string)GetValue(TextProperty);
        set => SetValue(TextProperty, value);
    }

    public static readonly DependencyProperty BackgroundProperty =
        DependencyProperty.Register(nameof(Background), typeof(Brush), typeof(Badge),
            new PropertyMetadata(Brushes.SteelBlue));

    public new Brush Background
    {
        get => (Brush)GetValue(BackgroundProperty);
        set => SetValue(BackgroundProperty, value);
    }
}
