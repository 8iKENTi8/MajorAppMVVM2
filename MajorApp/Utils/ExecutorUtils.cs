using MajorAppMVVM2.Models;
using System.Collections.Generic;
using System.Net.Http;
using System.Text.Json;
using System.Threading.Tasks;

namespace MajorAppMVVM2.Utils
{
    public static class ExecutorUtils
    {
        public static async Task<List<Executor>> GetExecutorsAsync()
        {
            try
            {
                // Отправляем GET запрос на сервер для получения списка исполнителей
                var response = await HttpClientUtils.SendRequestAsync(HttpMethod.Get, "https://localhost:5001/api/orders/executors");

                // Проверяем успешность запроса
                if (response != null && response.IsSuccessStatusCode)
                {
                    var jsonResponse = await response.Content.ReadAsStringAsync();

                    // Десериализация JSON данных в список исполнителей с явно установленными параметрами
                    var executors = JsonSerializer.Deserialize<List<Executor>>(jsonResponse, new JsonSerializerOptions
                    {
                        PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
                        PropertyNameCaseInsensitive = true // Игнорировать регистр имен свойств
                    });

                    return executors ?? new List<Executor>(); // Возвращаем пустой список, если десериализация не удалась
                }
                else
                {
                    // Обработка неуспешного ответа
                    throw new HttpRequestException($"Не удалось получить список исполнителей. Код статуса: {response?.StatusCode}");
                }
            }
            catch (HttpRequestException ex)
            {
                // Обработка ошибок HTTP запросов
                throw new HttpRequestException($"Ошибка запроса: {ex.Message}", ex);
            }
        }
    }
}
