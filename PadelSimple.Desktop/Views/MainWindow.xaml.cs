using PadelSimple.Desktop.ViewModels;
using System.Windows;
using System.Windows.Controls;
using PadelSimple.Models.Domain;
namespace PadelSimple.Desktop.Views;
using System.Windows.Threading;

public partial class MainWindow : Window
{
    public MainWindow(MainViewModel vm)
    {
        InitializeComponent();
        DataContext = vm;

        Loaded += async (_, __) =>
        {
            if (DataContext is MainViewModel mvm)
                await mvm.LoadDataCommand.ExecuteAsync(null);
        };
    }

    private void Exit_Click(object sender, RoutedEventArgs e)
    {
        Application.Current.Shutdown();
    }

    private bool _commitBusy;

    private void DataGrid_RowEditEnding(object sender, DataGridRowEditEndingEventArgs e)
    {
        if (_commitBusy) return;
        if (sender is not DataGrid grid) return;

        if (e.EditAction != DataGridEditAction.Commit)
            return;

        // Belangrijk: commit pas NA dit event uitvoeren (anders recursion -> StackOverflow)
        _commitBusy = true;

        grid.Dispatcher.BeginInvoke(new Action(() =>
        {
            try
            {
                grid.CommitEdit(DataGridEditingUnit.Row, true);
            }
            finally
            {
                _commitBusy = false;
            }
        }), DispatcherPriority.Background);
    }
}
