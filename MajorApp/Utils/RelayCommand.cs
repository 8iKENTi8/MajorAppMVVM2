using System;
using System.Windows.Input;

namespace MajorAppMVVM2.Utils
{
    // Класс RelayCommand для команд без параметров
    public class RelayCommand : ICommand
    {
        private readonly Func<bool> canExecute; // Делегат, возвращающий логическое значение, указывающее, можно ли выполнить команду
        private readonly Action execute; // Делегат, представляющий действие, которое будет выполнено командой

        // Конструктор принимает два параметра: действие и условие выполнения команды
        public RelayCommand(Action execute, Func<bool> canExecute = null)
        {
            this.execute = execute; // Сохраняем действие, которое будет выполнено командой
            this.canExecute = canExecute; // Сохраняем условие выполнения команды, если оно указано
        }

        // Событие, которое вызывается, когда изменяется состояние выполнения команды
        public event EventHandler CanExecuteChanged
        {
            add { CommandManager.RequerySuggested += value; }
            remove { CommandManager.RequerySuggested -= value; }
        }

        // Метод CanExecute определяет, может ли команда выполняться
        public bool CanExecute(object parameter)
        {
            return canExecute == null || canExecute();
        }

        // Метод Execute выполняет действие, связанное с командой
        public void Execute(object parameter)
        {
            execute();
        }

        // Метод RaiseCanExecuteChanged вызывает событие CanExecuteChanged для обновления состояния выполнения команды
        public void RaiseCanExecuteChanged()
        {
            CommandManager.InvalidateRequerySuggested();
        }
    }

    // Обобщенная версия RelayCommand для команд с параметрами
    public class RelayCommand<T> : ICommand
    {
        private readonly Action<T> execute; // Делегат, представляющий действие, которое будет выполнено командой с параметром
        private readonly Func<T, bool> canExecute; // Делегат, возвращающий логическое значение, указывающее, можно ли выполнить команду с параметром

        // Конструктор принимает два параметра: действие и условие выполнения команды с параметром
        public RelayCommand(Action<T> execute, Func<T, bool> canExecute = null)
        {
            this.execute = execute ?? throw new ArgumentNullException(nameof(execute)); // Сохраняем действие, которое будет выполнено командой
            this.canExecute = canExecute; // Сохраняем условие выполнения команды с параметром, если оно указано
        }

        // Событие, которое вызывается, когда изменяется состояние выполнения команды
        public event EventHandler CanExecuteChanged
        {
            add { CommandManager.RequerySuggested += value; }
            remove { CommandManager.RequerySuggested -= value; }
        }

        // Метод CanExecute определяет, может ли команда выполняться
        public bool CanExecute(object parameter)
        {
            return canExecute == null || canExecute((T)parameter);
        }

        // Метод Execute выполняет действие, связанное с командой с параметром
        public void Execute(object parameter)
        {
            execute((T)parameter);
        }

        // Метод RaiseCanExecuteChanged вызывает событие CanExecuteChanged для обновления состояния выполнения команды
        public void RaiseCanExecuteChanged()
        {
            CommandManager.InvalidateRequerySuggested();
        }
    }
}
