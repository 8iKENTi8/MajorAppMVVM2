using MajorAppMVVM2.Logging;
using MajorAppMVVM2.Models;
using MajorAppMVVM2.Utils;
using MajorAppMVVM2.Windows;
using System.Linq;
using System.Net.Http;
using System.Text.Json;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;

namespace MajorAppMVVM2
{
    public partial class EditOrderWindow : Window
    {
        private static readonly HttpClient client = new HttpClient();
        private Order _order;
        private readonly StatusChangeLogger _statusChangeLogger;

        public EditOrderWindow(Order order)
        {
            InitializeComponent();
            _order = order;
            _statusChangeLogger = new StatusChangeLogger("status_change.log");
            _order.AttachLogger(_statusChangeLogger);
            LoadOrderDetails();
            LoadExecutors(); // Загрузка исполнителей
        }

        private async void LoadExecutors()
        {
            try
            {
                var executors = await ExecutorUtils.GetExecutorsAsync();
                comboBoxExecutor.ItemsSource = executors;
                comboBoxExecutor.DisplayMemberPath = "Name";
                comboBoxExecutor.SelectedItem = executors.FirstOrDefault(e => e.Name == _order.Executor);
            }
            catch (HttpRequestException ex)
            {
                MessageBox.Show($"Не удалось загрузить исполнителей: {ex.Message}");
            }
        }

        private void LoadOrderDetails()
        {
            // Заполнение полей данными из выбранного заказа
            textBoxDescription.Text = _order.Description;
            textBoxPickupAddress.Text = _order.PickupAddress;
            textBoxDeliveryAddress.Text = _order.DeliveryAddress;
            textBoxComment.Text = _order.Comment;
            textBoxWidth.Text = _order.Width.ToString();
            textBoxHeight.Text = _order.Height.ToString();
            textBoxDepth.Text = _order.Depth.ToString();
            textBoxWeight.Text = _order.Weight.ToString();

            // Установка текущего статуса заявки
            comboBoxStatus.SelectedItem = comboBoxStatus.Items
                .Cast<ComboBoxItem>()
                .FirstOrDefault(item => item.Content.ToString() == _order.Status);

            // Устанавливаем ComboBoxExecutor значение по умолчанию
            comboBoxExecutor.SelectedItem = comboBoxExecutor.Items
                .Cast<ComboBoxItem>()
                .FirstOrDefault(item => item.Content.ToString() == _order.Executor);

            // Установка состояния полей
            SetFieldsState();
        }

        private void SetFieldsState()
        {
            bool isEditable = _order.Status == "Новая";

            // Поля редактируются только если статус "Новая"
            textBoxDescription.IsEnabled = isEditable;
            textBoxPickupAddress.IsEnabled = isEditable;
            textBoxDeliveryAddress.IsEnabled = isEditable;
            comboBoxExecutor.IsEnabled = isEditable;
            textBoxWidth.IsEnabled = isEditable;
            textBoxHeight.IsEnabled = isEditable;
            textBoxDepth.IsEnabled = isEditable;
            textBoxWeight.IsEnabled = isEditable;

            // Изменение цвета фона в зависимости от возможности редактирования
            var readOnlyColor = new SolidColorBrush(Color.FromRgb(211, 211, 211)); // Light Gray
            var editableColor = new SolidColorBrush(Colors.White);

            textBoxDescription.Background = isEditable ? editableColor : readOnlyColor;
            textBoxPickupAddress.Background = isEditable ? editableColor : readOnlyColor;
            textBoxDeliveryAddress.Background = isEditable ? editableColor : readOnlyColor;
            comboBoxExecutor.Background = isEditable ? editableColor : readOnlyColor;
            textBoxWidth.Background = isEditable ? editableColor : readOnlyColor;
            textBoxHeight.Background = isEditable ? editableColor : readOnlyColor;
            textBoxDepth.Background = isEditable ? editableColor : readOnlyColor;
            textBoxWeight.Background = isEditable ? editableColor : readOnlyColor;

            // Поле комментарий всегда редактируемо
            textBoxComment.IsEnabled = true;
            textBoxComment.Background = editableColor;
        }

        private async void SaveChanges_Click(object sender, RoutedEventArgs e)
        {
            // Валидация полей и проверка данных
            if (!ValidationUtils.ValidateOrderDetails(textBoxDescription, textBoxPickupAddress, textBoxDeliveryAddress, comboBoxExecutor,
                                                      textBoxWidth, textBoxHeight, textBoxDepth, textBoxWeight, null, out string errorMessage))
            {
                MessageBox.Show(errorMessage, "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
                return;
            }

            // Обновление данных заказа с помощью API
            UpdateOrderFromFields(updateComment: true);

            try
            {
                var json = JsonSerializer.Serialize(_order);
                var content = new StringContent(json, System.Text.Encoding.UTF8, "application/json");
                var response = await client.PutAsync($"https://localhost:5001/api/orders/{_order.Id}", content);

                if (response.IsSuccessStatusCode)
                {
                    MessageBox.Show("Изменения сохранены");
                    Close();
                }
                else
                {
                    MessageBox.Show($"Failed to update order. Status Code: {response.StatusCode}");
                }
            }
            catch (HttpRequestException ex)
            {
                MessageBox.Show($"Request error: {ex.Message}");
            }
        }

        private async void ComboBoxStatus_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            // Если событие вызвано изменением выбора статуса
            if (comboBoxStatus.SelectedItem is ComboBoxItem selectedStatus)
            {
                // Считываем новый статус из выбранного элемента
                string newStatus = selectedStatus.Content.ToString();

                // Не позволяем менять статус обратно на "Новая", если он уже изменен
                if (_order.Status != "Новая" && newStatus == "Новая")
                {
                    comboBoxStatus.SelectedItem = comboBoxStatus.Items
                        .Cast<ComboBoxItem>()
                        .FirstOrDefault(item => item.Content.ToString() == _order.Status);
                    MessageBox.Show("Статус заявки нельзя вернуть на 'Новая'");
                    return;
                }

                // Обработка смены статуса на "Отменена"
                if (_order.Status != "Отменена" && newStatus == "Отменена")
                {
                    var commentWindow = new CommentInputWindow();
                    if (commentWindow.ShowDialog() == true)
                    {
                        // Проверяем, не является ли комментарий пустым
                        if (string.IsNullOrWhiteSpace(commentWindow.Comment))
                        {
                            MessageBox.Show("Комментарий не может быть пустым", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Warning);
                            // Возвращаем статус к предыдущему значению, если комментарий пустой
                            comboBoxStatus.SelectedItem = comboBoxStatus.Items
                                .Cast<ComboBoxItem>()
                                .FirstOrDefault(item => item.Content.ToString() == _order.Status);
                            return;
                        }

                        _order.Comment = commentWindow.Comment;
                        _order.Status = newStatus;

                        // Обновляем заказ и сохраняем изменения
                        UpdateOrderFromFields(updateComment: false);
                        await SaveOrderAsync();
                    }
                    else
                    {
                        // Возвращаем статус к предыдущему значению, если окно комментария не подтверждено
                        comboBoxStatus.SelectedItem = comboBoxStatus.Items
                            .Cast<ComboBoxItem>()
                            .FirstOrDefault(item => item.Content.ToString() == _order.Status);
                    }
                }
                else if (_order.Status != newStatus)
                {
                    // Обновляем только статус заказа, если статус изменился на что-то другое
                    _order.Status = newStatus;

                    // Обновляем заказ и сохраняем изменения
                    UpdateOrderFromFields(updateComment: true);
                    await SaveOrderAsync();
                }
            }
        }



        private void UpdateOrderFromFields(bool updateComment)
        {
            _order.Description = textBoxDescription.Text;
            _order.PickupAddress = textBoxPickupAddress.Text;
            _order.DeliveryAddress = textBoxDeliveryAddress.Text;
            // Обновляем комментарий только если переводим заявку в статус Отменена
            if (updateComment)
            {
                _order.Comment = textBoxComment.Text;
            }
            _order.Executor = (comboBoxExecutor.SelectedItem as Executor)?.Name;
            _order.Width = double.TryParse(textBoxWidth.Text, out var width) ? width : _order.Width;
            _order.Height = double.TryParse(textBoxHeight.Text, out var height) ? height : _order.Height;
            _order.Depth = double.TryParse(textBoxDepth.Text, out var depth) ? depth : _order.Depth;
            _order.Weight = double.TryParse(textBoxWeight.Text, out var weight) ? weight : _order.Weight;

            if (comboBoxStatus.SelectedItem is ComboBoxItem selectedStatus)
            {
                _order.Status = selectedStatus.Content.ToString();
            }
        }

        private async Task SaveOrderAsync()
        {
            var json = JsonSerializer.Serialize(_order);
            var content = new StringContent(json, System.Text.Encoding.UTF8, "application/json");
            var response = await client.PutAsync($"https://localhost:5001/api/orders/{_order.Id}", content);

            if (response.IsSuccessStatusCode)
            {
                MessageBox.Show("Заявка отменена успешно.");
                Close();
            }
            else
            {
                MessageBox.Show($"Ошибка при отмене заявки. Код состояния: {response.StatusCode}");
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
    }
}
