using System.Windows.Navigation;
using ProjectSystemHelpStudents.UsersContent;
using ProjectSystemHelpStudents.Helper;
using System.Windows.Interop;
using System.Windows;
using System;

namespace ProjectSystemHelpStudents
{
    /// <summary>
    /// Логика взаимодействия для MainWindow.xaml
    /// </summary>
    public partial class MainWindow : System.Windows.Window
    {
        public MainWindow()
        {
            InitializeComponent();
            AuthPage authPage = new AuthPage();
            frmAuth.Content = authPage;

            FrmClass.frmStackPanelButton = frmStackPanelButton;
            FrmClass.frmAuth = frmAuth;
            FrmClass.frmReg = frmReg;
            FrmClass.frmContentUser = frmContentUser;
            FrmClass.frmContentAdmin = frmContentAdmin;
        }

        protected override void OnSourceInitialized(EventArgs e)
        {
            base.OnSourceInitialized(e);
            HwndSource source = PresentationSource.FromVisual(this) as HwndSource;
            source.AddHook(WndProc);
        }

        private IntPtr WndProc(IntPtr hwnd, int msg, IntPtr wParam, IntPtr lParam, ref bool handled)
        {
            if (msg == 0x8001)
            {
                this.Show();
                this.WindowState = WindowState.Normal;
                this.Activate();
                handled = true;
            }
            return IntPtr.Zero;
        }
    }
}
