using System.Windows;
using MajorAppMVVM2.ViewModels;

namespace MajorAppMVVM2
{
    public partial class OrdersWindow : Window
    {
        public OrdersWindow()
        {
            InitializeComponent();
            DataContext = new OrdersViewModel(this);
        }
    }
}
