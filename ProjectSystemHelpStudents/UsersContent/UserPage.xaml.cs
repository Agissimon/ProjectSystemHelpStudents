using ProjectSystemHelpStudents.Helper;
using System.Windows;
using System.Windows.Controls;

namespace ProjectSystemHelpStudents.UsersContent
{
    public partial class UserPage : Page
    {
        public UserPage()
        {
            InitializeComponent();
            LoadUserData();
        }

        private void LoadUserData()
        {
            // Загружаем данные пользователя из UserSession
            UserNameTextBox.Text = UserSession.NameUser;
        }

        private void SaveButton_Click(object sender, RoutedEventArgs e)
        {
            // Сохранение данных пользователя в UserSession
            UserSession.NameUser = UserNameTextBox.Text;
            MessageBox.Show($"Имя пользователя сохранено: {UserSession.NameUser}");
        }
    }
}
