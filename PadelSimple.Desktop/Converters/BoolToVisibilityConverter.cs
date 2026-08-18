using System;
using System.Globalization;
using System.Windows;
using System.Windows.Data;

namespace PadelSimple.Desktop.Converters;

/// <summary>Converteert bool naar Visibility (true = Visible, false = Collapsed).</summary>
[ValueConversion(typeof(bool), typeof(Visibility))]
public class BoolToVisibilityConverter : IValueConverter
{
    public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        => value is bool b && b ? Visibility.Visible : Visibility.Collapsed;

    public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        => value is Visibility v && v == Visibility.Visible;
}

/// <summary>Inverteert de bool-naar-Visibility conversie (true = Collapsed, false = Visible).</summary>
[ValueConversion(typeof(bool), typeof(Visibility))]
public class InverseBoolToVisibilityConverter : IValueConverter
{
    public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        => value is bool b && b ? Visibility.Collapsed : Visibility.Visible;

    public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        => value is Visibility v && v == Visibility.Collapsed;
}

/// <summary>Converteert bool naar een Nederlandse statustekst (Actief/Inactief, Lid/Geen lid, enz.).</summary>
[ValueConversion(typeof(bool), typeof(string))]
public class BoolToStatusConverter : IValueConverter
{
    public string TrueText { get; set; } = "Ja";
    public string FalseText { get; set; } = "Nee";

    public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        => value is bool b && b ? TrueText : FalseText;

    public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        => value?.ToString() == TrueText;
}

/// <summary>Converteert null/lege string naar Collapsed (verbergt lege foutboodschappen).</summary>
[ValueConversion(typeof(object), typeof(Visibility))]
public class NullToVisibilityConverter : IValueConverter
{
    public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        => string.IsNullOrWhiteSpace(value?.ToString()) ? Visibility.Collapsed : Visibility.Visible;

    public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        => throw new NotImplementedException();
}
