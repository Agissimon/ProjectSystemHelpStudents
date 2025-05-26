using System;
using System.Windows;
using System.Windows.Interop;
using ProjectSystemHelpStudents.Helper;
using ProjectSystemHelpStudents.UsersContent;
using ProjectSystemHelpStudents.Views.AdminPages;

namespace ProjectSystemHelpStudents
{
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            InitializeComponent();

            FrmClass.frmReg = this.frmReg;
            FrmClass.frmAuth = this.frmAuth;
            FrmClass.frmContentUser = this.frmContentUser;
            FrmClass.frmContentAdmin = this.frmContentAdmin;
            FrmClass.frmStackPanelButton = this.frmStackPanelButton;

            ShowLogin();
        }

        public void ShowLogin()
        {
            // Сброс сессии
            UserSession.IdUser = 0;
            UserSession.NameUser = null;

            // Показываем только AuthPage
            frmAuth.Visibility = Visibility.Visible;
            frmAuth.Content = new AuthPage();

            frmContentUser.Visibility = Visibility.Collapsed;
            frmContentUser.Content = null;

            frmStackPanelButton.Visibility = Visibility.Collapsed;
            frmStackPanelButton.Content = null;

            frmContentAdmin.Visibility = Visibility.Collapsed;
            frmContentAdmin.Content = null;
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
                Show();
                WindowState = WindowState.Normal;
                Activate();
                handled = true;
            }
            return IntPtr.Zero;
        }
    }
}
