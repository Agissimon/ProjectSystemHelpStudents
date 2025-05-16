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
using Model = ProjectSystemHelpStudents;
using System.Configuration;
using ProjectSystemHelpStudents.Helper;
using System.Collections.Generic;
using System.Runtime.InteropServices;

namespace ProjectSystemHelpStudents
{
    public partial class App : Application
    {
        private static Mutex _singleInstanceMutex;
        private WinForms.NotifyIcon _notifyIcon;
        private Timer _reminderTimer;
        private Timer _dailySummaryTimer;
        private bool _isReallyClosing;
        [DllImport("user32.dll", SetLastError = true)]
        private static extern IntPtr FindWindow(string lpClassName, string lpWindowName);

        [DllImport("user32.dll")]
        private static extern bool PostMessage(IntPtr hWnd, uint Msg, IntPtr wParam, IntPtr lParam);

        private const int WM_SHOWAPP = 0x8001;

        public App()
        {
            bool isNew;
            _singleInstanceMutex = new Mutex(true, "MyTaskSingletonMutex", out isNew);
            if (!isNew)
            {
                // Найти главное окно уже запущенного экземпляра и послать ему сообщение
                IntPtr otherWnd = FindWindow(null, "MyTask");
                if (otherWnd != IntPtr.Zero)
                {
                    PostMessage(otherWnd, WM_SHOWAPP, IntPtr.Zero, IntPtr.Zero);
                }
                Environment.Exit(0);
            }

            this.DispatcherUnhandledException += App_DispatcherUnhandledException;
        }

        private void App_DispatcherUnhandledException(object sender,
            System.Windows.Threading.DispatcherUnhandledExceptionEventArgs e)
        {
            MessageBox.Show($"Ошибка: {e.Exception.Message}", "Ошибка",
                            MessageBoxButton.OK, MessageBoxImage.Error);
            e.Handled = true;
        }

        protected override void OnStartup(StartupEventArgs e)
        {
            base.OnStartup(e);

            // Настройка иконки в трее
            var uri = new Uri(
                "pack://application:,,,/ProjectSystemHelpStudents;component/Resources/Icon/logo001.ico",
                UriKind.Absolute);
            StreamResourceInfo sri = Application.GetResourceStream(uri);
            if (sri == null)
                throw new FileNotFoundException("Не найден ресурс logo001.ico", uri.ToString());

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

            // Таймер напоминаний (каждую минуту)
            _reminderTimer = new Timer(_ =>
            {
                try { CheckReminders(); }
                catch { /* логирование */ }
            },
            null,
            TimeSpan.Zero,
            TimeSpan.FromMinutes(1));

            // Таймер ежедневной сводки в 12:00
            ScheduleDailySummary();

            // Запуск главного окна
            MainWindow = new MainWindow();
            MainWindow.Closing += MainWindow_Closing;
            MainWindow.Show();
        }

        private void ScheduleDailySummary()
        {
            DateTime now = DateTime.Now;
            DateTime todayNoon = DateTime.Today.AddHours(12);
            TimeSpan due = now < todayNoon
                ? (todayNoon - now)
                : (todayNoon.AddDays(1) - now);

            _dailySummaryTimer = new Timer(_ =>
            {
                try { ShowDailyOverdueSummary(); }
                catch { /* логирование */ }
            },
            null,
            due,
            TimeSpan.FromDays(1));
        }

        private void ShowDailyOverdueSummary()
        {
            int userId;
            try
            {
                userId = UserSession.IdUser;
            }
            catch
            {
                return; // если нет сессии
            }

            string userName = "";
            string userEmail = "";
            List<Model.Task> overdue;

            using (var ctx = new Model.TaskManagementEntities1())
            {
                var user = ctx.Users.Find(userId);
                if (user == null || string.IsNullOrWhiteSpace(user.Mail))
                    return;

                userName = user.Name?.Trim() ?? "Пользователь";
                userEmail = user.Mail;

                DateTime today = DateTime.Today;
                overdue = ctx.Task
                    .Include(t => t.Status)
                    .Where(t =>
                        t.CreatorId == userId &&
                        t.Status.Name != "Завершено" &&
                        DbFunctions.TruncateTime(t.EndDate) < today)
                    .ToList();
            }

            string title = "Сводка за день";
            string text = overdue.Count == 0
                ? "У вас нет просроченных задач. Отличная работа! 🎉"
                : $"У вас {overdue.Count} просроченных задач. Не забудьте перенести сроки или завершить их.";

            _notifyIcon.ShowBalloonTip(8000, title, text, WinForms.ToolTipIcon.Info);

            SendDailySummaryEmail(userEmail, userName, overdue.Count);
        }

        private void SendDailySummaryEmail(string to, string userName, int overdueCount)
        {
            try
            {
                string from = ConfigurationManager.AppSettings["EmailFrom"];
                string pass = ConfigurationManager.AppSettings["EmailPassword"];

                using (var smtp = new SmtpClient("smtp.gmail.com", 587))
                {
                    smtp.Credentials = new NetworkCredential(from, pass);
                    smtp.EnableSsl = true;

                    string subject = "MyTask — Ваша сводка за день";

                    string body = $@"
                            <div style='font-family:Segoe UI, sans-serif; color:#333;'>
                                <h2>Сводка за день</h2>
                                <p>Привет, <b>{userName}</b>!</p>
                                <p>Сегодня у вас <b>{overdueCount}</b> просроченных задач.</p>
                                <p style='margin-top:10px;'>Помните: переносить сроки, делегировать и даже удалять задачи – нормально! Это дает свободу. 😊</p>
                                <hr style='margin:20px 0;' />
                                <p style='font-size:12px; color:#888;'>Это письмо создано автоматически приложением MyTask.</p>
                            </div>";

                    var msg = new MailMessage(from, to)
                    {
                        Subject = subject,
                        Body = body,
                        IsBodyHtml = true
                    };

                    smtp.Send(msg);
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine("Ошибка при отправке письма сводки: " + ex.Message);
            }
        }

        private void CheckReminders()
        {
            using (var ctx = new Model.TaskManagementEntities1())
            {
                DateTime now = DateTime.Now;
                var due = ctx.Task
                    .Include(t => t.Status)
                    .Where(t =>
                        t.Status.Name != "Завершено" &&
                        t.ReminderDate != null &&
                        DbFunctions.DiffMinutes(now, t.ReminderDate) == 0)
                    .ToList();

                foreach (var t in due)
                {
                    _notifyIcon.ShowBalloonTip(
                        5000,
                        "Напоминание: " + t.Title,
                        t.Description ?? string.Empty,
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
                    to = ctx.Users.Find(t.CreatorId)?.Mail;
                }
                if (string.IsNullOrWhiteSpace(to)) return;

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
                Console.WriteLine("Ошибка отправки почты: " + ex.Message);
            }
        }

        private void MainWindow_Closing(object sender,
            System.ComponentModel.CancelEventArgs e)
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
            }
            MainWindow.Show();
            MainWindow.WindowState = WindowState.Normal;
            MainWindow.Activate();
        }

        private void ExitApplication()
        {
            _isReallyClosing = true;
            _notifyIcon.Visible = false;
            _notifyIcon.Dispose();
            _reminderTimer?.Dispose();
            _dailySummaryTimer?.Dispose();
            _singleInstanceMutex?.ReleaseMutex();
            Shutdown();
        }

        protected override void OnExit(ExitEventArgs e)
        {
            _reminderTimer?.Dispose();
            _dailySummaryTimer?.Dispose();
            _notifyIcon?.Dispose();
            _singleInstanceMutex?.ReleaseMutex();
            base.OnExit(e);
        }
    }
}
