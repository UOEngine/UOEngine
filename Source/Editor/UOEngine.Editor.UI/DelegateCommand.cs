// Copyright (c) 2026 UOEngine Project, Scotty1234
// Licensed under the MIT License. See LICENSE file in the project root for details.
using System.Windows.Input;

namespace UOEngine.Editor.UI;

internal class DelegateCommand : ICommand
{
    public event EventHandler? CanExecuteChanged;

    private readonly Action _execute;
    private readonly Func<bool>? _canExecute;

    public bool CanExecute(object? parameter) => _canExecute?.Invoke() ?? true;

    public void Execute(object? parameter) => _execute();

    internal DelegateCommand(Action execute, Func<bool>? canExecute = null)
    {
        _execute = execute;
        _canExecute = canExecute;
    }
}
