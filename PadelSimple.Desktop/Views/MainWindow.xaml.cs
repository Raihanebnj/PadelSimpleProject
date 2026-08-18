using PadelSimple.Desktop.ViewModels;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Threading;

namespace PadelSimple.Desktop.Views;

public partial class MainWindow : Window
{
    public MainWindow(MainViewModel vm)
    {
        InitializeComponent();
        DataContext = vm;

        Loaded += async (_, _) =>
        {
            if (DataContext is MainViewModel mvm)
                await mvm.LoadDataCommand.ExecuteAsync(null);
        };
    }

    private bool _commitBusy;

    /// <summary>
    /// Commit van een rij in de DataGrid wordt uitgesteld om een recursieve StackOverflow te vermijden.
    /// </summary>
    private void DataGrid_RowEditEnding(object sender, DataGridRowEditEndingEventArgs e)
    {
        if (_commitBusy) return;
        if (sender is not DataGrid grid) return;
        if (e.EditAction != DataGridEditAction.Commit) return;

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
