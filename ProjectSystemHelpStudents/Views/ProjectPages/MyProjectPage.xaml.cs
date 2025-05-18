using ProjectSystemHelpStudents.Helper;
using System;
using System.Collections.ObjectModel;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Collections.Specialized;
using System.Collections.Generic;

namespace ProjectSystemHelpStudents.UsersContent
{
    public partial class MyProjectPage : Page
    {
        public ObservableCollection<ProjectViewModel> Projects { get; set; }
        public string ProjectCountText => $"{Projects.Count} проекта";
        private ObservableCollection<ProjectViewModel> AllProjects = new ObservableCollection<ProjectViewModel>();
        public List<Team> AvailableTeams { get; set; } = new List<Team>();


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
                return;

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
            int uid = UserSession.IdUser;

            using (var ctx = new TaskManagementEntities1())
            {
                var userTeamIds = ctx.TeamMember
                                     .Where(tm => tm.UserId == uid)
                                     .Select(tm => tm.TeamId)
                                     .ToList();
                var ownTeams = ctx.Team
                                  .Where(t => t.LeaderId == uid)
                                  .Select(t => t.TeamId)
                                  .ToList();
                var allowedTeamIds = userTeamIds
                                     .Concat(ownTeams)
                                     .Distinct()
                                     .ToList();

                var allowedTeams = ctx.Team
                                      .Where(t => allowedTeamIds.Contains(t.TeamId))
                                      .OrderBy(t => t.Name)
                                      .ToList();

                var userProjects = ctx.Project
                    .Where(p => p.OwnerId == uid
                             || (p.TeamId != null && allowedTeamIds.Contains(p.TeamId.Value)))
                    .OrderBy(p => p.Name)
                    .ToList();

                Projects.Clear();
                AllProjects.Clear();

                foreach (var p in userProjects)
                {
                    var vm = new ProjectViewModel
                    {
                        ProjectId = p.ProjectId,
                        Name = p.Name,
                        Description = p.Description,
                        StartDate = p.StartDate,
                        EndDate = p.EndDate,
                        TeamId = p.TeamId,
                        TeamName = p.Team?.Name,
                        IsCompleted = p.IsCompleted,

                        // наполняем только теми командами, где пользователь есть
                        AvailableTeams = allowedTeams
                    };
                    Projects.Add(vm);
                    AllProjects.Add(vm);
                }
            }

            ProjectsListView.ItemsSource = Projects;

        }


        private void AssignProjectToTeam_Click(object sender, RoutedEventArgs e)
        {
            if (sender is MenuItem mi
                && mi.DataContext is Team selectedTeam
                && mi.Tag is ProjectViewModel projectVm)
            {
                try
                {
                    using (var ctx = new TaskManagementEntities1())
                    {
                        var dbProj = ctx.Project.Find(projectVm.ProjectId);
                        if (dbProj != null)
                        {
                            dbProj.TeamId = selectedTeam.TeamId == 0 ? (int?)null : selectedTeam.TeamId;
                            ctx.SaveChanges();
                            MessageBox.Show(
                              $"Проект «{projectVm.Name}» добавлен в команду «{selectedTeam.Name}»",
                              "Успех", MessageBoxButton.OK, MessageBoxImage.Information);
                            RefreshProjects();
                        }
                    }
                }
                catch (Exception ex)
                {
                    MessageBox.Show($"Ошибка при назначении команды: {ex.Message}",
                                    "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
                }
            }
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

        private void ActivateProject_Click(object sender, RoutedEventArgs e)
        {
            if (sender is MenuItem menuItem && menuItem.Tag is ProjectViewModel project)
            {
                using (var ctx = new TaskManagementEntities1())
                {
                    var dbProject = ctx.Project.FirstOrDefault(p => p.ProjectId == project.ProjectId);
                    if (dbProject != null && dbProject.IsCompleted)
                    {
                        dbProject.IsCompleted = false;
                        ctx.SaveChanges();

                        MessageBox.Show($"Проект «{project.Name}» активирован.", "Успех", MessageBoxButton.OK, MessageBoxImage.Information);
                        RefreshProjects();
                    }
                    else
                    {
                        MessageBox.Show("Проект уже активен или не найден.", "Информация", MessageBoxButton.OK, MessageBoxImage.Information);
                    }
                }
            }
        }

        private void RemoveProjectFromTeam_Click(object sender, RoutedEventArgs e)
        {
            if (!(sender is MenuItem mi) || !(mi.Tag is ProjectViewModel vm))
                return;

            try
            {
                using (var ctx = new TaskManagementEntities1())
                {
                    var project = ctx.Project.Find(vm.ProjectId);
                    if (project != null && project.TeamId != null)
                    {
                        project.TeamId = null;
                        ctx.SaveChanges();

                        MessageBox.Show(
                            $"Проект «{vm.Name}» удалён из команды.",
                            "Успех",
                            MessageBoxButton.OK,
                            MessageBoxImage.Information
                        );
                    }
                    else
                    {
                        MessageBox.Show(
                            "Проект либо уже не состоит ни в одной команде, либо не найден.",
                            "Информация",
                            MessageBoxButton.OK,
                            MessageBoxImage.Information
                        );
                    }
                }
                RefreshProjects();
            }
            catch (Exception ex)
            {
                MessageBox.Show(
                    $"Не удалось удалить проект из команды: {ex.Message}",
                    "Ошибка",
                    MessageBoxButton.OK,
                    MessageBoxImage.Error
                );
            }
        }
    }
}