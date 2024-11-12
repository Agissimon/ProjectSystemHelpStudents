using ProjectSystemHelpStudents.Helper;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Navigation;

namespace ProjectSystemHelpStudents.UsersContent
{
    public partial class StackPanelButtonPage : Page
    {
        public StackPanelButtonPage()
        {
            InitializeComponent();
            DataContext = this;
            UserNameButton.Content = UserSession.NameUser;  
        }
        private void UserNameButton_Click(object sender, RoutedEventArgs e)
        {
            UserPage content = new UserPage();
            FrmClass.frmContentUser.Content = content;
            StackPanelButtonPage _content = new StackPanelButtonPage();
            FrmClass.frmStackPanelButton.Content = _content;
        }

        private void SearchButton_Click(object sender, RoutedEventArgs e)
        {
            SearchWindow addTaskWindow = new SearchWindow();
            addTaskWindow.ShowDialog();
        }

        private void IncomingButton_Click(object sender, RoutedEventArgs e)
        {
            IncomingPage content = new IncomingPage();
            FrmClass.frmContentUser.Content = content;
            StackPanelButtonPage _content = new StackPanelButtonPage();
            FrmClass.frmStackPanelButton.Content = _content;
        }

        private void TodayButton_Click(object sender, RoutedEventArgs e)
        {
            TodayPage content = new TodayPage();
            FrmClass.frmContentUser.Content = content;
            StackPanelButtonPage _content = new StackPanelButtonPage();
            FrmClass.frmStackPanelButton.Content = _content;
        }

        private void UpcomingButton_Click(object sender, RoutedEventArgs e)
        {
            UpcomingTasksPage content = new UpcomingTasksPage();
            FrmClass.frmContentUser.Content = content;
            StackPanelButtonPage _content = new StackPanelButtonPage();
            FrmClass.frmStackPanelButton.Content = _content;
        }

        private void FiltersButton_Click(object sender, RoutedEventArgs e)
        {
            FiltersPage content = new FiltersPage();
            FrmClass.frmContentUser.Content = content;
            StackPanelButtonPage _content = new StackPanelButtonPage();
            FrmClass.frmStackPanelButton.Content = _content;
        }

        private void CompletedButton_Click(object sender, RoutedEventArgs e)
        {
            CompletedPage content = new CompletedPage();
            FrmClass.frmContentUser.Content = content;
            StackPanelButtonPage _content = new StackPanelButtonPage();
            FrmClass.frmStackPanelButton.Content = _content;
        }

        private void HomeButton_Click(object sender, RoutedEventArgs e)
        {
        }

        private void WorkButton_Click(object sender, RoutedEventArgs e)
        {
        }

        private void StudyButton_Click(object sender, RoutedEventArgs e)
        {
        } 
    }
}
