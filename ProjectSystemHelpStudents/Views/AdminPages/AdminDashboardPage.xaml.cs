using System;
using System.Configuration;
using System.Windows;
using System.Windows.Controls;
using ProjectSystemHelpStudents.Helper;
using ProjectSystemHelpStudents.UsersContent;
using ProjectSystemHelpStudents.Views.AdminPages;

namespace ProjectSystemHelpStudents.Views.AdminPages
{
    public partial class AdminNavigationPage : Page
    {
        public AdminNavigationPage()
        {
            InitializeComponent();
        }

        private void UsersButton_Click(object sender, RoutedEventArgs e)
        {
            var page = new UsersManagementPage();
            FrmClass.frmContentUser.Content = page;
        }

        private void ProjectsButton_Click(object sender, RoutedEventArgs e)
        {
            var page = new ProjectsManagementPage();
            FrmClass.frmContentUser.Content = page;
        }

        private void SettingsButton_Click(object sender, RoutedEventArgs e)
        {
            var page = new SettingsPage();
            FrmClass.frmContentUser.Content = page;
        }

        private void SignOutButton_Click(object sender, RoutedEventArgs e)
        {
            UserSession.IdUser = 0;
            UserSession.NameUser = null;

            var mainWindow = Application.Current.MainWindow as MainWindow;

            if (mainWindow != null)
            {
                mainWindow.frmAuth.Content = null;

                mainWindow.frmContentUser.Content = null;
                mainWindow.frmStackPanelButton.Content = null;

                mainWindow.frmAuth.Navigate(new AuthPage());

                Console.WriteLine(UserSession.IdUser);
                Console.WriteLine(UserSession.NameUser);
            }
        }
    }
}
