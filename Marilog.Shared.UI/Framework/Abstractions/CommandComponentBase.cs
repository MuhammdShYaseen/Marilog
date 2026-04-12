using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Web;

namespace Marilog.Shared.UI.Framework.Abstractions
{
    /// <summary>
    /// Base class for components that support ICommand execution.
    /// Mirrors WPF ICommandSource behavior.
    /// </summary>
    public abstract class CommandElementBase : FrameworkElement
    {
        //--Private Parm------
        private bool _canExecuteCache = true;
        private ICommand? _currentCommand;
        private object? _currentCommandParameter;

        // =========================
        // COMMAND PARAMETERS
        // =========================

        [Parameter]
        public string? CommandKey { get; set; }

        [Parameter]
        public object? CommandParameter { get; set; }

        // Optional direct command
        [Parameter]
        public ICommand? Command { get; set; }
        [Parameter]
        public EventCallback<MouseEventArgs> Click { get; set; }

        // =========================
        // EXECUTION
        // =========================

        protected bool TryExecuteCommand()
        {
            if (!IsEnabled)
                return false;

            // Direct ICommand (like WPF Button.Command)

            if (Command is not null)
            {
                if (Command.CanExecute(CommandParameter))
                {
                    Command.Execute(CommandParameter);
                    return true;
                }

                return false;
            }

            // CommandContext lookup (CommandKey system)

            if (!string.IsNullOrWhiteSpace(CommandKey))
            {
                return CommandContext?
                    .TryExecute(
                        CommandKey,
                        CommandParameter)
                    == true;
            }

            return false;
        }

        // =========================
        // SUBSCRIBE
        // =========================

        private void Subscribe(ICommand? command)
        {
            if (command is IRaiseCanExecuteChanged raise)
            {
                raise.CanExecuteChanged += OnCanExecuteChanged;
            }
        }

        private void Unsubscribe(ICommand? command)
        {
            if (command is IRaiseCanExecuteChanged raise)
            {
                raise.CanExecuteChanged -= OnCanExecuteChanged;
            }
        }

        private void OnCanExecuteChanged(object? sender, EventArgs e)
        {
            InvokeAsync(UpdateCanExecute);
        }
        // =========================
        // STATE
        // =========================

        private void UpdateCanExecute()
        {
            var newValue =
                Command?.CanExecute(CommandParameter) ?? true;

            if (_canExecuteCache != newValue)
            {
                _canExecuteCache = newValue;
                InvokeAsync(StateHasChanged);
            }
        }

        protected bool IsDisabled()
            => !IsEnabled ||
               (Command != null && !_canExecuteCache);

        // =========================
        // CLICK
        // =========================

        protected async Task HandleClick(MouseEventArgs args)
        {
            if (IsDisabled())
                return;

            if (Command?.CanExecute(CommandParameter) == true)
            {
                Command.Execute(CommandParameter);

                UpdateCanExecute();
            }

            if (Click.HasDelegate)
                await Click.InvokeAsync(args);
        }

        // =========================
        // CLICK HELPER
        // =========================

        protected async Task HandleCommandClick(
            MouseEventArgs args,
            EventCallback<MouseEventArgs> click)
        {
            TryExecuteCommand();

            if (click.HasDelegate)
                await click.InvokeAsync(args);
        }

        protected override void OnParametersSet()
        {
            if (_currentCommand != Command)
            {
                Unsubscribe(_currentCommand);
                Subscribe(Command);
                _currentCommand = Command;

                UpdateCanExecute();
            }

            if (!Equals(_currentCommandParameter, CommandParameter))
            {
                _currentCommandParameter = CommandParameter;
                UpdateCanExecute();
            }

            base.OnParametersSet();
        }

        // =========================
        // DISPOSE
        // =========================

        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                Unsubscribe(_currentCommand);
            }

            base.Dispose(disposing);
        }
    }
}