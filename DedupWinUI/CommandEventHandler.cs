using System;
using System.Windows.Input;

namespace DedupWinUI
{
    internal class CommandEventHandler<T> : ICommand
    {
        public event EventHandler CanExecuteChanged;

        public Action<T> action;
        public bool CanExecute(object parameter)
        {
            return true;
        }

        public void Execute(object parameter)
        {
            this.action((T)parameter);
        }
        public CommandEventHandler(Action<T> action)
        {
            this.action = action;

        }
    }
}
