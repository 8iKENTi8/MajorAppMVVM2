using MajorAppMVVM2.Logging;
using MajorAppMVVM2.Models;
using MajorAppMVVM2.Utils;
using MajorAppMVVM2.Windows;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Net.Http;
using System.Runtime.CompilerServices;
using System.Text.Json;
using System.Threading.Tasks;
using System.Windows.Input;

namespace MajorAppMVVM2.ViewModels
{
    public class EditOrderViewModel
    {
        private static readonly HttpClient client = new HttpClient();
        private readonly StatusChangeLogger _statusChangeLogger;
        private Order _order;
        private string _errorMessage;

        // Конструктор по умолчанию
        public EditOrderViewModel()
        {
            _statusChangeLogger = new StatusChangeLogger("status_change.log");
            
            _order = new Order();
            _order.AttachLogger(_statusChangeLogger);

        }

        public EditOrderViewModel(Order order)
        {
            _order = order;
            _statusChangeLogger = new StatusChangeLogger("status_change.log");
            _order.AttachLogger(_statusChangeLogger);

        }
    }
}