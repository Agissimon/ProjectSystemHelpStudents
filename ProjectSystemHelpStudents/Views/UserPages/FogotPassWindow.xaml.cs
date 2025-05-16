using ProjectSystemHelpStudents.Helper;
using System;
using System.Configuration;
using System.Linq;
using System.Net;
using System.Net.Mail;
using System.Windows;

namespace ProjectSystemHelpStudents.UsersContent
{
    public partial class FogotPassWindow : Window
    {
        public FogotPassWindow()
        {
            InitializeComponent();
            WindowStartupLocation = WindowStartupLocation.CenterScreen;
            ResizeMode = ResizeMode.NoResize;
            WindowStyle = WindowStyle.SingleBorderWindow;
        }

        private void btnMail_Click(object sender, RoutedEventArgs e)
        {
            string rawEmail = txbForPass.Text;
            if (string.IsNullOrWhiteSpace(rawEmail) || !rawEmail.Contains("@"))
            {
                MessageBox.Show("Введите корректный email.");
                return;
            }
            string clientEmail = rawEmail.Trim().ToLower();

            int cooldownMinutes;
            if (!int.TryParse(ConfigurationManager.AppSettings["ResetCooldownMinutes"], out cooldownMinutes))
                cooldownMinutes = 15;
            TimeSpan resetCooldown = TimeSpan.FromMinutes(cooldownMinutes);

            string fromEmail = ConfigurationManager.AppSettings["EmailFrom"];
            string fromPassword = ConfigurationManager.AppSettings["EmailPassword"];
            if (string.IsNullOrWhiteSpace(fromEmail) || string.IsNullOrWhiteSpace(fromPassword))
            {
                MessageBox.Show("Email отправителя или пароль не настроены в App.config.");
                return;
            }

            using (var context = new TaskManagementEntities1())
            {
                var user = context.Users
                    .FirstOrDefault(u =>
                        u.Mail != null &&
                        u.Mail.Trim().ToLower() == clientEmail);

                if (user == null)
                {
                    MessageBox.Show("Пользователь с такой электронной почтой не найден.");
                    return;
                }

                if (user.LastPasswordResetRequest.HasValue &&
                    DateTime.Now - user.LastPasswordResetRequest.Value < resetCooldown)
                {
                    DateTime nextAllowed = user.LastPasswordResetRequest.Value.Add(resetCooldown);
                    MessageBox.Show(
                        "Новый временный пароль можно запросить не раньше, чем в " +
                        nextAllowed.ToString("HH:mm:ss") + ".");
                    return;
                }

                // Генерация и сохранение временного пароля
                string tempPassword = GenerateTemporaryPassword();
                user.Password = PasswordHelper.HashPassword(tempPassword);
                user.MustChangePassword = true;
                user.LastPasswordResetRequest = DateTime.Now;
                context.SaveChanges();

                SmtpClient smtp = new SmtpClient("smtp.gmail.com", 587)
                {
                    EnableSsl = true,
                    Credentials = new NetworkCredential(fromEmail, fromPassword)
                };

                MailMessage msg = new MailMessage
                {
                    From = new MailAddress(fromEmail),
                    Subject = "Восстановление пароля",
                    Body = "Ваш временный пароль: " + tempPassword + Environment.NewLine +
                                 "Он действителен в течение " + cooldownMinutes +
                                 " минут. Пожалуйста, смените его после входа.",
                    IsBodyHtml = false
                };
                msg.To.Add(user.Mail.Trim());

                try
                {
                    smtp.Send(msg);
                    MessageBox.Show("Временный пароль был отправлен на ваш почтовый ящик.");
                    this.Close();
                }
                catch (Exception ex)
                {
                    MessageBox.Show("Ошибка при отправке письма: " + ex.Message);
                }
            }
        }

        private string GenerateTemporaryPassword()
        {
            const string chars = "ABCDEFGHIJKLMNOPQRSTUVWXYZabcdefghijklmnopqrstuvwxyz0123456789";
            Random rnd = new Random();
            char[] buffer = new char[8];
            for (int i = 0; i < buffer.Length; i++)
            {
                buffer[i] = chars[rnd.Next(chars.Length)];
            }
            return new string(buffer);
        }
    }
}
