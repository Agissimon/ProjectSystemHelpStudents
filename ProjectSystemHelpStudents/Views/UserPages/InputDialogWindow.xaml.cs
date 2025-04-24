using System.Windows;

namespace ProjectSystemHelpStudents
{
    public partial class InputDialog : Window
    {
        // Public properties to be set by the caller
        public string InputText { get; set; }
        public string PlaceholderText { get; set; }
        public string TitleText { get; set; }

        public InputDialog(string message, string title = "Ввод данных")
        {
            InitializeComponent();
            MessageText.Text = message;
            this.Title = title;
            this.DataContext = this;
        }


        private void OkButton_Click(object sender, RoutedEventArgs e)
        {
            if (string.IsNullOrWhiteSpace(InputText))
            {
                MessageBox.Show("Поле не может быть пустым", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
                return;
            }

            DialogResult = true;
            Close();
        }

        private void CancelButton_Click(object sender, RoutedEventArgs e)
        {
            DialogResult = false;
            Close();
        }
    }
}
