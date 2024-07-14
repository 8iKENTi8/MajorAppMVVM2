using MajorAppMVVM2.Logging;
using MajorAppMVVM2.Models;
using MajorAppMVVM2.Utils;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Net.Http;
using System.Text.Json;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;

namespace MajorAppMVVM2.ViewModels
{
    public class EditOrderViewModel : INotifyPropertyChanged
    {
        private static readonly HttpClient client = new HttpClient();
        private readonly StatusChangeLogger _statusChangeLogger;
        private Order _order;
        private List<Executor> _executors;
        private Executor _selectedExecutor;
        private string _description;
        private string _pickupAddress;
        private string _deliveryAddress;
        private string _comment;
        private double _width;
        private double _height;
        private double _depth;
        private double _weight;
        private DateTime? _createdDate;
        private string _status;

        public event PropertyChangedEventHandler PropertyChanged;

        private List<string> _statuses = new List<string>
        {
            "Новая",
            "Передано на выполнение",
            "Выполнено",
            "Отменена"
        };

        public List<string> Statuses
        {
            get => _statuses;
            set
            {
                _statuses = value;
                OnPropertyChanged(nameof(Statuses));
            }
        }

        public EditOrderViewModel(Order order)
        {
            _order = order;
            _statusChangeLogger = new StatusChangeLogger("status_change.log");
            _order.AttachLogger(_statusChangeLogger);

            // Инициализация команд
            SaveChangesCommand = new RelayCommand(SaveChanges, CanSaveChanges);

            // Загрузка данных
            LoadDataAsync();
        }

        private async void LoadDataAsync()
        {
            try
            {
                // Загрузка исполнителей
                Executors = await ExecutorUtils.GetExecutorsAsync();

                // Установка статусов
                Statuses = new List<string> { "Новая", "Передано на выполнение", "Выполнено", "Отменена" };

                // Загрузка деталей заказа после загрузки исполнителей
                LoadOrderDetails();
            }
            catch (HttpRequestException ex)
            {
                MessageBox.Show($"Не удалось загрузить исполнителей: {ex.Message}");
            }
        }

        public List<Executor> Executors
        {
            get => _executors;
            set
            {
                _executors = value;
                OnPropertyChanged(nameof(Executors));  // Указание имени свойства
                ((RelayCommand)SaveChangesCommand).RaiseCanExecuteChanged();
            }
        }

        public Executor SelectedExecutor
        {
            get => _selectedExecutor;
            set
            {
                _selectedExecutor = value;
                OnPropertyChanged(nameof(SelectedExecutor));
                // Обновление состояния команды при изменении выбранного исполнителя
                ((RelayCommand)SaveChangesCommand).RaiseCanExecuteChanged();
            }
        }

        public string Description
        {
            get => _description;
            set
            {
                _description = value;
                OnPropertyChanged(nameof(Description));
                // Обновление состояния команды при изменении описания
                ((RelayCommand)SaveChangesCommand).RaiseCanExecuteChanged();
            }
        }

        public string PickupAddress
        {
            get => _pickupAddress;
            set
            {
                _pickupAddress = value;
                OnPropertyChanged(nameof(PickupAddress));
                // Обновление состояния команды при изменении адреса подачи
                ((RelayCommand)SaveChangesCommand).RaiseCanExecuteChanged();
            }
        }

        public string DeliveryAddress
        {
            get => _deliveryAddress;
            set
            {
                _deliveryAddress = value;
                OnPropertyChanged(nameof(DeliveryAddress));
                // Обновление состояния команды при изменении адреса доставки
                ((RelayCommand)SaveChangesCommand).RaiseCanExecuteChanged();
            }
        }

        public string Comment
        {
            get => _comment;
            set
            {
                _comment = value;
                OnPropertyChanged(nameof(Comment));
                // Обновление состояния команды при изменении комментария
                ((RelayCommand)SaveChangesCommand).RaiseCanExecuteChanged();
            }
        }

        public double Width
        {
            get => _width;
            set
            {
                _width = value;
                OnPropertyChanged(nameof(Width));
                // Обновление состояния команды при изменении ширины
                ((RelayCommand)SaveChangesCommand).RaiseCanExecuteChanged();
            }
        }

        public double Height
        {
            get => _height;
            set
            {
                _height = value;
                OnPropertyChanged(nameof(Height));
                // Обновление состояния команды при изменении высоты
                ((RelayCommand)SaveChangesCommand).RaiseCanExecuteChanged();
            }
        }

        public double Depth
        {
            get => _depth;
            set
            {
                _depth = value;
                OnPropertyChanged(nameof(Depth));
                // Обновление состояния команды при изменении глубины
                ((RelayCommand)SaveChangesCommand).RaiseCanExecuteChanged();
            }
        }

        public double Weight
        {
            get => _weight;
            set
            {
                _weight = value;
                OnPropertyChanged(nameof(Weight));
                // Обновление состояния команды при изменении веса
                ((RelayCommand)SaveChangesCommand).RaiseCanExecuteChanged();
            }
        }

        public DateTime? CreatedDate
        {
            get => _createdDate;
            set
            {
                _createdDate = value;
                OnPropertyChanged(nameof(CreatedDate));
                // Обновление состояния команды при изменении даты создания
                ((RelayCommand)SaveChangesCommand).RaiseCanExecuteChanged();
            }
        }

        public string Status
        {
            get => _status;
            set
            {
                _status = value;
                OnPropertyChanged(nameof(Status));
                // Обновление состояния команды при изменении статуса
                ((RelayCommand)SaveChangesCommand).RaiseCanExecuteChanged();
            }
        }

        public ICommand SaveChangesCommand { get; private set; }

        private bool CanSaveChanges()
        {
            bool canSave = !string.IsNullOrWhiteSpace(Description) &&
                           !string.IsNullOrWhiteSpace(PickupAddress) &&
                           !string.IsNullOrWhiteSpace(DeliveryAddress) &&
                           SelectedExecutor != null &&
                           Width > 0 && Height > 0 && Depth > 0 && Weight > 0 && CreatedDate.HasValue;

            return canSave;
        }

        private async void SaveChanges()
        {
            try
            {
                // Обновление данных заказа
                _order.Description = Description;
                _order.PickupAddress = PickupAddress;
                _order.DeliveryAddress = DeliveryAddress;
                _order.Comment = Comment;
                _order.Executor = SelectedExecutor?.Name;  // Добавил проверку на null
                _order.Width = Width;
                _order.Height = Height;
                _order.Depth = Depth;
                _order.Weight = Weight;
                _order.Status = Status;

                var json = JsonSerializer.Serialize(_order);
                var content = new StringContent(json, System.Text.Encoding.UTF8, "application/json");
                var response = await client.PutAsync($"https://localhost:5001/api/orders/{_order.Id}", content);

                if (response.IsSuccessStatusCode)
                {
                    MessageBox.Show("Изменения сохранены");
                    // Закрытие окна редактирования после успешного сохранения
                    HideWindow();
                }
                else
                {
                    MessageBox.Show($"Ошибка при сохранении изменений. Код состояния: {response.StatusCode}");
                }
            }
            catch (HttpRequestException ex)
            {
                MessageBox.Show($"Ошибка HTTP запроса: {ex.Message}");
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Произошла ошибка: {ex.Message}");
            }
        }

        private void LoadOrderDetails()
        {
            // Загрузка деталей заказа
            Description = _order.Description;
            PickupAddress = _order.PickupAddress;
            DeliveryAddress = _order.DeliveryAddress;
            Comment = _order.Comment;
            Width = _order.Width;
            Height = _order.Height;
            Depth = _order.Depth;
            Weight = _order.Weight;
            CreatedDate = _order.CreatedDate;
            Status = _order.Status;

            // Установка выбранного исполнителя
            SelectedExecutor = Executors?.FirstOrDefault(e => e.Name == _order.Executor);
        }

        private void HideWindow()
        {
            Application.Current.Windows.OfType<Window>().SingleOrDefault(x => x.IsActive)?.Hide();
        }

        protected void OnPropertyChanged(string propertyName)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));

            // Обновляем состояние команды при изменении свойств, влияющих на возможность сохранения изменений
            if (propertyName == nameof(Description) ||
                propertyName == nameof(PickupAddress) ||
                propertyName == nameof(DeliveryAddress) ||
                propertyName == nameof(SelectedExecutor) ||
                propertyName == nameof(Width) ||
                propertyName == nameof(Height) ||
                propertyName == nameof(Depth) ||
                propertyName == nameof(Weight) ||
                propertyName == nameof(CreatedDate) ||
                propertyName == nameof(Status))
            {
                ((RelayCommand)SaveChangesCommand).RaiseCanExecuteChanged();
            }
        }
    }
}
