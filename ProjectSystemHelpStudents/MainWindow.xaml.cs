using System;
using System.Windows;
using System.Windows.Interop;
using ProjectSystemHelpStudents.Helper;
using ProjectSystemHelpStudents.Properties;
using ProjectSystemHelpStudents.UsersContent;
using ProjectSystemHelpStudents.Views.AdminPages;

namespace ProjectSystemHelpStudents
{
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            InitializeComponent();

            // Регистрируем фреймы
            FrmClass.frmReg = this.frmReg;
            FrmClass.frmAuth = this.frmAuth;
            FrmClass.frmContentUser = this.frmContentUser;
            FrmClass.frmContentAdmin = this.frmContentAdmin;
            FrmClass.frmStackPanelButton = this.frmStackPanelButton;

            ShowLogin();
        }

        public void ShowLogin()
        {
            // 1) Сброс сессии
            UserSession.IdUser = 0;
            UserSession.NameUser = null;

            // 2) Обновляем настройки
            Properties.Settings.Default.Reload();

            // 3) Если RememberMe == false — очищаем старые Сredentials
            if (!Properties.Settings.Default.RememberMe)
            {
                Properties.Settings.Default.SavedLogin = "";
                Properties.Settings.Default.SavedPasswordHash = "";
                Properties.Settings.Default.Save();
            }

            // 4) Скрываем все панели
            frmContentUser.Visibility = Visibility.Collapsed;
            frmContentUser.Content = null;
            frmStackPanelButton.Visibility = Visibility.Collapsed;
            frmStackPanelButton.Content = null;
            frmContentAdmin.Visibility = Visibility.Collapsed;
            frmContentAdmin.Content = null;

            // 5) Показываем AuthPage
            frmAuth.Visibility = Visibility.Visible;
            frmAuth.Content = new AuthPage();
        }

        public void ShowAdminUI()
        {
            frmAuth.Visibility = Visibility.Collapsed;
            frmAuth.Content = null;

            frmContentUser.Visibility = Visibility.Collapsed;
            frmContentUser.Content = null;

            frmStackPanelButton.Visibility = Visibility.Visible;
            frmStackPanelButton.Content = new AdminNavigationPage();

            frmContentAdmin.Visibility = Visibility.Visible;
            frmContentAdmin.Content = new UsersManagementPage();
        }

        public void ShowUserUI()
        {
            frmAuth.Visibility = Visibility.Collapsed;
            frmAuth.Content = null;

            frmContentAdmin.Visibility = Visibility.Collapsed;
            frmContentAdmin.Content = null;

            frmContentUser.Visibility = Visibility.Visible;
            frmContentUser.Content = new UpcomingTasksPage();

            frmStackPanelButton.Visibility = Visibility.Visible;
            frmStackPanelButton.Content = new StackPanelButtonPage();
        }

        protected override void OnSourceInitialized(EventArgs e)
        {
            base.OnSourceInitialized(e);
            var source = PresentationSource.FromVisual(this) as HwndSource;
            source?.AddHook(WndProc);
        }

        private IntPtr WndProc(IntPtr hwnd, int msg, IntPtr wParam, IntPtr lParam, ref bool handled)
        {
            const int WM_SHOWME = 0x8001;
            if (msg == WM_SHOWME)
            {
                if (!Settings.Default.RememberMe)
                {
                    // Если ользователь не хотел автологин чистим данные и показываем логин
                    UserSession.IdUser = 0;
                    UserSession.NameUser = null;

                    Settings.Default.SavedLogin = "";
                    Settings.Default.SavedPasswordHash = "";
                    Settings.Default.Save();

                    ShowLogin();
                }
                else
                {
                    // Если пользователь хотел автологин просто вернём UI как есть
                }

                // В любом случае нужно показать само окно
                this.Show();
                this.WindowState = WindowState.Normal;
                this.Activate();

                handled = true;
            }
            return IntPtr.Zero;
        }
    }
}
