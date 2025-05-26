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
            FrmClass.NavigateTo(new UsersManagementPage());
        }

        private void ProjectsButton_Click(object sender, RoutedEventArgs e)
        {
            FrmClass.NavigateTo(new ProjectsManagementPage());
        }

        private void SettingsButton_Click(object sender, RoutedEventArgs e)
        {
            FrmClass.NavigateTo(new SettingsPage());
        }

        private void SignOutButton_Click(object sender, RoutedEventArgs e)
        {
            // Сброс сессии
            UserSession.IdUser = 0;
            UserSession.NameUser = null;

            // Сброс настроек автологина
            Properties.Settings.Default.RememberMe = false;
            Properties.Settings.Default.SavedLogin = string.Empty;
            Properties.Settings.Default.SavedPasswordHash = string.Empty;
            Properties.Settings.Default.Save();

            // Скрываем все фреймы контента
            FrmClass.frmContentUser.Visibility = Visibility.Collapsed;
            FrmClass.frmContentUser.Content = null;
            FrmClass.frmStackPanelButton.Visibility = Visibility.Collapsed;
            FrmClass.frmStackPanelButton.Content = null;
            FrmClass.frmContentAdmin.Visibility = Visibility.Collapsed;
            FrmClass.frmContentAdmin.Content = null;

            // Показываем окно логина
            FrmClass.frmAuth.Visibility = Visibility.Visible;
            FrmClass.frmAuth.Content = new AuthPage();
        }
    }
}
