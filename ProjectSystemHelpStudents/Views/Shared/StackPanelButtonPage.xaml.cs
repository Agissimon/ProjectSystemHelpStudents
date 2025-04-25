using ProjectSystemHelpStudents.Helper;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Navigation;

namespace ProjectSystemHelpStudents.UsersContent
{
    public partial class StackPanelButtonPage : Page, INotifyPropertyChanged
    {
        private string _nameUser;
        public static Action RefreshProjectStackPanel;

        public string NameUser
        {
            get => _nameUser;
            set
            {
                _nameUser = value;
                OnPropertyChanged(nameof(NameUser));
            }
        }

        public event PropertyChangedEventHandler PropertyChanged;

        protected void OnPropertyChanged(string propertyName)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }

        public StackPanelButtonPage()
        {
            InitializeComponent();
            DataContext = this;

            NameUser = string.Empty;
            if (!string.IsNullOrEmpty(UserSession.NameUser))
            {
                NameUser = UserSession.NameUser.Split(' ')[0];
            }

            GenerateProjectButtons();
            RefreshProjectStackPanel = GenerateProjectButtons;
            SubscribeToProjectAddedEvent();

            UserSession.UserNameUpdated += (newName) =>
            {
                Dispatcher.Invoke(() => UpdateUserName(newName));
            };
        }

        private void UpdateUserName(string newName)
        {
            NameUser = string.Empty;
            NameUser = newName;
        }

        private void SubscribeToProjectAddedEvent()
        {
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

            var detachedProjects = UserSettingsHelper.GetDetachedProjects();
            ProjectStackPanel.Children.Clear();

            foreach (var project in projects)
            {
                if (detachedProjects.Contains(project.ProjectId))
                    continue;

                StackPanel projectPanel = new StackPanel { Orientation = Orientation.Horizontal };

                // Кнопка проекта
                Button projectButton = new Button
                {
                    Content = project.Name,
                    Style = (Style)FindResource("TransparentButtonStyle"),
                    Margin = new Thickness(10, 5, 0, 0),
                    Tag = project.ProjectId
                };
                projectButton.Click += ProjectButton_Click;

                // Кнопка открепить
                Button detachButton = new Button
                {
                    Content = "⨉",
                    Width = 25,
                    Height = 25,
                    Margin = new Thickness(5, 10, 0, 0),
                    Tag = project.ProjectId,
                    Style = (Style)FindResource("TransparentButtonStyle")
                };
                detachButton.Click += DetachButton_Click;

                projectPanel.Children.Add(projectButton);
                projectPanel.Children.Add(detachButton);

                projectStackPanel.Children.Add(projectPanel);
            }

            ProjectStackPanel.Children.Clear();
            ProjectStackPanel.Children.Add(projectStackPanel);
        }

        private void DetachButton_Click(object sender, RoutedEventArgs e)
        {
            if (sender is Button button && button.Tag is int projectId)
            {
                UserSettingsHelper.AddDetachedProject(projectId);
                GenerateProjectButtons();
            }
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

            // После изменения имени пользователя обновите его в интерфейсе
            var updatedName = "Новое имя пользователя"; // Пример нового имени
            UpdateUserName(updatedName);
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
