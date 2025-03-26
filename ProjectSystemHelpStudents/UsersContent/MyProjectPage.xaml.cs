using ProjectSystemHelpStudents.Helper;
using System;
using System.Collections.ObjectModel;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;

namespace ProjectSystemHelpStudents.UsersContent
{
    public partial class MyProjectPage : Page
    {
        public ObservableCollection<ProjectViewModel> Projects { get; set; }
        public string ProjectCountText => $"{Projects.Count} проекта";

        public MyProjectPage()
        {
            InitializeComponent();
            Projects = new ObservableCollection<ProjectViewModel>();
            DataContext = this;

            RefreshProjects();
        }

        private void OpenContextMenu_Click(object sender, RoutedEventArgs e)
        {
            if (sender is Button button && button.ContextMenu != null)
            {
                button.ContextMenu.PlacementTarget = button; // Устанавливаем кнопку как цель размещения
                button.ContextMenu.Placement = System.Windows.Controls.Primitives.PlacementMode.Bottom; // Открытие снизу
                button.ContextMenu.IsOpen = true;
            }
        }

        private void RefreshProjects()
        {
            try
            {
                using (var context = new TaskManagementEntities1())
                {
                    var userProjects = context.Project
                        .Select(p => new
                        {
                            p.ProjectId,
                            p.Name,
                            p.Description,
                            p.StartDate,
                            p.EndDate
                        })
                        .ToList();

                    Projects.Clear();
                    foreach (var project in userProjects)
                    {
                        Projects.Add(new ProjectViewModel
                        {
                            ProjectId = project.ProjectId,
                            Name = project.Name,
                            Icon = "📁"
                        });
                    }
                }
                ProjectsListView.ItemsSource = Projects;
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка при загрузке проектов: {ex.Message}", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private int GetProjectTaskCount(int projectId)
        {
            using (var context = new TaskManagementEntities1())
            {
                return context.Task.Where(t => t.ProjectId == projectId).Count();
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

        private void AddProjectAbove_Click(object sender, RoutedEventArgs e)
        {
            if (sender is MenuItem menuItem && menuItem.Tag is ProjectViewModel project)
            {
                MessageBox.Show($"Добавить проект выше: {project.Name}");
            }
        }

        private void AddProjectBelow_Click(object sender, RoutedEventArgs e)
        {
            if (sender is MenuItem menuItem && menuItem.Tag is ProjectViewModel project)
            {
                MessageBox.Show($"Добавить проект ниже: {project.Name}");
            }
        }

        //private void EditProject_Click(object sender, RoutedEventArgs e)
        //{
        //    if (sender is MenuItem menuItem && menuItem.Tag is ProjectViewModel project)
        //    {
        //        try
        //        {
        //            AddProjectWindow editWindow = new AddProjectWindow();
        //            editWindow.ProjectId = project.ProjectId; // Передаем ID проекта для редактирования
        //            editWindow.ShowDialog();

        //            if (editWindow.IsProjectUpdated)
        //            {
        //                RefreshProjects(); // Обновляем список проектов
        //            }
        //        }
        //        catch (Exception ex)
        //        {
        //            MessageBox.Show($"Ошибка при редактировании проекта: {ex.Message}", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
        //        }
        //    }
        //}

        private void AddToFavorites_Click(object sender, RoutedEventArgs e)
        {
            if (sender is MenuItem menuItem && menuItem.Tag is ProjectViewModel project)
            {
                MessageBox.Show($"Добавить в избранное: {project.Name}");
            }
        }

        private void DuplicateProject_Click(object sender, RoutedEventArgs e)
        {
            if (sender is MenuItem menuItem && menuItem.Tag is ProjectViewModel project)
            {
                MessageBox.Show($"Дублировать проект: {project.Name}");
            }
        }

        private void ShareProject_Click(object sender, RoutedEventArgs e)
        {
            if (sender is MenuItem menuItem && menuItem.Tag is ProjectViewModel project)
            {
                MessageBox.Show($"Общий доступ к проекту: {project.Name}");
            }
        }

        private void CopyProjectLink_Click(object sender, RoutedEventArgs e)
        {
            if (sender is MenuItem menuItem && menuItem.Tag is ProjectViewModel project)
            {
                MessageBox.Show($"Скопировать ссылку на проект: {project.Name}");
            }
        }

        private void SaveAsTemplate_Click(object sender, RoutedEventArgs e)
        {
            if (sender is MenuItem menuItem && menuItem.Tag is ProjectViewModel project)
            {
                MessageBox.Show($"Сохранить как шаблон: {project.Name}");
            }
        }

        private void ViewTemplates_Click(object sender, RoutedEventArgs e)
        {
            MessageBox.Show("Посмотреть шаблоны");
        }

        private void ImportFromCSV_Click(object sender, RoutedEventArgs e)
        {
            MessageBox.Show("Импорт из CSV");
        }

        private void ExportToCSV_Click(object sender, RoutedEventArgs e)
        {
            MessageBox.Show("Экспорт CSV");
        }

        private void AddTasksByEmail_Click(object sender, RoutedEventArgs e)
        {
            MessageBox.Show("Добавлять задачи по Email");
        }

        private void ShowProjectCalendar_Click(object sender, RoutedEventArgs e)
        {
            if (sender is MenuItem menuItem && menuItem.Tag is ProjectViewModel project)
            {
                MessageBox.Show($"Календарная лента проекта: {project.Name}");
            }
        }

        private void ShowActivityLog_Click(object sender, RoutedEventArgs e)
        {
            if (sender is MenuItem menuItem && menuItem.Tag is ProjectViewModel project)
            {
                MessageBox.Show($"Журнал действий: {project.Name}");
            }
        }

        private void AddExtension_Click(object sender, RoutedEventArgs e)
        {
            if (sender is MenuItem menuItem && menuItem.Tag is ProjectViewModel project)
            {
                MessageBox.Show($"Добавить расширение для: {project.Name}");
            }
        }

        private void ArchiveProject_Click(object sender, RoutedEventArgs e)
        {
            if (sender is MenuItem menuItem && menuItem.Tag is ProjectViewModel project)
            {
                MessageBox.Show($"Архивировать проект: {project.Name}");
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
                            // Удаляем сначала все связанные задачи
                            var tasks = context.Task.Where(t => t.ProjectId == project.ProjectId).ToList();
                            context.Task.RemoveRange(tasks);
                            context.SaveChanges(); // Сначала удаляем задачи

                            // Теперь удаляем сам проект
                            context.Project.Remove(project);
                            context.SaveChanges();

                            // Удаляем из локального списка
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

    }
}