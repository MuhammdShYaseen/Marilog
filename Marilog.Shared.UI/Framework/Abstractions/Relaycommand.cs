using System;
using System.Collections.Generic;
using System.Text;

namespace Marilog.Shared.UI.Framework.Abstractions
{
    /// <summary>
    /// A lightweight ICommand implementation — mirrors WPF RelayCommand / DelegateCommand.
    /// Supports CanExecute logic and raises CanExecuteChanged via ComponentBase.StateHasChanged pattern.
    /// </summary>
    public sealed class RelayCommand : ICommand
    {
        private readonly Action _execute;
        private readonly Func<bool>? _canExecute;

        public RelayCommand(
            Action execute,
            Func<bool>? canExecute = null)
        {
            _execute = execute ?? throw new ArgumentNullException(nameof(execute));
            _canExecute = canExecute;
        }

        public event EventHandler? CanExecuteChanged;

        public bool CanExecute(object? parameter)
            => _canExecute?.Invoke() ?? true;

        public void Execute(object? parameter)
            => _execute();

        /// <summary>
        /// Thread-safe raise.
        /// </summary>
        public void RaiseCanExecuteChanged()
        {
            var handler = CanExecuteChanged;
            handler?.Invoke(this, EventArgs.Empty);
        }
    }

    /// <summary>
    /// Generic RelayCommand with strong typing.
    /// </summary>
    public sealed class RelayCommand<T> : ICommand
    {
        private readonly Action<T?> _execute;
        private readonly Func<T?, bool>? _canExecute;

        public RelayCommand(Action<T?> execute, Func<T?, bool>? canExecute = null)
        {
            _execute = execute ?? throw new ArgumentNullException(nameof(execute));
            _canExecute = canExecute;
        }

        public event EventHandler? CanExecuteChanged;

        public bool CanExecute(object? parameter)
        {
            if (_canExecute == null)
                return true;

            if (parameter is T t)
                return _canExecute(t);

            return _canExecute(default);
        }

        public void Execute(object? parameter)
        {
            if (parameter is T t)
                _execute(t);
            else
                _execute(default);
        }

        public void RaiseCanExecuteChanged()
        {
            var handler = CanExecuteChanged;
            handler?.Invoke(this, EventArgs.Empty);
        }
    }
}
