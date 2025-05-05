using ProjectSystemHelpStudents.Helper;
using System;
using System.Linq;
using System.Windows;
using System.Windows.Controls;

namespace ProjectSystemHelpStudents.UsersContent
{
    public partial class UserPage : Page
    {
        public delegate void UserNameUpdatedHandler(string newName);

        private User currentUser;

        public UserPage()
        {
            InitializeComponent();
            LoadUserData();
        }

        private void LoadUserData()
        {
            int userId = UserSession.IdUser;
            var user = DBClass.entities.Users.FirstOrDefault(u => u.IdUser == userId);
            if (user != null)
            {
                currentUser = new User
                {
                    IdUser = user.IdUser,
                    Name = user.Name,
                    Surname = user.Surname,
                    Patronymic = user.Patronymic,
                    Login = user.Login,
                    Password = user.Password,
                    Mail = user.Mail
                };
                UserNameTextBox.Text = currentUser.Name;
                UserSurnameTextBox.Text = currentUser.Surname;
                UserPatronymicTextBox.Text = currentUser.Patronymic;
                UserEmailTextBox.Text = currentUser.Mail;
            }
            else
            {
                MessageBox.Show("Ошибка: Пользователь не найден.");
            }
        }

        private void SaveButton_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                if (currentUser != null)
                {
                    currentUser.Name = UserNameTextBox.Text;
                    currentUser.Surname = UserSurnameTextBox.Text;
                    currentUser.Patronymic = UserPatronymicTextBox.Text;
                    currentUser.Mail = UserEmailTextBox.Text;

                    SaveUserDataToDatabase();

                    // Передаем только имя пользователя
                    string newName = currentUser.Name;
                    UserSession.NotifyUserNameUpdated(newName);

                    MessageBox.Show("Данные пользователя успешно сохранены.");
                }
                else
                {
                    MessageBox.Show("Ошибка: Пользователь не найден.");
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Ошибка при сохранении данных пользователя: " + ex.Message);
            }
        }

        private void SaveUserDataToDatabase()
        {
            var user = DBClass.entities.Users.FirstOrDefault(u => u.IdUser == currentUser.IdUser);
            if (user != null)
            {
                user.Name = currentUser.Name;
                user.Surname = currentUser.Surname;
                user.Patronymic = currentUser.Patronymic;
                user.Mail = currentUser.Mail;
                DBClass.entities.SaveChanges();
            }
        }
    }
}
