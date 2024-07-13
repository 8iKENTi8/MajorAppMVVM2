using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;
using System.Windows;
using MajorAppMVVM2.Models;
using MajorAppMVVM2.Utils;

namespace MajorAppMVVM2
{
    public partial class MainWindow : Window
    {
        private static readonly HttpClient client = new HttpClient();  // Статический HttpClient для выполнения HTTP запросов

        public MainWindow()
        {
            InitializeComponent();  // Инициализация компонентов окна
            LoadExecutors();  // Загрузка данных исполнителей в AutoCompleteBox
        }

        // Метод для получения исполнителей
        private async void LoadExecutors()
        {
            try
            {
                var executors = await ExecutorUtils.GetExecutorsAsync();
                comboBoxExecutor.ItemsSource = executors;
                comboBoxExecutor.DisplayMemberPath = "Name";
            }
            catch (HttpRequestException ex)
            {
                MessageBox.Show($"Не удалось загрузить исполнителей: {ex.Message}");
            }
        }

        // Метод для создания новой заявки
        private async void CreateOrder(object sender, RoutedEventArgs e)
        {
            // Проверка валидности данных из текстовых полей и других элементов управления
            if (!ValidationUtils.ValidateOrderDetails(textBoxDescription, textBoxPickupAddress, textBoxDeliveryAddress, comboBoxExecutor,
                                                      textBoxWidth, textBoxHeight, textBoxDepth, textBoxWeight, datePickerCreatedDate, out string errorMessage))
            {
                MessageBox.Show(errorMessage, "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
                return;
            }

            // Создаем объект заказа на основе введенных данных
            var order = new Order
            {
                Description = textBoxDescription.Text,  
                PickupAddress = textBoxPickupAddress.Text,  
                DeliveryAddress = textBoxDeliveryAddress.Text,  
                Comment = textBoxComment.Text,  
                CreatedDate = datePickerCreatedDate.SelectedDate.GetValueOrDefault(),  
                UpdatedDate = DateTime.Now,
                Executor = (comboBoxExecutor.SelectedItem as Executor)?.Name,
                Width = double.Parse(textBoxWidth.Text),  
                Height = double.Parse(textBoxHeight.Text),  
                Depth = double.Parse(textBoxDepth.Text),  
                Weight = double.Parse(textBoxWeight.Text)  
            };

            // Преобразуем объект заказа в JSON формат для отправки на сервер
            var json = JsonSerializer.Serialize(order);
            var content = new StringContent(json, Encoding.UTF8, "application/json");

            try
            {
                // Отправляем POST запрос на сервер для создания новой заявки
                var response = await client.PostAsync("https://localhost:5001/api/orders", content);

                // Проверяем успешность запроса и отображаем соответствующее сообщение
                if (response.IsSuccessStatusCode)
                {
                    MessageBox.Show("Заявка успешно создана.");
                }
                else
                {
                    MessageBox.Show($"Не удалось создать заявку. Код статуса: {response.StatusCode}");
                }
            }
            catch (HttpRequestException ex)
            {
                // Обработка ошибок HTTP запросов и отображение сообщения об ошибке
                MessageBox.Show($"Ошибка запроса: {ex.Message}");
            }
        }

        // Метод для предотвращения ввода нечисловых символов в текстовые поля для числовых данных
        private void NumericOnly_PreviewTextInput(object sender, System.Windows.Input.TextCompositionEventArgs e)
        {
            e.Handled = !IsTextAllowed(e.Text);
        }

        // Метод для проверки, может ли текст быть преобразован в число типа double
        private bool IsTextAllowed(string text)
        {
            return double.TryParse(text, out _); 
        }

        // Метод для открытия окна с просмотром заявок 
        private void OpenOrdersWindow(object sender, RoutedEventArgs e)
        {
            OrdersWindow ordersWindow = new OrdersWindow();
            ordersWindow.Show();
            Hide();  
        }
    }
}
