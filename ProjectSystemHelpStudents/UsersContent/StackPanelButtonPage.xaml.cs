using ProjectSystemHelpStudents.Helper;
using System.Collections.Generic;
using System.Linq;
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

            GenerateProjectButtons();

            SubscribeToProjectAddedEvent();
        }

        private void SubscribeToProjectAddedEvent()
        {
            // Создаем экземпляр AddProjectWindow для подписки на событие
            var addProjectWindow = new AddProjectWindow();
            addProjectWindow.ProjectAdded += (newProject) =>
            {
                Dispatcher.Invoke(() => GenerateProjectButtons());
            };
        }

        private void GenerateProjectButtons()
        {
            var projectStackPanel = new StackPanel();
            var projects = GetProjects();

            foreach (var project in projects)
            {
                Button projectButton = new Button
                {
                    Content = project.Name,
                    Style = (Style)FindResource("TransparentButtonStyle"),
                    Margin = new Thickness(5),
                    Tag = project.ProjectId
                };

                projectButton.Click += ProjectButton_Click;
                projectStackPanel.Children.Add(projectButton);
            }

            // Очищаем старые кнопки и добавляем новые
            ProjectStackPanel.Children.Clear();
            ProjectStackPanel.Children.Add(projectStackPanel);
        }

        private List<Project> GetProjects()
        {
            using (var context = new TaskManagementEntities1())
            {
                return context.Project.ToList();
            }
        }

        private void ProjectButton_Click(object sender, RoutedEventArgs e)
        {
            Button button = sender as Button;
            int projectId = (int)button.Tag;

            ProjectDetailsPage content = new ProjectDetailsPage(projectId);
            FrmClass.frmContentUser.Content = content;
            StackPanelButtonPage _content = new StackPanelButtonPage();
            FrmClass.frmStackPanelButton.Content = _content;
        }

        private void UserNameButton_Click(object sender, RoutedEventArgs e)
        {
            int userId = UserSession.IdUser;

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
        private void MyProjectButton_Click(object sender, RoutedEventArgs e)
        {
            MyProjectPage content = new MyProjectPage();
            FrmClass.frmContentUser.Content = content;
            StackPanelButtonPage _content = new StackPanelButtonPage();
            FrmClass.frmStackPanelButton.Content = _content;
        }     
    }
}
