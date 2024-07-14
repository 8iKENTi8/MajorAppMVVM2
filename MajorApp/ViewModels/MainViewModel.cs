using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Net.Http;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;
using MajorAppMVVM2.Models;
using MajorAppMVVM2.Utils;

namespace MajorAppMVVM2.ViewModels
{
    public class MainViewModel : INotifyPropertyChanged
    {
        // Поля для хранения данных ViewModel
        private List<Executor> executors;
        private string description;
        private string pickupAddress;
        private string deliveryAddress;
        private string comment;
        private Executor selectedExecutor;
        private double width;
        private double height;
        private double depth;
        private double weight;
        private DateTime? createdDate;

        // Событие, вызываемое при изменении свойства
        public event PropertyChangedEventHandler PropertyChanged;

        public MainViewModel()
        {
            // Загрузка списка исполнителей
            LoadExecutors();
            // Инициализация команды создания заявки
            CreateOrderCommand = new RelayCommand(async () => await CreateOrder(), CanCreateOrder);
        }

        // Список исполнителей
        public List<Executor> Executors
        {
            get => executors;
            set
            {
                executors = value;
                OnPropertyChanged();
            }
        }

        // Описание груза
        public string Description
        {
            get => description;
            set
            {
                description = value;
                OnPropertyChanged();
                OnPropertyChanged(nameof(CreateOrderCommand));
            }
        }

        // Адрес получения груза
        public string PickupAddress
        {
            get => pickupAddress;
            set
            {
                pickupAddress = value;
                OnPropertyChanged();
                OnPropertyChanged(nameof(CreateOrderCommand));
            }
        }

        // Адрес доставки груза
        public string DeliveryAddress
        {
            get => deliveryAddress;
            set
            {
                deliveryAddress = value;
                OnPropertyChanged();
                OnPropertyChanged(nameof(CreateOrderCommand));
            }
        }

        // Комментарий к заявке
        public string Comment
        {
            get => comment;
            set
            {
                comment = value;
                OnPropertyChanged();
                OnPropertyChanged(nameof(CreateOrderCommand));
            }
        }

        // Выбранный исполнитель
        public Executor SelectedExecutor
        {
            get => selectedExecutor;
            set
            {
                selectedExecutor = value;
                OnPropertyChanged();
                OnPropertyChanged(nameof(CreateOrderCommand));
            }
        }

        // Ширина груза
        public double Width
        {
            get => width;
            set
            {
                width = value;
                OnPropertyChanged();
                OnPropertyChanged(nameof(CreateOrderCommand));
            }
        }

        // Высота груза
        public double Height
        {
            get => height;
            set
            {
                height = value;
                OnPropertyChanged();
                OnPropertyChanged(nameof(CreateOrderCommand));
            }
        }

        // Глубина груза
        public double Depth
        {
            get => depth;
            set
            {
                depth = value;
                OnPropertyChanged();
                OnPropertyChanged(nameof(CreateOrderCommand));
            }
        }

        // Вес груза
        public double Weight
        {
            get => weight;
            set
            {
                weight = value;
                OnPropertyChanged();
                OnPropertyChanged(nameof(CreateOrderCommand));
            }
        }

        // Дата создания заявки
        public DateTime? CreatedDate
        {
            get => createdDate;
            set
            {
                createdDate = value;
                OnPropertyChanged();
                OnPropertyChanged(nameof(CreateOrderCommand));
            }
        }

        // Команда для создания заявки
        public ICommand CreateOrderCommand { get; }

        // Метод для загрузки списка исполнителей
        private async void LoadExecutors()
        {
            try
            {
                // Получение списка исполнителей из утилиты
                Executors = await ExecutorUtils.GetExecutorsAsync();
            }
            catch (HttpRequestException ex)
            {
                // Показ сообщения об ошибке в случае неудачного запроса
                MessageBox.Show($"Не удалось загрузить исполнителей: {ex.Message}");
            }
        }

        // Метод, проверяющий возможность создания заявки
        private bool CanCreateOrder()
        {
            bool canCreate = !string.IsNullOrWhiteSpace(Description) &&
                             !string.IsNullOrWhiteSpace(PickupAddress) &&
                             !string.IsNullOrWhiteSpace(DeliveryAddress) &&
                             SelectedExecutor != null &&
                             Width > 0 && Height > 0 && Depth > 0 && Weight > 0 && CreatedDate.HasValue;

            // Для отладки
            Console.WriteLine($"CanCreateOrder: {canCreate}");
            return canCreate;
        }

        // Метод для создания заявки
        private async Task CreateOrder()
        {
            // Создание объекта заявки
            var order = new Order
            {
                Description = Description,
                PickupAddress = PickupAddress,
                DeliveryAddress = DeliveryAddress,
                Comment = Comment,
                CreatedDate = CreatedDate.GetValueOrDefault(),
                UpdatedDate = DateTime.Now,
                Executor = SelectedExecutor.Name,
                Width = Width,
                Height = Height,
                Depth = Depth,
                Weight = Weight
            };

            try
            {
                // Отправка POST-запроса на создание заявки с использованием HttpClientUtils
                var response = await HttpClientUtils.SendRequestAsync(HttpMethod.Post, "https://localhost:5001/api/orders", order);
                if (response.IsSuccessStatusCode)
                {
                    // Успешное создание заявки
                    MessageBox.Show("Заявка успешно создана.");
                }
                else
                {
                    // Ошибка при создании заявки
                    MessageBox.Show($"Не удалось создать заявку. Код статуса: {response.StatusCode}");
                }
            }
            catch (HttpRequestException ex)
            {
                // Обработка исключения при ошибке запроса
                MessageBox.Show($"Ошибка запроса: {ex.Message}");
            }
        }

        // Метод, вызываемый при изменении свойства
        protected void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));

            // Проверяем, если изменилось свойство, влияющее на возможность создания заявки
            if (propertyName == nameof(Description) ||
                propertyName == nameof(PickupAddress) ||
                propertyName == nameof(DeliveryAddress) ||
                propertyName == nameof(SelectedExecutor) ||
                propertyName == nameof(Width) ||
                propertyName == nameof(Height) ||
                propertyName == nameof(Depth) ||
                propertyName == nameof(Weight) ||
                propertyName == nameof(CreatedDate))
            {
                // Обновляем состояние команды создания заявки
                ((RelayCommand)CreateOrderCommand).RaiseCanExecuteChanged();
            }
        }
    }
}
