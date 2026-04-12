    namespace Marilog.Shared.UI.Framework.Abstractions
    {
        /// <summary>
        /// Base class for all components that support Command execution.
        /// Mirrors WPF ButtonBase behavior.
        /// </summary>
        public abstract class CommandComponentBase : FrameworkElement
        {
            

            // =========================
            // COMMAND PARAMETERS
            // =========================

            [Parameter]
            public string? CommandKey { get; set; }

            [Parameter]
            public object? CommandParameter { get; set; }

            // =========================
            // OPTIONAL DIRECT COMMAND
            // =========================

            [Parameter]
            public ICommand? Command { get; set; }

            // =========================
            // EXECUTION STATE CACHE
            // =========================

            private bool _canExecuteCache = true;

            // =========================
            // LIFECYCLE
            // =========================

            protected override void OnParametersSet()
            {
                SubscribeCommand();
                UpdateCanExecute();

                base.OnParametersSet();
            }

            private void SubscribeCommand()
            {
                if (Command != null)
                {
                    Command.CanExecuteChanged -= OnCanExecuteChanged;
                    Command.CanExecuteChanged += OnCanExecuteChanged;
                }
            }

            private void OnCanExecuteChanged(
                object? sender,
                EventArgs e)
            {
                UpdateCanExecute();

                InvokeAsync(StateHasChanged);
            }

            private void UpdateCanExecute()
            {
                if (Command != null)
                {
                    _canExecuteCache =
                        Command.CanExecute(CommandParameter);
                }
                else if (!string.IsNullOrWhiteSpace(CommandKey))
                {
                    _canExecuteCache =
                        CommandContext?.CanExecute(
                            CommandKey,
                            CommandParameter) ?? true;
                }
                else
                {
                    _canExecuteCache = true;
                }
            }

            // =========================
            // EXECUTION
            // =========================

            protected bool CanExecute()
            {
                return IsEnabled && _canExecuteCache;
            }

            protected void ExecuteCommand()
            {
                if (!CanExecute())
                    return;

                // Direct command
                if (Command != null)
                {
                    if (Command.CanExecute(CommandParameter))
                        Command.Execute(CommandParameter);

                    return;
                }

                // CommandKey via context
                if (!string.IsNullOrWhiteSpace(CommandKey))
                {
                    CommandContext?.TryExecute(
                        CommandKey,
                        CommandParameter);
                }
            }

            // =========================
            // DISPOSE
            // =========================

            protected override void Dispose(bool disposing)
            {
                if (disposing)
                {
                    if (Command != null)
                        Command.CanExecuteChanged -= OnCanExecuteChanged;
                }

                base.Dispose(disposing);
            }
        }
    }
