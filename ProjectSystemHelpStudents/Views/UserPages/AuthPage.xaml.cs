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

            // Если есть сохранённые учётки и стоит RememberMe — пытаемся автологин
            if (Properties.Settings.Default.RememberMe
                && !string.IsNullOrEmpty(Properties.Settings.Default.SavedLogin)
                && !string.IsNullOrEmpty(Properties.Settings.Default.SavedPasswordHash))
            {
                txbLogin.Text = Properties.Settings.Default.SavedLogin;
                // вставляем хэш в скрытое поле, чтобы сразу вызвать AttemptLogin:
                AttemptLogin(Properties.Settings.Default.SavedPasswordHash, isHashed: true);
            }
        }

        private void btnLogIn_Click(object sender, RoutedEventArgs e)
        {
            string password = psbPassword.Visibility == Visibility.Visible
                  ? psbPassword.Password
                  : txbPassword.Text;

            if (string.IsNullOrWhiteSpace(password))
            {
                MessageBox.Show("Вы не ввели пароль пользователя");
                logCount++;
                return;
            }

            if (string.IsNullOrWhiteSpace(txbLogin.Text))
            {
                MessageBox.Show("Вы не ввели логин пользователя");
                logCount++;
                return;
            }

            if (string.IsNullOrWhiteSpace(psbPassword.Password))
            {
                MessageBox.Show("Вы не ввели пароль пользователя");
                logCount++;
                return;
            }

            if (logCount >= 4)
            {
                if (CaptchaPanel.Visibility != Visibility.Visible)
                {
                    MessageBox.Show("Вы превысили лимит попыток. Подтвердите, что вы не робот.");
                    GenerateCaptcha();
                    CaptchaPanel.Visibility = Visibility.Visible;
                    return;
                }

                // Проверяем ответ на капчу
                if (!int.TryParse(CaptchaAnswer.Text, out int userAnswer)
                    || userAnswer != correctCaptchaAnswer)
                {
                    MessageBox.Show("Капча введена неверно.");
                    return;
                }
            }

            AttemptLogin(password, isHashed: false);
        }

        private void AttemptLogin(string pwdOrHash, bool isHashed)
        {
            // Получаем хэш пароля, если пришёл не хэш
            string hashed = isHashed
                           ? pwdOrHash
                           : PasswordHelper.HashPassword(pwdOrHash);

            // Ищем пользователя
            var user = DBClass.entities.Users
                .FirstOrDefault(u => u.Login == txbLogin.Text && u.Password == hashed);

            if (user == null)
            {
                MessageBox.Show("Неверный логин или пароль");
                logCount++;
                return;
            }

            // Сохраняем в сессии
            UserSession.IdUser = user.IdUser;
            UserSession.NotifyUserNameUpdated(user.Name);

            // Обрабатываем Remember Me
            if (chkRememberMe.IsChecked == true && !isHashed)
            {
                Properties.Settings.Default.RememberMe = true;
                Properties.Settings.Default.SavedLogin = user.Login;
                Properties.Settings.Default.SavedPasswordHash = hashed;
                Properties.Settings.Default.Save();
            }
            else
            {
                Properties.Settings.Default.RememberMe = false;
                Properties.Settings.Default.SavedLogin = "";
                Properties.Settings.Default.SavedPasswordHash = "";
                Properties.Settings.Default.Save();
            }

            // Навигация далее
            var mainWin = Application.Current.MainWindow as MainWindow;
            if (mainWin == null) return;

            // Блок смены пароля
            if (user.MustChangePassword.GetValueOrDefault())
            {
                mainWin.frmAuth.Content = null;
                mainWin.frmContentUser.Content = new UserPage(true);
                mainWin.frmStackPanelButton.Content = new StackPanelButtonPage();
                return;
            }

            // Показываем UI в зависимости от роли
            if (user.RoleUser == 1)
            {
                MessageBox.Show("Здравствуйте, " + UserSession.NameUser);
                mainWin.ShowAdminUI();
            }
            else if (user.RoleUser == 2)
            {
                MessageBox.Show("Здравствуйте, " + UserSession.NameUser);
                mainWin.ShowUserUI();
            }
        }

        private void GenerateCaptcha()
        {
            var rnd = new Random();
            int a = rnd.Next(1, 10), b = rnd.Next(1, 10);
            correctCaptchaAnswer = a + b;
            CaptchaQuestion.Text = $"{a} + {b} = ?";
        }

        private void btnReg_Click(object sender, RoutedEventArgs e)
        {
            RegPage reg = new RegPage();
            FrmClass.frmAuth.Content = reg;
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

        private void btnFogot_Click(object sender, RoutedEventArgs e)
        {
            FogotPassWindow fog = new FogotPassWindow();
            fog.Show();
        }
    }
}
