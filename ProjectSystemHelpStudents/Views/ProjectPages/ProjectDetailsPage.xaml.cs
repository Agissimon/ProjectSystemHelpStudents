using ProjectSystemHelpStudents.Helper;
using ProjectSystemHelpStudents.ViewModels;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows;
using System.Windows.Controls;

namespace ProjectSystemHelpStudents.UsersContent
{
    public partial class ProjectDetailsPage : Page
    {
        private int _projectId;

        public ProjectDetailsPage(int projectId)
        {
            InitializeComponent();
            _projectId = projectId;
            LoadProjectDetails();
            RefreshTasks();
        }

        private void LoadProjectDetails()
        {
            using (var context = new TaskManagementEntities1())
            {
                var project = context.Project.FirstOrDefault(p => p.ProjectId == _projectId);
                if (project != null)
                {
                    ProjectTitle.Text = project.Name;
                    ProjectDescription.Text = project.Description;
                    ProjectDates.Text = $"С {project.StartDate:dd.MM.yyyy} по {project.EndDate:dd.MM.yyyy}";
                }
            }
        }

        private void RefreshTasks()
        {
            try
            {
                using (var context = new TaskManagementEntities1())
                {
                    var sections = context.Section
                        .Where(s => s.ProjectId == _projectId)
                        .ToList();

                    var sectionTaskGroups = new List<SectionTaskGroupViewModel>();

                    foreach (var section in sections)
                    {
                        var tasks = context.Task
                            .Where(t => t.ProjectId == _projectId && t.SectionId == section.IdSection && t.IdUser == UserSession.IdUser)
                            .ToList()
                            .Select(t => new TaskViewModel
                            {
                                IdTask = t.IdTask,
                                Title = t.Title,
                                Description = t.Description,
                                Status = t.Status?.Name,
                                EndDate = t.EndDate,
                                IsCompleted = t.Status?.Name == "Завершено",
                                EndDateFormatted = t.EndDate != DateTime.MinValue ? t.EndDate.ToString("dd MMMM yyyy") : "Без срока"
                            })
                            .ToList();

                        sectionTaskGroups.Add(new SectionTaskGroupViewModel
                        {
                            SectionId = section.IdSection,
                            SectionName = section.Name,
                            Tasks = tasks
                        });
                    }

                    SectionsTasksControl.ItemsSource = sectionTaskGroups;
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Ошибка при загрузке задач: " + ex.Message);
            }
        }

        private void TaskListView_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (sender is ListView listView && listView.SelectedItem is TaskViewModel task)
            {
                var detailsWindow = new TaskDetailsWindow(task);
                detailsWindow.TaskUpdated += RefreshTasks;
                detailsWindow.ShowDialog();
                listView.SelectedItem = null;
            }
        }

        private void ToggleTaskStatus_Click(object sender, RoutedEventArgs e)
        {
            if (sender is CheckBox checkBox && checkBox.DataContext is TaskViewModel task)
            {
                try
                {
                    using (var context = new TaskManagementEntities1())
                    {
                        var dbTask = context.Task.FirstOrDefault(t => t.IdTask == task.IdTask);
                        if (dbTask != null)
                        {
                            dbTask.StatusId = checkBox.IsChecked == true
                                ? context.Status.FirstOrDefault(s => s.Name == "Завершено")?.StatusId ?? dbTask.StatusId
                                : context.Status.FirstOrDefault(s => s.Name == "Не завершено")?.StatusId ?? dbTask.StatusId;

                            context.SaveChanges();
                            RefreshTasks();
                        }
                    }
                }
                catch (Exception ex)
                {
                    MessageBox.Show("Ошибка обновления статуса: " + ex.Message);
                }
            }
        }

        private void AddTaskToSection_Click(object sender, RoutedEventArgs e)
        {
            if (sender is Button button && button.Tag is int sectionId)
            {
                var addTaskWindow = new AddTaskWindow(_projectId, sectionId);
                addTaskWindow.ShowDialog();
                RefreshTasks();
            }
        }

        private void AddSection_Click(object sender, RoutedEventArgs e)
        {
            var dialog = new InputDialog("Введите название раздела:");
            if (dialog.ShowDialog() == true)
            {
                var sectionName = dialog.InputText;
                if (!string.IsNullOrWhiteSpace(sectionName))
                {
                    using (var context = new TaskManagementEntities1())
                    {
                        var newSection = new Section
                        {
                            ProjectId = _projectId,
                            Name = sectionName
                        };
                        context.Section.Add(newSection);
                        context.SaveChanges();
                    }
                    RefreshTasks();
                }
            }
        }
        private void OpenAddSectionPopup_Click(object sender, RoutedEventArgs e)
        {
            SectionNameTextBox.Text = string.Empty;
            AddSectionPopup.IsOpen = true;
        }

        private void CancelAddSection_Click(object sender, RoutedEventArgs e)
        {
            AddSectionPopup.IsOpen = false;
        }

        private void ConfirmAddSection_Click(object sender, RoutedEventArgs e)
        {
            var sectionName = SectionNameTextBox.Text.Trim();
            if (!string.IsNullOrWhiteSpace(sectionName))
            {
                using (var context = new TaskManagementEntities1())
                {
                    var newSection = new Section
                    {
                        ProjectId = _projectId,
                        Name = sectionName
                    };
                    context.Section.Add(newSection);
                    context.SaveChanges();
                }

                AddSectionPopup.IsOpen = false;
                RefreshTasks();
            }
            else
            {
                MessageBox.Show("Введите название раздела.");
            }
        }
    }
}
