using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;

namespace PadelSimple.Desktop.ViewModels;

public class RelayCommand : ICommand
{
    private readonly Func<object?, Task>? _executeAsync;
    private readonly Action<object?>? _executeSync;
    private readonly Predicate<object?>? _canExecute;

    public RelayCommand(Func<object?, Task> execute, Predicate<object?>? canExecute = null)
    { _executeAsync = execute; _canExecute = canExecute; }

    public RelayCommand(Action<object?> execute, Predicate<object?>? canExecute = null)
    { _executeSync = execute; _canExecute = canExecute; }

    public bool CanExecute(object? parameter) => _canExecute?.Invoke(parameter) ?? true;
    public async void Execute(object? parameter)
    { if (_executeAsync != null) await _executeAsync(parameter); else _executeSync?.Invoke(parameter); }
    public event EventHandler? CanExecuteChanged
    { add { CommandManager.RequerySuggested += value; } remove { CommandManager.RequerySuggested -= value; } }
}