using PadelSimple.Desktop.ViewModels;
using System.Windows;

namespace PadelSimple.Desktop.Views;

public partial class ReservatieDialoog : Window
{
    public ReservatieDialoog(ReservatieDialoogViewModel vm)
    {
        InitializeComponent();
        DataContext = vm;
    }
}
