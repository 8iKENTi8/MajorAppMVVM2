using System.Net.Http;
using System.Text.Json;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System;


namespace MajorAppMVVM2.Utils
{
    public static class HttpClientUtils
    {
        private static readonly HttpClient client = new HttpClient();

        public static async Task<HttpResponseMessage> SendRequestAsync(HttpMethod method, string url, object data = null)
        {
            try
            {
                HttpRequestMessage request = new HttpRequestMessage(method, url);
                if (data != null)
                {
                    var json = JsonSerializer.Serialize(data);
                    request.Content = new StringContent(json, Encoding.UTF8, "application/json");
                }

                return await client.SendAsync(request);
            }
            catch (HttpRequestException ex)
            {
                MessageBox.Show($"Ошибка HTTP запроса: {ex.Message}");
                return null;
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Произошла ошибка: {ex.Message}");
                return null;
            }
        }
    }
}