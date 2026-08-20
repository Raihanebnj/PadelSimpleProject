using System;
using System.Windows.Input;

namespace PadelSimple.Desktop.ViewModels;

public class RelaisCommando : ICommand
{
    private readonly Action<object?> _uitvoeren;
    private readonly Predicate<object?>? _kanUitvoeren;

    public RelaisCommando(Action<object?> uitvoeren, Predicate<object?>? kanUitvoeren = null)
    {
        _uitvoeren = uitvoeren;
        _kanUitvoeren = kanUitvoeren;
    }

    public bool CanExecute(object? parameter) => _kanUitvoeren?.Invoke(parameter) ?? true;

    public void Execute(object? parameter) => _uitvoeren(parameter);

    public event EventHandler? CanExecuteChanged;

    public void ActiveerCanExecuteChanged() =>
        CanExecuteChanged?.Invoke(this, EventArgs.Empty);
}
