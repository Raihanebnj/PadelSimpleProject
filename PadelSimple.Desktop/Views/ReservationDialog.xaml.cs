using PadelSimple.Desktop.ViewModels;
using System.Windows;

namespace PadelSimple.Desktop.Views;

public partial class ReservationDialog : Window
{
    public ReservationDialog(ReservationDialogViewModel vm)
    {
        InitializeComponent();
        DataContext = vm;
    }
}
