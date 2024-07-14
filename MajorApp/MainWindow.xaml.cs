using System.Windows;
using System.Windows.Input;

namespace MajorAppMVVM2
{
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            InitializeComponent(); 
        }

        // Обработчик события нажатия кнопки для открытия окна заявок
        private void OpenOrdersWindow(object sender, RoutedEventArgs e)
        {
            OrdersWindow ordersWindow = new OrdersWindow(); 
            ordersWindow.Show(); 
            Hide(); 
        }

        // Обработчик события предварительного ввода текста, который ограничивает ввод только числовыми значениями
        private void NumericOnly_PreviewTextInput(object sender, TextCompositionEventArgs e)
        {
            e.Handled = !IsTextNumeric(e.Text); 
        }

        // Метод проверки, является ли текст числовым значением
        private bool IsTextNumeric(string text)
        {
            return double.TryParse(text, out _); 
        }
    }
}
