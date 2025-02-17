#nullable enable
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;

namespace FlightLib
{
    public class Command : ICommand
    {
        public event EventHandler? CanExecuteChanged;
        Action<object?> ExecuteAction { get; }
        Func<object?, bool> CanExecuteAction { get; }
        public Command(Action<object?> action, Func<object?, bool> canExecute)
        { 
            ExecuteAction = action;
            CanExecuteAction = canExecute ?? new(_ => true);
        }
        public bool CanExecute(object? parameter)
        => CanExecuteAction(parameter);

        public void Execute(object? parameter)
        => ExecuteAction(parameter);
    }
}
