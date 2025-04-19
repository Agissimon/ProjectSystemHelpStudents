using ProjectSystemHelpStudents.Helper;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Mail;
using System.Net;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;

namespace ProjectSystemHelpStudents.UsersContent
{
    /// <summary>
    /// Логика взаимодействия для FogotPassWindow.xaml
    /// </summary>
    public partial class FogotPassWindow : System.Windows.Window
    {
        public FogotPassWindow()
        {
            InitializeComponent();
            this.WindowStartupLocation = WindowStartupLocation.CenterScreen;
            this.ResizeMode = ResizeMode.NoResize;
            this.WindowStyle = WindowStyle.SingleBorderWindow;
        }
        private void btnMail_Click(object sender, RoutedEventArgs e)
        {
            var clientEmail = txbForPass.Text;
            var user = DBClass.entities.Users.Where(i => i.Mail == clientEmail).FirstOrDefault();

            if (user == null)
            {
                MessageBox.Show("Пользователь с такой электронной почтой не найден");
                return;
            }

            SmtpClient client = new SmtpClient("smtp.mail.ru", 587)
            {
                EnableSsl = true,
                Credentials = new NetworkCredential("kozinog@mail.ru", "9iJRbHr0AZxBLrCz0rHU")
            };

            var mailMessage = new MailMessage
            {
                From = new MailAddress("kozinog@mail.ru"),
                Subject = "Восстановление пароля: Гастроном",
                Body = $"Ваш пароль: {user.Password}",
                IsBodyHtml = false
            };

            mailMessage.To.Add(clientEmail);

            try
            {
                client.Send(mailMessage);
                MessageBox.Show("Пароль был отправлен на ваш почтовый ящик");
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
            }
        }
    }
}
