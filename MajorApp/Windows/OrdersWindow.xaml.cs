using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Net.Http;
using System.Text.Json;
using System.Windows;
using System.Windows.Controls;
using MajorAppMVVM2.Models;

namespace MajorAppMVVM2
{
    public partial class OrdersWindow : Window
    {
        private static readonly HttpClient client = new HttpClient();
        private List<Order> _orders;  // Храним исходный список заказов

        public OrdersWindow()
        {
            InitializeComponent();
            LoadOrders();  // Загрузка данных при создании окна
        }

        // Универсальный метод для загрузки и обновления данных в DataGrid
        private async void LoadOrders()
        {
            try
            {
                var response = await client.GetAsync("https://localhost:5001/api/orders");

                if (response.IsSuccessStatusCode)
                {
                    var json = await response.Content.ReadAsStringAsync();
                    _orders = JsonSerializer.Deserialize<List<Order>>(json, new JsonSerializerOptions { PropertyNameCaseInsensitive = true });
                    dataGridOrders.ItemsSource = _orders;  // Устанавливаем список заказов в DataGrid
                }
                else
                {
                    MessageBox.Show($"Failed to load orders. Status Code: {response.StatusCode}");
                }
            }
            catch (HttpRequestException ex)
            {
                MessageBox.Show($"Request error: {ex.Message}");
            }
        }

        // Метод для создания новой заявки
        private void CreateNewOrder(object sender, RoutedEventArgs e)
        {
            var createOrderWindow = new MainWindow();
            createOrderWindow.Show();
            Hide();
        }

        // Метод для удаления заявки
        private async void DeleteOrder(object sender, RoutedEventArgs e)
        {
            var button = sender as Button;
            var order = button?.DataContext as Order;

            if (order == null)
                return;

            var result = MessageBox.Show($"Вы уверены, что хотите удалить заказ {order.Id}?", "Удалить заявку", MessageBoxButton.YesNo);

            if (result == MessageBoxResult.Yes)
            {
                try
                {
                    var response = await client.DeleteAsync($"https://localhost:5001/api/orders/{order.Id}");

                    if (response.IsSuccessStatusCode)
                    {
                        MessageBox.Show("Order deleted successfully.");
                        LoadOrders();  // Обновление данных после удаления заявки
                    }
                    else
                    {
                        MessageBox.Show($"Failed to delete order. Status Code: {response.StatusCode}");
                    }
                }
                catch (HttpRequestException ex)
                {
                    MessageBox.Show($"Request error: {ex.Message}");
                }
            }
        }

        // Событие, вызываемое при изменении текста в поле поиска
        private void SearchTextBox_TextChanged(object sender, TextChangedEventArgs e)
        {
            FilterOrders();
        }

        // Событие, вызываемое при изменении выбранной даты в DatePicker
        private void DatePicker_SelectedDateChanged(object sender, SelectionChangedEventArgs e)
        {
            FilterOrders();
        }

        // Метод для поиска заявок по введенному тексту и датам
        private void FilterOrders()
        {
            string searchText = searchTextBox.Text.ToLowerInvariant();

            DateTime? startDate = startDatePicker.SelectedDate;
            DateTime? endDate = endDatePicker.SelectedDate;

            // Фильтруем список заказов
            var filteredOrders = _orders.Where(o =>
                (string.IsNullOrWhiteSpace(searchText) ||
                (o.Id.ToString().Contains(searchText)) ||
                (o.Status?.ToLowerInvariant().Contains(searchText) == true) ||
                (o.Description?.ToLowerInvariant().Contains(searchText) == true) ||
                (o.PickupAddress?.ToLowerInvariant().Contains(searchText) == true) ||
                (o.DeliveryAddress?.ToLowerInvariant().Contains(searchText) == true) ||
                (o.Executor?.ToLowerInvariant().Contains(searchText) == true) ||
                (o.Comment?.ToLowerInvariant().Contains(searchText) == true) ||
                (o.CreatedDate.ToString("yyyy-MM-dd").ToLowerInvariant().Contains(searchText)) ||
                (o.UpdatedDate.ToString("yyyy-MM-dd").ToLowerInvariant().Contains(searchText)) ||
                (o.Weight.ToString().ToLowerInvariant().Contains(searchText)))
                && (!startDate.HasValue || o.CreatedDate >= startDate.Value)
                && (!endDate.HasValue || o.CreatedDate <= endDate.Value)
            ).ToList();

            dataGridOrders.ItemsSource = new ObservableCollection<Order>(filteredOrders);
        }

        // Метод для обновления заявки
        private void EditOrder(object sender, RoutedEventArgs e)
        {
            if (dataGridOrders.SelectedItem is Order selectedOrder)
            {
                // Создание и отображение окна редактирования с данными выбранной заявки
                var editWindow = new EditOrderWindow(selectedOrder);
                editWindow.ShowDialog();

                LoadOrders();
            }
        }


    }
}
