using PadelSimple.Desktop.ViewModels;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Threading;

namespace PadelSimple.Desktop.Views;

public partial class Hoofdvenster : Window
{
    public Hoofdvenster(HoofdViewModel vm)
    {
        InitializeComponent();
        DataContext = vm;

        Loaded += async (_, _) =>
        {
            if (DataContext is HoofdViewModel hvm)
                await hvm.LaadDataCommand.ExecuteAsync(null);
        };
    }

    private bool _commitBezet;

    /// <summary>
    /// Commit van een rij in de DataGrid wordt uitgesteld om een recursieve StackOverflow te vermijden.
    /// </summary>
    private void DataGrid_RowEditEnding(object sender, DataGridRowEditEndingEventArgs e)
    {
        if (_commitBezet) return;
        if (sender is not DataGrid grid) return;
        if (e.EditAction != DataGridEditAction.Commit) return;

        _commitBezet = true;
        grid.Dispatcher.BeginInvoke(new Action(() =>
        {
            try
            {
                grid.CommitEdit(DataGridEditingUnit.Row, true);
            }
            finally
            {
                _commitBezet = false;
            }
        }), DispatcherPriority.Background);
    }
}
