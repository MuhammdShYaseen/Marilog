namespace Marilog.Shared.UI.Framework.Abstractions
{
    public sealed class CommandContext
    {
        private readonly Dictionary<string, ICommand>
            _commands = new();

        // =========================
        // REGISTER
        // =========================

        public void Register(
            string key,
            ICommand command)
        {
            if (string.IsNullOrWhiteSpace(key))
                throw new ArgumentException(
                    "Command key cannot be null or empty.",
                    nameof(key));

            _commands[key] = command;
        }

        // =========================
        // UNREGISTER
        // =========================

        public void Unregister(string key)
        {
            if (string.IsNullOrWhiteSpace(key))
                return;

            _commands.Remove(key);
        }

        // =========================
        // CONTAINS
        // =========================

        public bool Contains(string key)
        {
            return _commands.ContainsKey(key);
        }

        // =========================
        // TRY EXECUTE
        // =========================

        public bool TryExecute(
            string key,
            object? parameter = null)
        {
            if (string.IsNullOrWhiteSpace(key))
                return false;

            if (_commands.TryGetValue(
                    key,
                    out var command))
            {
                if (command.CanExecute(parameter))
                {
                    command.Execute(parameter);
                    return true;
                }
            }

            return false;
        }

        // =========================
        // CAN EXECUTE CHECK
        // =========================

        public bool CanExecute(
            string key,
            object? parameter = null)
        {
            if (string.IsNullOrWhiteSpace(key))
                return false;

            if (_commands.TryGetValue(
                    key,
                    out var command))
            {
                return command.CanExecute(parameter);
            }

            return false;
        }
    }
}
