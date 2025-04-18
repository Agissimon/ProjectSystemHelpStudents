﻿using ProjectSystemHelpStudents.Helper;
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
                        var isDetached = UserSettingsHelper.IsDetached(project.ProjectId);
                        Projects.Add(new ProjectViewModel
                        {
                            ProjectId = project.ProjectId,
                            Name = project.Name,
                            Icon = "📁",
                            IsDetached = isDetached // Помечаем, если проект в избранном
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

                    // Обновим StackPanel
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
                    // Открытие окна редактирования с переданным ProjectId
                    AddProjectWindow editWindow = new AddProjectWindow
                    {
                        ProjectId = project.ProjectId
                    };

                    // Обработка закрытия окна редактирования
                    editWindow.ProjectAdded += (newProject) =>
                    {
                        // Обновляем список проектов после редактирования
                        Dispatcher.Invoke(() => RefreshProjects());
                    };

                    editWindow.ShowDialog();

                    // После того как проект обновлен, обновляем список
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
