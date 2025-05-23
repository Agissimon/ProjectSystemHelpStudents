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
        }

        private void btnLogIn_Click(object sender, RoutedEventArgs e)
        {
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

            AttemptLogin();
        }

        private void AttemptLogin()
        {
            string hashedPassword = PasswordHelper.HashPassword(psbPassword.Password);

            var user = DBClass.entities.Users
                .FirstOrDefault(i => i.Login == txbLogin.Text && i.Password == hashedPassword);

            if (user == null)
            {
                MessageBox.Show("Неверный логин или пароль");
                logCount++;
                return;
            }

            if (user.MustChangePassword.GetValueOrDefault())
            {
                UserSession.IdUser = user.IdUser;
                UserSession.NotifyUserNameUpdated(user.Name);

                var mainWin = Application.Current.MainWindow as MainWindow;
                   if (mainWin != null)
                   {
                    mainWin.frmAuth.Content = null;
                          
                    mainWin.frmContentUser.Content = new UserPage(true);

                    mainWin.frmStackPanelButton.Content = new StackPanelButtonPage();
                   }
                else
                {
                    this.NavigationService?.Navigate(new UserPage());
                }

                return;
            }
            if (user.RoleUser == 1)
            {
                UserSession.IdUser = user.IdUser;
                UserSession.NotifyUserNameUpdated(user.Name);
                MessageBox.Show("Здравствуйте, " + UserSession.NameUser);

                var mainWindow = Application.Current.MainWindow as MainWindow;
                if (mainWindow != null)
                {
                    mainWindow.frmAuth.Content = null;
                    mainWindow.frmContentUser.Content = null;
                    mainWindow.frmStackPanelButton.Content = new AdminNavigationPage();
                }
            }
            if (user.RoleUser == 2)
            {
                UserSession.IdUser = user.IdUser;
                UserSession.NotifyUserNameUpdated(user.Name);
                MessageBox.Show("Здравствуйте, " + UserSession.NameUser);

                var mainWindow = Application.Current.MainWindow as MainWindow;
                if (mainWindow != null)
                {
                    mainWindow.frmAuth.Content = null;
                    mainWindow.frmContentUser.Content = new UpcomingTasksPage();
                    mainWindow.frmStackPanelButton.Content = new StackPanelButtonPage();
                }
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
