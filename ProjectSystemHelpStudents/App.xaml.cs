using System;
using System.Data.Entity;                
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Mail;
using System.Threading;
using System.Windows;
using System.Windows.Resources;         
using WinForms = System.Windows.Forms;   
using Drawing = System.Drawing;         
using STT = System.Threading.Tasks.Task; 
using Model = ProjectSystemHelpStudents;
using System.Configuration;

namespace ProjectSystemHelpStudents
{
    public partial class App : Application
    {
        private WinForms.NotifyIcon _notifyIcon;
        private CancellationTokenSource _cts;
        private bool _isReallyClosing;

        protected override void OnStartup(StartupEventArgs e)
        {
            base.OnStartup(e);

            var uri = new Uri(
                "pack://application:,,,/ProjectSystemHelpStudents;component/Resources/Icon/logo001.ico",
                UriKind.Absolute);
            StreamResourceInfo sri = Application.GetResourceStream(uri);
            if (sri == null)
                throw new FileNotFoundException("Не найден ресурс logo001.ico", uri.ToString());

            // Создаём и настраиваем NotifyIcon
            _notifyIcon = new WinForms.NotifyIcon
            {
                Icon = new Drawing.Icon(sri.Stream),
                Text = "MyTask — менеджер задач",
                Visible = true
            };
            var menu = new WinForms.ContextMenuStrip();
            menu.Items.Add("Открыть", null, (s, a) => ShowMainWindow());
            menu.Items.Add("Выход", null, (s, a) => ExitApplication());
            _notifyIcon.ContextMenuStrip = menu;
            _notifyIcon.DoubleClick += (s, a) => ShowMainWindow();

            // Запускаем фоновый цикл напоминаний
            _cts = new CancellationTokenSource();
            STT.Run(() => ReminderLoop(_cts.Token));

            // Создаём и показываем главное окно, перехватывая крестик
            MainWindow = new MainWindow();
            MainWindow.Closing += MainWindow_Closing;
            MainWindow.Show();
        }

        private void MainWindow_Closing(object sender, System.ComponentModel.CancelEventArgs e)
        {
            if (!_isReallyClosing)
            {
                e.Cancel = true;
                MainWindow.Hide();
            }
        }

        private void ShowMainWindow()
        {
            if (MainWindow == null)
            {
                MainWindow = new MainWindow();
                MainWindow.Closing += MainWindow_Closing;
                MainWindow.Show();
            }
            else
            {
                MainWindow.Show();
                MainWindow.WindowState = WindowState.Normal;
                MainWindow.Activate();
            }
        }

        private void ExitApplication()
        {
            _isReallyClosing = true;
            _notifyIcon.Visible = false;
            _notifyIcon.Dispose();
            _cts.Cancel();
            Shutdown();
        }

        private async STT ReminderLoop(CancellationToken token)
        {
            while (!token.IsCancellationRequested)
            {
                try
                {
                    CheckReminders();
                }
                catch
                {
                    // TODO: логировать
                }
                await STT.Delay(TimeSpan.FromMinutes(1), token);
            }
        }

        private void CheckReminders()
        {
            using (var ctx = new Model.TaskManagementEntities1())
            {
                DateTime now = DateTime.Now;
                var due = ctx.Set<Model.Task>()
                    .Include("Status")
                    .Where(t =>
                        t.Status.Name != "Завершено"
                     && t.ReminderDate != null
                     && DbFunctions.DiffMinutes(now, t.ReminderDate) == 0)
                    .ToList();

                foreach (var t in due)
                {
                    _notifyIcon.ShowBalloonTip(
                        5000,
                        "Напоминание: " + t.Title,
                        t.Description ?? "",
                        WinForms.ToolTipIcon.Info);
                    SendEmailReminder(t);
                }
            }
        }

        private void SendEmailReminder(Model.Task t)
        {
            try
            {
                string to;
                using (var ctx = new Model.TaskManagementEntities1())
                {
                    var user = ctx.Users.Find(t.IdUser);
                    to = user?.Mail;
                }
                if (string.IsNullOrWhiteSpace(to)) return;

                // Получаем данные из App.config
                var from = ConfigurationManager.AppSettings["EmailFrom"];
                var pass = ConfigurationManager.AppSettings["EmailPassword"];

                using (var smtp = new SmtpClient("smtp.gmail.com", 587))
                {
                    smtp.Credentials = new NetworkCredential(from, pass);
                    smtp.EnableSsl = true;

                    var msg = new MailMessage(from, to)
                    {
                        Subject = $"Напоминание: {t.Title}",
                        Body = $"Ваша задача «{t.Title}» запланирована на {t.ReminderDate:dd.MM.yyyy HH:mm}.",
                        IsBodyHtml = false
                    };
                    smtp.Send(msg);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка отправки почты: {ex.Message}");
            }
        }

        protected override void OnExit(ExitEventArgs e)
        {
            _cts.Cancel();
            _notifyIcon.Dispose();
            base.OnExit(e);
        }
    }
}
