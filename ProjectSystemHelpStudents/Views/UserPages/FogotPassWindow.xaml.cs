using ProjectSystemHelpStudents.Helper;
using System;
using System.Linq;
using System.Net.Mail;
using System.Net;
using System.Windows;
using System.Configuration;

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
            var clientEmail = txbForPass.Text.Trim();

            if (string.IsNullOrEmpty(clientEmail) || !clientEmail.Contains("@"))
            {
                MessageBox.Show("Введите корректный email.");
                return;
            }

            var user = DBClass.entities.Users.FirstOrDefault(i => i.Mail == clientEmail);

            if (user == null)
            {
                MessageBox.Show("Пользователь с такой электронной почтой не найден.");
                return;
            }

            string fromEmail = ConfigurationManager.AppSettings["EmailFrom"];
            string fromPassword = ConfigurationManager.AppSettings["EmailPassword"];

            if (string.IsNullOrWhiteSpace(fromEmail) || string.IsNullOrWhiteSpace(fromPassword))
            {
                MessageBox.Show("Email отправителя или пароль не настроены в App.config.");
                return;
            }

            SmtpClient client = new SmtpClient("smtp.gmail.com", 587)
            {
                EnableSsl = true,
                Credentials = new NetworkCredential(fromEmail, fromPassword)
            };

            var mailMessage = new MailMessage
            {
                From = new MailAddress(fromEmail),
                Subject = "Восстановление пароля",
                Body = $"Ваш пароль: {user.Password}",
                IsBodyHtml = false
            };

            mailMessage.To.Add(clientEmail);

            try
            {
                client.Send(mailMessage);
                MessageBox.Show("Пароль был отправлен на ваш почтовый ящик.");
            }
            catch (SmtpException smtpEx)
            {
                MessageBox.Show($"Ошибка отправки письма: {smtpEx.Message}");
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Неожиданная ошибка: {ex.Message}");
            }
        }
    }
}