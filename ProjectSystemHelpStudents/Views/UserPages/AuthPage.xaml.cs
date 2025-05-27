using ProjectSystemHelpStudents.Helper;
using ProjectSystemHelpStudents.Views.AdminPages;
using System;
using System.Linq;
using System.Windows;
using System.Windows.Controls;

namespace ProjectSystemHelpStudents.UsersContent
{
    public partial class AuthPage : Page
    {
        int logCount = 0;
        private int correctCaptchaAnswer;

        public AuthPage()
        {
            InitializeComponent();
            this.Loaded += AuthPage_Loaded;
        }

        private void AuthPage_Loaded(object sender, RoutedEventArgs e)
        {
            this.Loaded -= AuthPage_Loaded;

            // Грузим настройки
            Properties.Settings.Default.Reload();

            // Если есть валидный RememberMe — автологинимся
            if (Properties.Settings.Default.RememberMe
                && !string.IsNullOrEmpty(Properties.Settings.Default.SavedLogin)
                && !string.IsNullOrEmpty(Properties.Settings.Default.SavedPasswordHash))
            {
                txbLogin.Text = Properties.Settings.Default.SavedLogin;
                AttemptLogin(Properties.Settings.Default.SavedPasswordHash, isHashed: true);
                return;
            }

            // Иначе — показываем форму с пустыми полями
            txbLogin.Text = "";
            psbPassword.Password = "";
            chkRememberMe.IsChecked = false;

            // Подписываемся на изменения чекбокса:
            chkRememberMe.Checked += (s, a) =>
            {
                // ничего пока не сохраняем
            };
            chkRememberMe.Unchecked += (s, a) =>
            {
                // если пользователь снял галку до входа — сразу очистим старые данные
                Properties.Settings.Default.RememberMe = false;
                Properties.Settings.Default.SavedLogin = "";
                Properties.Settings.Default.SavedPasswordHash = "";
                Properties.Settings.Default.Save();
            };
        }

        private void btnLogIn_Click(object sender, RoutedEventArgs e)
        {
            string login = txbLogin.Text.Trim();
            string password = (psbPassword.Visibility == Visibility.Visible)
                ? psbPassword.Password
                : txbPassword.Text;

            if (string.IsNullOrEmpty(login))
            {
                MessageBox.Show("Введите логин");
                logCount++;
                return;
            }
            if (string.IsNullOrEmpty(password))
            {
                MessageBox.Show("Введите пароль");
                logCount++;
                return;
            }

            if (logCount >= 4 && CaptchaPanel.Visibility != Visibility.Visible)
            {
                GenerateCaptcha();
                CaptchaPanel.Visibility = Visibility.Visible;
                MessageBox.Show("Подтвердите, что вы не робот");
                return;
            }

            if (CaptchaPanel.Visibility == Visibility.Visible)
            {
                if (!int.TryParse(CaptchaAnswer.Text, out int ans) || ans != correctCaptchaAnswer)
                {
                    MessageBox.Show("Капча введена неверно");
                    return;
                }
            }

            // всегда ручной вход
            AttemptLogin(password, isHashed: false);
        }

        public void AttemptLogin(string pwdOrHash, bool isHashed)
        {
            string hashed = isHashed
                ? pwdOrHash
                : PasswordHelper.HashPassword(pwdOrHash);

            var user = DBClass.entities.Users
                .FirstOrDefault(u => u.Login == txbLogin.Text && u.Password == hashed);

            if (user == null)
            {
                MessageBox.Show("Неверный логин или пароль");
                logCount++;
                return;
            }

            // Сессия
            UserSession.IdUser = user.IdUser;
            UserSession.NotifyUserNameUpdated(user.Name);

            if (!isHashed)
            {
                if (chkRememberMe.IsChecked == true)
                {
                    // сохраняем
                    Properties.Settings.Default.RememberMe = true;
                    Properties.Settings.Default.SavedLogin = user.Login;
                    Properties.Settings.Default.SavedPasswordHash = hashed;
                }
                else
                {
                    // сбрасываем
                    Properties.Settings.Default.RememberMe = false;
                    Properties.Settings.Default.SavedLogin = "";
                    Properties.Settings.Default.SavedPasswordHash = "";
                }
                Properties.Settings.Default.Save();
            }

            var mw = Application.Current.MainWindow as MainWindow;
            if (mw == null) return;

            if (user.MustChangePassword.GetValueOrDefault())
            {
                mw.frmAuth.Content = null;
                mw.frmContentUser.Content = new UserPage(true);
                mw.frmStackPanelButton.Content = new StackPanelButtonPage();
                return;
            }

            MessageBox.Show($"Здравствуйте, {UserSession.NameUser}");
            if (user.RoleUser == 1)
                mw.ShowAdminUI();
            else
                mw.ShowUserUI();
        }

        private void GenerateCaptcha()
        {
            var rnd = new Random();
            int a = rnd.Next(1, 10), b = rnd.Next(1, 10);
            correctCaptchaAnswer = a + b;
            CaptchaQuestion.Text = $"{a} + {b} = ?";
        }

        private void btnReg_Click(object sender, RoutedEventArgs e) =>
            FrmClass.frmAuth.Content = new RegPage();

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

        private void btnFogot_Click(object sender, RoutedEventArgs e) =>
            new FogotPassWindow().Show();
    }
}
