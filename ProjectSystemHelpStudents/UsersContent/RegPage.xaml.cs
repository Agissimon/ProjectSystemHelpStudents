using ProjectSystemHelpStudents.Helper;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace ProjectSystemHelpStudents.UsersContent
{
    /// <summary>
    /// Логика взаимодействия для RegPage.xaml
    /// </summary>
    public partial class RegPage : Page
    {
        public RegPage()
        {
            InitializeComponent();
        }

        private void btnLogIn_Click(object sender, RoutedEventArgs e)
        {
            string login = txbLogin.Text;
            string fio = txbName.Text;
            string password = psbPassword.Password;
            string mail = txbMail.Text;

            if (IsValidData(login, fio, password, mail))
            {
                try
                {
                    using (var context = new TaskManagementEntities())
                    {
                        int maxUserId = context.Users.Max(u => (int?)u.IdUser) ?? 0;

                        string[] fioParts = fio.Split(' ');
                        string name = fioParts.Length > 0 ? fioParts[0] : "";
                        string surname = fioParts.Length > 1 ? fioParts[1] : "";
                        string patronymic = fioParts.Length > 2 ? fioParts[2] : "";

                        if (string.IsNullOrEmpty(patronymic))
                        {
                            patronymic = "";
                        }

                        Users newUser = new Users
                        {
                            IdUser = maxUserId + 1,
                            Login = login,
                            Name = name,
                            Surname = surname,
                            Patronymic = patronymic,
                            Password = password,
                            RoleUser = 2,
                            Mail = mail
                        };

                        context.Users.Add(newUser);

                        context.SaveChanges();

                        MessageBox.Show("Пользователь успешно зарегистрирован.");
                    }
                }
                catch (Exception ex)
                {
                    MessageBox.Show($"Ошибка при добавлении пользователя: {ex.Message}");
                }
            }
        }

        private bool IsValidData(string login, string name, string password, string mail)
        {
            if (string.IsNullOrWhiteSpace(login) ||
                string.IsNullOrWhiteSpace(name) ||
                string.IsNullOrWhiteSpace(password) ||
                string.IsNullOrWhiteSpace(mail))
            {
                MessageBox.Show("Заполните все поля.");
                return false;
            }

            if (!Regex.IsMatch(login, @"^[a-zA-Z0-9]+$"))
            {
                MessageBox.Show("Логин должен содержать только английские буквы и цифры.");
                return false;
            }

            if (password.Length < 6)
            {
                MessageBox.Show("Пароль должен содержать не менее 6 символов.");
                return false;
            }

            if (!Regex.IsMatch(mail, @"^[^@\s]+@[^@\s]+\.[^@\s]+$"))
            {
                MessageBox.Show("Введите корректный email.");
                return false;
            }

            return true;
        }

        private void btnBack_Click(object sender, RoutedEventArgs e)
        {
            AuthPage authPage = new AuthPage();
            FrmClass.frmAuth.Content = authPage;
        }

        private void chkShowPassword_Checked(object sender, RoutedEventArgs e)
        {
            if (txbPassword != null)
            {
                txbPassword.Text = psbPassword.Password;
                psbPassword.Visibility = Visibility.Collapsed;
                txbPassword.Visibility = Visibility.Visible;
            }
        }

        private void chkShowPassword_Unchecked(object sender, RoutedEventArgs e)
        {
            if (txbPassword != null)
            {
                psbPassword.Password = txbPassword.Text;
                psbPassword.Visibility = Visibility.Visible;
                txbPassword.Visibility = Visibility.Collapsed;
            }
        }

        private void txbLogin_TextChanged(object sender, TextChangedEventArgs e)
        {
            ValidateLogin();
        }

        private void txbName_TextChanged(object sender, TextChangedEventArgs e)
        {
            ValidateName();
        }

        private void txbMail_TextChanged(object sender, TextChangedEventArgs e)
        {
            ValidateEmail();
        }

        private void ValidateLogin()
        {
            if (!Regex.IsMatch(txbLogin.Text, @"^[a-zA-Z0-9]+$"))
            {
                txbLogin.Background = Brushes.LightPink;
            }
            else
            {
                txbLogin.Background = Brushes.White;
            }
        }

        private void ValidateName()
        {
            if (string.IsNullOrWhiteSpace(txbName.Text))
            {
                txbName.Background = Brushes.LightPink;
            }
            else
            {
                txbName.Background = Brushes.White;
            }
        }

        private void ValidateEmail()
        {
            if (!Regex.IsMatch(txbMail.Text, @"^[^@\s]+@[^@\s]+\.[^@\s]+$"))
            {
                txbMail.Background = Brushes.LightPink;
            }
            else
            {
                txbMail.Background = Brushes.White;
            }
        }
    }
}
