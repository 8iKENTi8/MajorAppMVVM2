using System.Windows.Controls;

namespace MajorAppMVVM2.Utils
{
    public static class ValidationUtils
    {
        public static bool ValidateOrderDetails(TextBox descriptionTextBox, TextBox pickupAddressTextBox, TextBox deliveryAddressTextBox, ComboBox executorComboBox,
                                                TextBox widthTextBox, TextBox heightTextBox, TextBox depthTextBox, TextBox weightTextBox,
                                                DatePicker createdDatePicker, out string errorMessage)
        {
            errorMessage = string.Empty;

            if (createdDatePicker != null && createdDatePicker.SelectedDate == null)
            {
                errorMessage = "Дата создания должна быть выбрана.";
                return false;
            }

            if (string.IsNullOrWhiteSpace(descriptionTextBox.Text) ||
                string.IsNullOrWhiteSpace(pickupAddressTextBox.Text) ||
                string.IsNullOrWhiteSpace(deliveryAddressTextBox.Text) ||
                executorComboBox.SelectedItem == null)  // Проверка на выбранного исполнителя
            {
                errorMessage = "Описание, адрес получения, адрес доставки и исполнитель должны быть заполнены.";
                return false;
            }

            if (!double.TryParse(widthTextBox.Text, out double width) || width <= 0 ||
                !double.TryParse(heightTextBox.Text, out double height) || height <= 0 ||
                !double.TryParse(depthTextBox.Text, out double depth) || depth <= 0 ||
                !double.TryParse(weightTextBox.Text, out double weight) || weight <= 0)
            {
                errorMessage = "Ширина, высота, глубина и вес должны быть положительными числами.";
                return false;
            }

            return true;
        }
    }
}
