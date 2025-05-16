using ProjectSystemHelpStudents.Helper;
using System;
using System.Linq;
using System.Text.RegularExpressions;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;

namespace ProjectSystemHelpStudents.UsersContent
{
    public partial class RegPage : Page
    {
        private readonly Brush _errorBrush;

        public RegPage()
        {
            InitializeComponent();
            _errorBrush = (Brush)TryFindResource("ErrorBrush") ?? Brushes.Red;
        }

        private void btnLogIn_Click(object sender, RoutedEventArgs e)
        {
            string login = txbLogin.Text;
            string fio = txbName.Text;
            string password = psbPassword.Password;
            string mail = txbMail.Text;

            if (!IsValidData(login, fio, password, mail))
                return;

            try
            {
                using (var context = new TaskManagementEntities1())
                {
                    int maxUserId = context.Users.Max(u => (int?)u.IdUser) ?? 0;
                    var parts = fio.Split(' ');
                    var user = new Users
                    {
                        IdUser = maxUserId + 1,
                        Login = login,
                        Name = parts.ElementAtOrDefault(0) ?? "",
                        Surname = parts.ElementAtOrDefault(1) ?? "",
                        Patronymic = parts.ElementAtOrDefault(2) ?? "",
                        Password = PasswordHelper.HashPassword(password),
                        RoleUser = 2,
                        Mail = mail
                    };
                    context.Users.Add(user);
                    context.SaveChanges();
                }
                MessageBox.Show("Пользователь успешно зарегистрирован.", "Успех", MessageBoxButton.OK, MessageBoxImage.Information);
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка при добавлении пользователя: {ex.Message}", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private bool IsValidData(string login, string name, string password, string mail)
        {
            ClearErrorBorder(txbLogin);
            ClearErrorBorder(psbPassword);
            ClearErrorBorder(txbPassword);
            ClearErrorBorder(txbName);
            ClearErrorBorder(txbMail);

            if (string.IsNullOrWhiteSpace(login) ||
                string.IsNullOrWhiteSpace(name) ||
                string.IsNullOrWhiteSpace(password) ||
                string.IsNullOrWhiteSpace(mail))
            {
                MessageBox.Show("Заполните все поля.", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Warning);
                HighlightEmptyFields();
                return false;
            }

            if (!Regex.IsMatch(login, @"^[a-zA-Z0-9]+$"))
            {
                MessageBox.Show("Логин должен содержать только английские буквы и цифры.", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Warning);
                SetErrorBorder(txbLogin);
                return false;
            }

            if (password.Length < 6)
            {
                MessageBox.Show("Пароль должен содержать не менее 6 символов.", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Warning);
                SetErrorBorder(psbPassword);
                SetErrorBorder(txbPassword);
                return false;
            }

            if (!Regex.IsMatch(mail, @"^[^@\s]+@[^@\s]+\.[^@\s]+$"))
            {
                MessageBox.Show("Введите корректный email.", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Warning);
                SetErrorBorder(txbMail);
                return false;
            }

            return true;
        }

        private void HighlightEmptyFields()
        {
            if (string.IsNullOrWhiteSpace(txbLogin.Text)) SetErrorBorder(txbLogin);
            if (string.IsNullOrWhiteSpace(psbPassword.Password)) SetErrorBorder(psbPassword);
            if (string.IsNullOrWhiteSpace(txbPassword.Text)) SetErrorBorder(txbPassword);
            if (string.IsNullOrWhiteSpace(txbName.Text)) SetErrorBorder(txbName);
            if (string.IsNullOrWhiteSpace(txbMail.Text)) SetErrorBorder(txbMail);
        }

        private void SetErrorBorder(Control ctl)
        {
            ctl.BorderBrush = _errorBrush;
            ctl.BorderThickness = new Thickness(1.5);
        }

        private void ClearErrorBorder(Control ctl)
        {
            ctl.ClearValue(Control.BorderBrushProperty);
            ctl.ClearValue(Control.BorderThicknessProperty);
        }

        private void txbLogin_TextChanged(object sender, TextChangedEventArgs e)
        {
            if (Regex.IsMatch(txbLogin.Text, @"^[a-zA-Z0-9]+$"))
                ClearErrorBorder(txbLogin);
        }

        private void txbName_TextChanged(object sender, TextChangedEventArgs e)
        {
            if (!string.IsNullOrWhiteSpace(txbName.Text))
                ClearErrorBorder(txbName);
        }

        private void txbMail_TextChanged(object sender, TextChangedEventArgs e)
        {
            if (Regex.IsMatch(txbMail.Text, @"^[^@\s]+@[^@\s]+\.[^@\s]+$"))
                ClearErrorBorder(txbMail);
        }

        private void psbPassword_PasswordChanged(object sender, RoutedEventArgs e)
        {
            if (psbPassword.Password.Length >= 6)
                ClearErrorBorder(psbPassword);
            txbPassword.Text = psbPassword.Password;
        }

        private void txbPassword_TextChanged(object sender, TextChangedEventArgs e)
        {
            if (txbPassword.Text.Length >= 6)
                ClearErrorBorder(txbPassword);
            psbPassword.Password = txbPassword.Text;
        }

        private void chkShowPassword_Checked(object sender, RoutedEventArgs e)
        {
            txbPassword.Text = psbPassword.Password;
            psbPassword.Visibility = Visibility.Collapsed;
            txbPassword.Visibility = Visibility.Visible;
        }

        private void chkShowPassword_Unchecked(object sender, RoutedEventArgs e)
        {
            psbPassword.Password = txbPassword.Text;
            psbPassword.Visibility = Visibility.Visible;
            txbPassword.Visibility = Visibility.Collapsed;
        }

        private void btnBack_Click(object sender, RoutedEventArgs e)
        {
            AuthPage authPage = new AuthPage();
            FrmClass.frmAuth.Content = authPage;
        }
    }
}
