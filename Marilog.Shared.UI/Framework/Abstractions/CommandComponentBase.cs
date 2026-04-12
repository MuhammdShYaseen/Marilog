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
    }
}