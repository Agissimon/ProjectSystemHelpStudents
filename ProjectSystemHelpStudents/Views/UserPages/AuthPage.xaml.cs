using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.RightsManagement;
using System.Text;
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
using ProjectSystemHelpStudents.Helper;

namespace ProjectSystemHelpStudents.UsersContent
{
    /// <summary>
    /// Логика взаимодействия для AuthPage.xaml
    /// </summary>
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
            }
            else if (string.IsNullOrWhiteSpace(psbPassword.Password))
            {
                MessageBox.Show("Вы не ввели пароль пользователя");
                logCount++;
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

                if (int.TryParse(CaptchaAnswer.Text, out int userAnswer) && userAnswer == correctCaptchaAnswer)
                {
                    AttemptLogin();
                }
                else
                {
                    MessageBox.Show("Капча введена неверно.");
                }
            }
            else
            {
                AttemptLogin();
            }
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
            }
            else
            {
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
        }

        private void GenerateCaptcha()
        {
            Random rnd = new Random();
            int a = rnd.Next(1, 10);
            int b = rnd.Next(1, 10);
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
