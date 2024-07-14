using MajorAppMVVM2.Logging;
using MajorAppMVVM2.Models;
using MajorAppMVVM2.Utils;
using MajorAppMVVM2.Windows;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;
using System.IO;


namespace MajorAppMVVM2.ViewModels
{
    public class EditOrderViewModel : INotifyPropertyChanged
    {
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

        // Событие для уведомления об изменениях свойств
        public event PropertyChangedEventHandler PropertyChanged;

        // Список возможных статусов заказа
        private List<string> _statuses = new List<string>
        {
            "Новая",
            "Передано на выполнение",
            "Выполнено",
            "Отменена"
        };

        // Свойство для получения и установки списка статусов
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
            // Инициализация логгера для отслеживания изменений статуса
            string logDirectoryPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Logs");
            _statusChangeLogger = new StatusChangeLogger(logDirectoryPath);
            _order.AttachLogger(_statusChangeLogger); // Прикрепляем логгер к заказу

            // Инициализация команд
            SaveChangesCommand = new RelayCommand(async () => await SaveChanges(updateComment: true), CanSaveChanges);

            // Загрузка данных
            LoadDataAsync();
        }

        // Метод для асинхронной загрузки данных о заказе и исполнителях
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

        // Свойство для получения и установки списка исполнителей
        public List<Executor> Executors
        {
            get => _executors;
            set
            {
                _executors = value;
                OnPropertyChanged(nameof(Executors));
                ((RelayCommand)SaveChangesCommand).RaiseCanExecuteChanged();
            }
        }

        // Свойство для получения и установки выбранного исполнителя
        public Executor SelectedExecutor
        {
            get => _selectedExecutor;
            set
            {
                _selectedExecutor = value;
                OnPropertyChanged(nameof(SelectedExecutor));
                ((RelayCommand)SaveChangesCommand).RaiseCanExecuteChanged();
            }
        }

        // Свойство для получения и установки описания заказа
        public string Description
        {
            get => _description;
            set
            {
                _description = value;
                OnPropertyChanged(nameof(Description));
                ((RelayCommand)SaveChangesCommand).RaiseCanExecuteChanged();
            }
        }

        // Свойство для получения и установки адреса получения заказа
        public string PickupAddress
        {
            get => _pickupAddress;
            set
            {
                _pickupAddress = value;
                OnPropertyChanged(nameof(PickupAddress));
                ((RelayCommand)SaveChangesCommand).RaiseCanExecuteChanged();
            }
        }

        // Свойство для получения и установки адреса доставки заказа
        public string DeliveryAddress
        {
            get => _deliveryAddress;
            set
            {
                _deliveryAddress = value;
                OnPropertyChanged(nameof(DeliveryAddress));
                ((RelayCommand)SaveChangesCommand).RaiseCanExecuteChanged();
            }
        }

        // Свойство для получения и установки комментария к заказу
        public string Comment
        {
            get => _comment;
            set
            {
                _comment = value;
                OnPropertyChanged(nameof(Comment));
                ((RelayCommand)SaveChangesCommand).RaiseCanExecuteChanged();
            }
        }

        // Свойство для получения и установки ширины груза
        public double Width
        {
            get => _width;
            set
            {
                _width = value;
                OnPropertyChanged(nameof(Width));
                ((RelayCommand)SaveChangesCommand).RaiseCanExecuteChanged();
            }
        }

        // Свойство для получения и установки высоты груза
        public double Height
        {
            get => _height;
            set
            {
                _height = value;
                OnPropertyChanged(nameof(Height));
                ((RelayCommand)SaveChangesCommand).RaiseCanExecuteChanged();
            }
        }

        // Свойство для получения и установки глубины груза
        public double Depth
        {
            get => _depth;
            set
            {
                _depth = value;
                OnPropertyChanged(nameof(Depth));
                ((RelayCommand)SaveChangesCommand).RaiseCanExecuteChanged();
            }
        }

        // Свойство для получения и установки веса груза
        public double Weight
        {
            get => _weight;
            set
            {
                _weight = value;
                OnPropertyChanged(nameof(Weight));
                ((RelayCommand)SaveChangesCommand).RaiseCanExecuteChanged();
            }
        }

        // Свойство для получения и установки даты создания заказа
        public DateTime? CreatedDate
        {
            get => _createdDate;
            set
            {
                _createdDate = value;
                OnPropertyChanged(nameof(CreatedDate));
                ((RelayCommand)SaveChangesCommand).RaiseCanExecuteChanged();
            }
        }

        // Свойство для получения и установки статуса заказа
        public string Status
        {
            get => _status;
            set
            {
                if (_status != value)
                {
                    _status = value;
                    OnPropertyChanged(nameof(Status));

                    // Устанавливаем доступность полей в зависимости от статуса
                    OnPropertyChanged(nameof(IsDescriptionEditable));
                    OnPropertyChanged(nameof(IsPickupAddressEditable));
                    OnPropertyChanged(nameof(IsDeliveryAddressEditable));
                    OnPropertyChanged(nameof(IsExecutorEditable));
                    OnPropertyChanged(nameof(IsWidthEditable));
                    OnPropertyChanged(nameof(IsHeightEditable));
                    OnPropertyChanged(nameof(IsDepthEditable));
                    OnPropertyChanged(nameof(IsWeightEditable));

                    // Обработка изменения статуса на "Отменена"
                    if (_status == "Отменена" && _order.Status != "Отменена")
                    {
                        _ = HandleStatusChangeToCancelled();
                    }
                    else if (_status != "Отменена")
                    {
                        _order.Status = _status;
                    }

                    // Обновление состояния команды при изменении статуса
                    ((RelayCommand)SaveChangesCommand).RaiseCanExecuteChanged();
                }
            }
        }

        // Команда для сохранения изменений заказа
        public ICommand SaveChangesCommand { get; private set; }

        // Определяем, можно ли редактировать поля в зависимости от статуса
        public bool IsDescriptionEditable => Status == "Новая";
        public bool IsPickupAddressEditable => Status == "Новая";
        public bool IsDeliveryAddressEditable => Status == "Новая";
        public bool IsExecutorEditable => Status == "Новая";
        public bool IsWidthEditable => Status == "Новая";
        public bool IsHeightEditable => Status == "Новая";
        public bool IsDepthEditable => Status == "Новая";
        public bool IsWeightEditable => Status == "Новая";

        // Метод для проверки, можно ли сохранить изменения
        private bool CanSaveChanges()
        {
            // Проверка, что статус можно редактировать
            bool canSave = !string.IsNullOrWhiteSpace(Description) &&
                           !string.IsNullOrWhiteSpace(PickupAddress) &&
                           !string.IsNullOrWhiteSpace(DeliveryAddress) &&
                           SelectedExecutor != null &&
                           Width > 0 && Height > 0 && Depth > 0 && Weight > 0 && CreatedDate.HasValue;

            return canSave;
        }

        // Асинхронный метод для сохранения изменений заказа
        private async Task SaveChanges(bool updateComment)
        {
            try
            {
                // Обновление данных заказа
                _order.Description = Description;
                _order.PickupAddress = PickupAddress;
                _order.DeliveryAddress = DeliveryAddress;
                // Обновляем комментарий только если переводим заявку в статус Отменена
                if (updateComment)
                {
                    _order.Comment = Comment;
                }
                _order.Executor = SelectedExecutor?.Name;  // Добавил проверку на null
                _order.Width = Width;
                _order.Height = Height;
                _order.Depth = Depth;
                _order.Weight = Weight;
                _order.Status = Status;

                var response = await HttpClientUtils.SendRequestAsync(HttpMethod.Put, $"https://localhost:5001/api/orders/{_order.Id}", _order);

                if (response != null && response.IsSuccessStatusCode)
                {
                    MessageBox.Show("Изменения сохранены");
                    // Закрытие окна редактирования после успешного сохранения
                    HideWindow();
                }
                else
                {
                    MessageBox.Show($"Ошибка при сохранении изменений. Код состояния: {response?.StatusCode}");
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

        // Метод для загрузки деталей заказа
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
            // Устанавливаем доступность полей в зависимости от статуса
            OnPropertyChanged(nameof(IsDescriptionEditable));
            OnPropertyChanged(nameof(IsPickupAddressEditable));
            OnPropertyChanged(nameof(IsDeliveryAddressEditable));
            OnPropertyChanged(nameof(IsExecutorEditable));
            OnPropertyChanged(nameof(IsWidthEditable));
            OnPropertyChanged(nameof(IsHeightEditable));
            OnPropertyChanged(nameof(IsDepthEditable));
            OnPropertyChanged(nameof(IsWeightEditable));
        }

        // Метод для скрытия окна редактирования заказа
        private void HideWindow()
        {
            Application.Current.Windows.OfType<EditOrderWindow>().SingleOrDefault(x => x.IsActive)?.Close();
        }


        // Метод для обработки изменения статуса заказа на "Отменена"
        private async Task HandleStatusChangeToCancelled()
        {
            var commentWindow = new CommentInputWindow();
            if (commentWindow.ShowDialog() == true)
            {
                var comment = commentWindow.Comment;

                // Проверяем, не является ли комментарий пустым
                if (string.IsNullOrWhiteSpace(comment))
                {
                    MessageBox.Show("Комментарий не может быть пустым", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Warning);
                    // Возвращаем статус к предыдущему значению
                    Status = _order.Status;
                    return;
                }

                // Устанавливаем комментарий и статус
                _order.Comment = comment;
                _order.Status = Status;

                // Обновляем данные в базе данных
                await SaveChanges(updateComment: false);

                // Закрываем окно редактирования после успешного сохранения
                HideWindow();

            }
            else
            {
                // Возвращаем статус к предыдущему значению, если окно ввода закрыто без ввода комментария
                Status = _order.Status;
            }
        }

        // Метод для уведомления об изменении свойств
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
                propertyName == nameof(Status) ||
                propertyName == nameof(IsDescriptionEditable) ||
                propertyName == nameof(IsPickupAddressEditable) ||
                propertyName == nameof(IsDeliveryAddressEditable) ||
                propertyName == nameof(IsExecutorEditable) ||
                propertyName == nameof(IsWidthEditable) ||
                propertyName == nameof(IsHeightEditable) ||
                propertyName == nameof(IsDepthEditable) ||
                propertyName == nameof(IsWeightEditable))
            {
                ((RelayCommand)SaveChangesCommand).RaiseCanExecuteChanged();
            }
        }
    }
}
