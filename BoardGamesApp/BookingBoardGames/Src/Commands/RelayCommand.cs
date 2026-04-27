using System;
using System.Windows.Input;

namespace BookingBoardGames.Src.Commands
{
    public class RelayCommand : ICommand
    {
        private readonly Action<Object?> executeAction;
        private readonly Func<bool> canExecuteFunction;

        public RelayCommand(Action<Object?> executeAction, Func<bool> canExecuteFunction = null)
        {
            this.executeAction = executeAction;
            this.canExecuteFunction = canExecuteFunction;
        }

        public event EventHandler CanExecuteChanged;

        public bool CanExecute(object parameter) => canExecuteFunction?.Invoke() ?? true;
        public void Execute(object parameter) => executeAction(parameter);

        public void NotifyCanExecuteChanged() =>
            CanExecuteChanged?.Invoke(this, EventArgs.Empty);
    }
}
