using ProjectSystemHelpStudents.Helper;
using System;
using System.Linq;
using System.Windows;
using System.Windows.Controls;

namespace ProjectSystemHelpStudents.UsersContent
{
    public partial class UserPage : Page
    {
        private ProjectSystemHelpStudents.Users currentUser;
        private bool _forcedChange;

        public UserPage(bool forcedChange = false)
        {
            InitializeComponent();
            _forcedChange = forcedChange;
            LoadUserData();
        }

        private void LoadUserData()
        {
            int userId = UserSession.IdUser;
            var user = DBClass.entities.Users.FirstOrDefault(u => u.IdUser == userId);
            if (user == null)
            {
                MessageBox.Show("Ошибка: Пользователь не найден.");
                return;
            }

            currentUser = user;
            UserNameTextBox.Text = user.Name;
            UserSurnameTextBox.Text = user.Surname;
            UserPatronymicTextBox.Text = user.Patronymic;
            UserEmailTextBox.Text = user.Mail;
        }

        private void SaveButton_Click(object sender, RoutedEventArgs e)
        {
            currentUser.Name = UserNameTextBox.Text.Trim();
            currentUser.Surname = UserSurnameTextBox.Text.Trim();
            currentUser.Patronymic = UserPatronymicTextBox.Text.Trim();
            currentUser.Mail = UserEmailTextBox.Text.Trim();

            string newPwd = NewPasswordBox.Visibility == Visibility.Visible
                ? NewPasswordBox.Password
                : NewPasswordTextBox.Text;
            string confirmPwd = ConfirmPasswordBox.Visibility == Visibility.Visible
                ? ConfirmPasswordBox.Password
                : ConfirmPasswordTextBox.Text;

            if (!string.IsNullOrWhiteSpace(newPwd))
            {
                if (newPwd != confirmPwd)
                {
                    MessageBox.Show("Пароли не совпадают.");
                    return;
                }

                currentUser.Password = PasswordHelper.HashPassword(newPwd);
                currentUser.MustChangePassword = false;
            }

            DBClass.entities.SaveChanges();
            UserSession.NotifyUserNameUpdated(currentUser.Name);
            MessageBox.Show("Данные успешно сохранены.");

            if (_forcedChange)
            {
                var main = Application.Current.MainWindow as MainWindow;
                if (main != null)
                {
                    main.frmAuth.Content = null;
                    main.frmContentUser.Content = new UpcomingTasksPage();
                    main.frmStackPanelButton.Content = new StackPanelButtonPage();
                }
            }
        }

        private void NewPwdToggle_Checked(object sender, RoutedEventArgs e)
        {
            NewPasswordTextBox.Text = NewPasswordBox.Password;
            NewPasswordBox.Visibility = Visibility.Collapsed;
            NewPasswordTextBox.Visibility = Visibility.Visible;
        }

        private void NewPwdToggle_Unchecked(object sender, RoutedEventArgs e)
        {
            NewPasswordBox.Password = NewPasswordTextBox.Text;
            NewPasswordBox.Visibility = Visibility.Visible;
            NewPasswordTextBox.Visibility = Visibility.Collapsed;
        }

        private void NewPasswordBox_PasswordChanged(object sender, RoutedEventArgs e)
        {
            if (NewPwdToggle.IsChecked == true)
                NewPasswordTextBox.Text = NewPasswordBox.Password;
        }

        private void NewPasswordTextBox_TextChanged(object sender, TextChangedEventArgs e)
        {
            if (NewPwdToggle.IsChecked == false)
                NewPasswordBox.Password = NewPasswordTextBox.Text;
        }

        private void ConfirmPwdToggle_Checked(object sender, RoutedEventArgs e)
        {
            ConfirmPasswordTextBox.Text = ConfirmPasswordBox.Password;
            ConfirmPasswordBox.Visibility = Visibility.Collapsed;
            ConfirmPasswordTextBox.Visibility = Visibility.Visible;
        }

        private void ConfirmPwdToggle_Unchecked(object sender, RoutedEventArgs e)
        {
            ConfirmPasswordBox.Password = ConfirmPasswordTextBox.Text;
            ConfirmPasswordBox.Visibility = Visibility.Visible;
            ConfirmPasswordTextBox.Visibility = Visibility.Collapsed;
        }

        private void ConfirmPasswordBox_PasswordChanged(object sender, RoutedEventArgs e)
        {
            if (ConfirmPwdToggle.IsChecked == true)
                ConfirmPasswordTextBox.Text = ConfirmPasswordBox.Password;
        }

        private void ConfirmPasswordTextBox_TextChanged(object sender, TextChangedEventArgs e)
        {
            if (ConfirmPwdToggle.IsChecked == false)
                ConfirmPasswordBox.Password = ConfirmPasswordTextBox.Text;
        }
    }
}