using MajorAppMVVM2.Models;
using MajorAppMVVM2.ViewModels;
using System.Net.Http;
using System.Windows;
using System.Windows.Input;

namespace MajorAppMVVM2
{
    public partial class EditOrderWindow : Window
    {
        public EditOrderViewModel ViewModel { get; private set; }

        public EditOrderWindow(Order order)
        {
            InitializeComponent();
            ViewModel = new EditOrderViewModel(order);
            DataContext = ViewModel;
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
