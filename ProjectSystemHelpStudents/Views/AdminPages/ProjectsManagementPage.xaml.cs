using System.Collections.ObjectModel;
using System;
using System.ComponentModel;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Linq;

namespace ProjectSystemHelpStudents.Views.AdminPages
{
    public partial class ProjectsManagementPage : Page
    {
        private readonly TaskManagementEntities1 _ctx = new TaskManagementEntities1();
        private ICollectionView _projectsView;

        public ProjectsManagementPage()
        {
            InitializeComponent();
            LoadProjects();
        }

        private void LoadProjects()
        {
            var list = _ctx.Project.Include("Team").ToList();
            var projects = new ObservableCollection<Project>(list);
            _projectsView = CollectionViewSource.GetDefaultView(projects);
            ProjectsGrid.ItemsSource = _projectsView;
        }

        private void EditProjectButton_Click(object sender, RoutedEventArgs e)
        {
            if (sender is Button btn && btn.Tag is int id)
            {
                var project = _ctx.Project.Find(id);
                if (project != null)
                {
                    var wnd = new Window
                    {
                        Title = "Редактирование проекта",
                        Content = new ProjectEditControl(project, _ctx),
                        Width = 320,
                        Height = 350,
                        ResizeMode = ResizeMode.NoResize,
                        WindowStartupLocation = WindowStartupLocation.CenterScreen,
                        Owner = Application.Current.MainWindow,
                        Style = (Style)Application.Current.FindResource("SmallWindowStyle")
                    };
                    wnd.ShowDialog();
                    LoadProjects();
                }
            }
        }

        private void AddProjectButton_Click(object sender, RoutedEventArgs e)
        {
            var newProj = new Project { Name = "Новый проект" };
            _ctx.Project.Add(newProj);
            _ctx.SaveChanges();

            (_projectsView.SourceCollection as ObservableCollection<Project>)?.Add(newProj);
        }

        private void SaveProjectsButton_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                _ctx.SaveChanges();
                MessageBox.Show("Проекты сохранены", "Успех", MessageBoxButton.OK, MessageBoxImage.Information);
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка при сохранении: {ex.Message}", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void DeleteProjectButton_Click(object sender, RoutedEventArgs e)
        {
            if (!(sender is Button btn) || !(btn.Tag is int id))
                return;

            var proj = _ctx.Project
                .Include("Section")
                .Include("Task")
                .Include("Task.Files")
                .Include("Task.Comment")
                .Include("Task.TaskAssignee")
                .Include("Task.TaskFilters")
                .Include("Task.TaskLabels")
                .FirstOrDefault(p => p.ProjectId == id);

            if (proj == null)
                return;

            if (MessageBox.Show(
                    $"Удалить проект «{proj.Name}» и все связанные данные?",
                    "Подтверждение",
                    MessageBoxButton.YesNo,
                    MessageBoxImage.Warning
                ) != MessageBoxResult.Yes)
                return;

            foreach (var task in proj.Task.ToList())
            {
                if (task.Files.Any())
                    _ctx.Files.RemoveRange(task.Files);

                if (task.Comment.Any())
                    _ctx.Comment.RemoveRange(task.Comment);

                if (task.TaskAssignee.Any())
                    _ctx.TaskAssignee.RemoveRange(task.TaskAssignee);

                if (task.TaskFilters.Any())
                    _ctx.TaskFilters.RemoveRange(task.TaskFilters);

                if (task.TaskLabels.Any())
                    _ctx.TaskLabels.RemoveRange(task.TaskLabels);

                _ctx.Task.Remove(task);
            }

            if (proj.Section.Any())
                _ctx.Section.RemoveRange(proj.Section);

            _ctx.Project.Remove(proj);

            try
            {
                _ctx.SaveChanges();

                var source = _projectsView.SourceCollection as ObservableCollection<Project>;
                var toRemove = source?.FirstOrDefault(p => p.ProjectId == id);
                if (toRemove != null)
                    source.Remove(toRemove);
            }
            catch (Exception ex)
            {
                MessageBox.Show(
                    $"Ошибка при удалении проекта: {ex.Message}",
                    "Ошибка",
                    MessageBoxButton.OK,
                    MessageBoxImage.Error
                );
            }
        }

        private void ProjectSearchBox_TextChanged(object sender, TextChangedEventArgs e)
        {
            var text = ProjectSearchBox.Text.Trim().ToLower();
            _projectsView.Filter = item =>
            {
                if (item is Project p)
                    return string.IsNullOrEmpty(text) || p.Name.ToLower().Contains(text);
                return false;
            };
        }
    }
}
