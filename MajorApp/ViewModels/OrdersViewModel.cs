using System.Collections.Generic;
using System;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Net.Http;
using System.Text.Json;
using System.Windows;
using System.Windows.Input;
using MajorAppMVVM2.Models;
using MajorAppMVVM2.Utils;

namespace MajorAppMVVM2.ViewModels
{
    public class OrdersViewModel : INotifyPropertyChanged
    {
        private ObservableCollection<Order> _orders;
        private string _searchText;
        private DateTime? _startDate;
        private DateTime? _endDate;
        private readonly Window _window;

        // Свойство для получения и установки коллекции заказов
        public ObservableCollection<Order> Orders
        {
            get => _orders;
            set
            {
                _orders = value;
                OnPropertyChanged(nameof(Orders));
            }
        }

        // Свойство для получения и установки текста поиска
        public string SearchText
        {
            get => _searchText;
            set
            {
                _searchText = value;
                OnPropertyChanged(nameof(SearchText));
                FilterOrders();
            }
        }

        // Свойство для получения и установки начальной даты фильтрации
        public DateTime? StartDate
        {
            get => _startDate;
            set
            {
                _startDate = value;
                OnPropertyChanged(nameof(StartDate));
                FilterOrders();
            }
        }

        // Свойство для получения и установки конечной даты фильтрации
        public DateTime? EndDate
        {
            get => _endDate;
            set
            {
                _endDate = value;
                OnPropertyChanged(nameof(EndDate));
                FilterOrders();
            }
        }

        // Команда для создания новой заявки
        public ICommand CreateNewOrderCommand { get; }

        // Команда для редактирования выбранной заявки
        public ICommand EditOrderCommand { get; }

        // Команда для удаления выбранной заявки
        public ICommand DeleteOrderCommand { get; }

        // Конструктор для инициализации команд и загрузки заказов
        public OrdersViewModel(Window window)
        {
            _window = window;
            CreateNewOrderCommand = new RelayCommand(CreateNewOrder);  // Инициализация команды создания новой заявки
            EditOrderCommand = new RelayCommand<Order>(EditOrder, order => order != null);  // Инициализация команды редактирования заявки
            DeleteOrderCommand = new RelayCommand<Order>(DeleteOrder, order => order != null);  // Инициализация команды удаления заявки
            LoadOrders();  // Загрузка заявок при инициализации ViewModel
        }

        // Асинхронный метод для загрузки заказов из API
        private async void LoadOrders()
        {
            try
            {
                var response = await HttpClientUtils.SendRequestAsync(HttpMethod.Get, "https://localhost:5001/api/orders");
                if (response.IsSuccessStatusCode)
                {
                    var json = await response.Content.ReadAsStringAsync();
                    var orders = JsonSerializer.Deserialize<List<Order>>(json, new JsonSerializerOptions { PropertyNameCaseInsensitive = true });
                    Orders = new ObservableCollection<Order>(orders);  // Присваиваем загруженные заказы свойству Orders
                }
                else
                {
                    MessageBox.Show($"Не удалось загрузить заказы. Код состояния: {response.StatusCode}");  // Сообщаем о неудачной загрузке
                }
            }
            catch (HttpRequestException ex)
            {
                MessageBox.Show($"Ошибка запроса: {ex.Message}");  // Обработка ошибок при запросе данных
            }
        }

        // Метод для создания новой заявки
        private void CreateNewOrder()
        {
            var createOrderWindow = new MainWindow();
            createOrderWindow.Closed += (s, e) => _window.Show();
            createOrderWindow.Show();
            _window.Hide();
        }

        // Метод для редактирования выбранной заявки
        private void EditOrder(Order order)
        {
            var editOrderWindow = new EditOrderWindow(order);
            editOrderWindow.ShowDialog();
            LoadOrders();
        }

        // Асинхронный метод для удаления выбранной заявки
        private async void DeleteOrder(Order order)
        {
            if (order == null)
                return;

            var result = MessageBox.Show($"Вы уверены, что хотите удалить заказ {order.Id}?", "Удалить заявку", MessageBoxButton.YesNo, MessageBoxImage.Question);  // Сообщаем о необходимости подтверждения удаления заявки

            if (result == MessageBoxResult.Yes)
            {
                try
                {
                    var response = await HttpClientUtils.SendRequestAsync(HttpMethod.Delete, $"https://localhost:5001/api/orders/{order.Id}");

                    if (response.IsSuccessStatusCode)
                    {
                        MessageBox.Show("Заявка успешно удалена.");  // Сообщаем об успешном удалении заявки
                        LoadOrders();  // Перезагружаем заказы после удаления заявки
                    }
                    else
                    {
                        MessageBox.Show($"Не удалось удалить заявку. Код состояния: {response.StatusCode}");  // Сообщаем о неудачном удалении заявки
                    }
                }
                catch (HttpRequestException ex)
                {
                    MessageBox.Show($"Ошибка запроса: {ex.Message}");  // Обработка ошибок при запросе данных
                }
            }
        }

        // Метод для фильтрации заявок по тексту поиска и датам
        private void FilterOrders()
        {
            if (_orders == null) return;  // Проверяем, что коллекция заказов не пустая

            var filteredOrders = _orders.Where(o =>
                (string.IsNullOrWhiteSpace(SearchText) ||  // Проверяем, что текст поиска пустой или заявка соответствует тексту поиска
                 o.Id.ToString().Contains(SearchText) ||
                 o.Status?.ToLowerInvariant().Contains(SearchText.ToLowerInvariant()) == true ||
                 o.Description?.ToLowerInvariant().Contains(SearchText.ToLowerInvariant()) == true ||
                 o.PickupAddress?.ToLowerInvariant().Contains(SearchText.ToLowerInvariant()) == true ||
                 o.DeliveryAddress?.ToLowerInvariant().Contains(SearchText.ToLowerInvariant()) == true ||
                 o.Executor?.ToLowerInvariant().Contains(SearchText.ToLowerInvariant()) == true ||
                 o.Comment?.ToLowerInvariant().Contains(SearchText.ToLowerInvariant()) == true ||
                 o.CreatedDate.ToString("yyyy-MM-dd").ToLowerInvariant().Contains(SearchText.ToLowerInvariant()) ||
                 o.UpdatedDate.ToString("yyyy-MM-dd").ToLowerInvariant().Contains(SearchText.ToLowerInvariant()) ||
                 o.Weight.ToString().ToLowerInvariant().Contains(SearchText.ToLowerInvariant()))
                && (!StartDate.HasValue || o.CreatedDate >= StartDate.Value)  // Проверяем, что дата создания заявки больше или равна начальной дате фильтрации
                && (!EndDate.HasValue || o.CreatedDate <= EndDate.Value)  // Проверяем, что дата создания заявки меньше или равна конечной дате фильтрации
            ).ToList();

            Orders = new ObservableCollection<Order>(filteredOrders);  // Присваиваем отфильтрованные заказы свойству Orders
        }

        // Событие для уведомления об изменениях свойств
        public event PropertyChangedEventHandler PropertyChanged;

        // Метод для вызова события PropertyChanged
        protected void OnPropertyChanged(string propertyName)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));  // Уведомляем об изменении свойства
        }
    }
}
