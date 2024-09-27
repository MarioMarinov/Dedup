using System;
using System.Windows.Input;

namespace DedupWinUI
{
    internal class CommandEventHandler<T>(Action<T> action) : ICommand
    {
#pragma warning disable CS0067 // Avoid "method is never used"
        public event EventHandler CanExecuteChanged;
#pragma warning restore CS0067

        public Action<T> action = action;
        public bool CanExecute(object parameter)
        {
            return true;
        }

        public void Execute(object parameter)
        {
            this.action((T)parameter);
        }
    }
}
