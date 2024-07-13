using System.Windows;

namespace MajorAppMVVM2.Windows
{
    public partial class CommentInputWindow : Window
    {
        public string Comment { get; private set; }

        public CommentInputWindow()
        {
            InitializeComponent();
        }

        private void OkButton_Click(object sender, RoutedEventArgs e)
        {
            Comment = textBoxComment.Text;
            DialogResult = true;
        }
    }
}
