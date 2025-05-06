using ProjectSystemHelpStudents.Helper;
using System;
using System.Collections.ObjectModel;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Collections.Specialized;

namespace ProjectSystemHelpStudents.UsersContent
{
    public partial class MyProjectPage : Page
    {
        public ObservableCollection<ProjectViewModel> Projects { get; set; }
        public string ProjectCountText => $"{Projects.Count} проекта";
        private ObservableCollection<ProjectViewModel> AllProjects = new ObservableCollection<ProjectViewModel>();


        public MyProjectPage()
        {
            InitializeComponent();
            Projects = new ObservableCollection<ProjectViewModel>();
            DataContext = this;

            RefreshProjects();
        }

        private void FilterComboBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            ApplyFilters();
        }

        private void ApplyFilters()
        {
            if (Projects == null || AllProjects == null)
                return; // или throw new InvalidOperationException("Projects или AllProjects не инициализированы");

            string searchText = SearchBox?.Text?.ToLower() ?? "";
            string selectedFilter = (FilterComboBox?.SelectedItem as ComboBoxItem)?.Content as string;

            var filtered = AllProjects.Where(p =>
                p.Name.ToLower().Contains(searchText) &&
                (selectedFilter == "Все проекты" ||
                 (selectedFilter == "Активные проекты" && !p.IsCompleted) ||
                 (selectedFilter == "Завершенные проекты" && p.IsCompleted))
            ).ToList();

            Projects.Clear();
            foreach (var project in filtered)
            {
                Projects.Add(project);
            }
        }

        private void ProjectsListView_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (ProjectsListView.SelectedItem is ProjectViewModel selectedProject)
            {
                var projectDetailsPage = new ProjectDetailsPage(selectedProject.ProjectId);
                NavigationService?.Navigate(projectDetailsPage);

                ProjectsListView.SelectedItem = null; // сброс выбора, чтобы можно было снова выбрать тот же проект
            }
        }

        private void OpenContextMenu_Click(object sender, RoutedEventArgs e)
        {
            if (sender is Button button && button.ContextMenu != null)
            {
                button.ContextMenu.PlacementTarget = button;
                button.ContextMenu.Placement = System.Windows.Controls.Primitives.PlacementMode.Bottom;
                button.ContextMenu.IsOpen = true;
            }
        }

        private void RefreshProjects()
        {
            using (var ctx = new TaskManagementEntities1())
            {
                int uid = UserSession.IdUser;
                var userTeamIds = ctx.TeamMember
                                     .Where(tm => tm.UserId == uid)
                                     .Select(tm => tm.TeamId)
                                     .ToList();

                var userProjects = ctx.Project
                    .Where(p =>
                        p.OwnerId == uid ||
                        (p.TeamId != null && userTeamIds.Contains(p.TeamId.Value))
                    )
                    .OrderBy(p => p.Name)
                    .ToList();

                Projects.Clear();
                AllProjects.Clear();

                foreach (var p in userProjects)
                {
                    var teamName = p.Team != null ? p.Team.Name : null;

                    var vm = new ProjectViewModel
                    {
                        ProjectId = p.ProjectId,
                        Name = p.Name,
                        Description = p.Description,
                        StartDate = p.StartDate,
                        EndDate = p.EndDate,
                        TeamId = p.TeamId,
                        TeamName = teamName,
                        IsCompleted = p.IsCompleted
                    };

                    Projects.Add(vm);
                    AllProjects.Add(vm);
                }
            }

            ProjectsListView.ItemsSource = Projects;
        }

        private void AddProject_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                AddProjectWindow addProjectWindow = new AddProjectWindow();

                addProjectWindow.ProjectAdded += (newProject) =>
                {
                    Dispatcher.Invoke(() => RefreshProjects());
                };

                addProjectWindow.ShowDialog();

                if (addProjectWindow.IsProjectAdded)
                {
                    RefreshProjects();
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка при добавлении проекта: {ex.Message}", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void AddToFavorites_Click(object sender, RoutedEventArgs e)
        {
            if (sender is MenuItem menuItem && menuItem.Tag is ProjectViewModel project)
            {
                if (UserSettingsHelper.IsDetached(project.ProjectId)) // Только если ОТКРЕПЛЁН
                {
                    UserSettingsHelper.RemoveDetachedProject(project.ProjectId); // Закрепляем
                    project.IsDetached = false;
                    RefreshProjects();

                    StackPanelButtonPage.RefreshProjectStackPanel?.Invoke();

                    MessageBox.Show($"Проект «{project.Name}» закреплён в боковой панели.", "Закреплено", MessageBoxButton.OK, MessageBoxImage.Information);
                }
                else
                {
                    MessageBox.Show("Проект уже закреплён.", "Информация", MessageBoxButton.OK, MessageBoxImage.Information);
                }
            }
        }

        private void EditProject_Click(object sender, RoutedEventArgs e)
        {
            if (sender is MenuItem menuItem && menuItem.Tag is ProjectViewModel project)
            {
                try
                {
                    AddProjectWindow editWindow = new AddProjectWindow
                    {
                        ProjectId = project.ProjectId
                    };

                    editWindow.ProjectAdded += (newProject) =>
                    {
                        Dispatcher.Invoke(() => RefreshProjects());
                    };

                    editWindow.ShowDialog();

                    if (editWindow.IsProjectUpdated)
                    {
                        RefreshProjects();
                    }
                }
                catch (Exception ex)
                {
                    MessageBox.Show($"Ошибка при редактировании проекта: {ex.Message}", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
                }
            }
        }

        private void DeleteProject_Click(object sender, RoutedEventArgs e)
        {
            if (sender is MenuItem menuItem && menuItem.Tag is ProjectViewModel projectToDelete)
            {
                try
                {
                    using (var context = new TaskManagementEntities1())
                    {
                        var project = context.Project.FirstOrDefault(p => p.ProjectId == projectToDelete.ProjectId);

                        if (project != null)
                        {
                            var tasks = context.Task.Where(t => t.ProjectId == project.ProjectId).ToList();
                            context.Task.RemoveRange(tasks);
                            context.SaveChanges();

                            context.Project.Remove(project);
                            context.SaveChanges();

                            Dispatcher.Invoke(() => Projects.Remove(projectToDelete));

                            MessageBox.Show("Проект успешно удален!", "Успех", MessageBoxButton.OK, MessageBoxImage.Information);
                        }
                        else
                        {
                            MessageBox.Show("Проект не найден в базе данных.", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
                        }
                    }
                }
                catch (Exception ex)
                {
                    MessageBox.Show($"Ошибка при удалении проекта: {ex.Message}", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
                }
            }
            else
            {
                MessageBox.Show("Ошибка: Данные проекта не найдены.", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void SearchBox_TextChanged(object sender, TextChangedEventArgs e)
        {
            ApplyFilters();
        }

        private void MarkAsCompleted_Click(object sender, RoutedEventArgs e)
        {
            if (sender is MenuItem menuItem && menuItem.Tag is ProjectViewModel project)
            {
                using (var ctx = new TaskManagementEntities1())
                {
                    var dbProject = ctx.Project.FirstOrDefault(p => p.ProjectId == project.ProjectId);
                    if (dbProject != null && !dbProject.IsCompleted)
                    {
                        dbProject.IsCompleted = true;
                        ctx.SaveChanges();

                        MessageBox.Show($"Проект «{project.Name}» отмечен как завершён.", "Успех", MessageBoxButton.OK, MessageBoxImage.Information);
                        RefreshProjects();
                    }
                    else
                    {
                        MessageBox.Show("Проект уже завершён или не найден.", "Информация", MessageBoxButton.OK, MessageBoxImage.Information);
                    }
                }
            }
        }
    }
}
