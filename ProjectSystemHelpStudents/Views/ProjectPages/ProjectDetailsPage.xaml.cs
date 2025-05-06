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
                    int uid = UserSession.IdUser;

                    // Получаем проект
                    var project = context.Project.FirstOrDefault(p => p.ProjectId == _projectId);
                    if (project == null)
                        return;

                    // Проверяем, имеет ли пользователь доступ к проекту
                    var userTeamIds = context.TeamMember
                        .Where(tm => tm.UserId == uid)
                        .Select(tm => tm.TeamId)
                        .ToList();

                    bool hasAccess = project.OwnerId == uid ||
                                    (project.TeamId != null && userTeamIds.Contains(project.TeamId.Value));

                    if (!hasAccess)
                    {
                        MessageBox.Show("У вас нет доступа к этому проекту.");
                        return;
                    }

                    var sections = context.Section
                        .Where(s => s.ProjectId == _projectId)
                        .ToList();

                    var sectionTaskGroups = new List<SectionTaskGroupViewModel>();

                    foreach (var section in sections)
                    {
                        var tasks = context.Task
                            .Where(t => t.ProjectId == _projectId && t.SectionId == section.IdSection)
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

        private void EditSection_Click(object sender, RoutedEventArgs e)
        {
            if (sender is Button button && button.Tag is int sectionId)
            {
                var dialog = new InputDialog("Введите новое название раздела:")
                {
                    TitleText = "Редактирование раздела",
                    PlaceholderText = "Введите название нового раздела"
                };

                if (dialog.ShowDialog() == true)
                {
                    var newSectionName = dialog.InputText;
                    if (!string.IsNullOrWhiteSpace(newSectionName))
                    {
                        using (var context = new TaskManagementEntities1())
                        {
                            var section = context.Section.FirstOrDefault(s => s.IdSection == sectionId);
                            if (section != null)
                            {
                                section.Name = newSectionName;
                                context.SaveChanges();
                                RefreshTasks();
                            }
                        }
                    }
                }
            }
        }

        private void DeleteSection_Click(object sender, RoutedEventArgs e)
        {
            if (sender is Button button && button.Tag is int sectionId)
            {
                var result = MessageBox.Show("Вы уверены, что хотите удалить этот раздел и все его задачи?", "Подтверждение удаления", MessageBoxButton.YesNo, MessageBoxImage.Warning);
                if (result == MessageBoxResult.Yes)
                {
                    using (var context = new TaskManagementEntities1())
                    {
                        var section = context.Section.FirstOrDefault(s => s.IdSection == sectionId);
                        if (section != null)
                        {
                            var tasks = context.Task.Where(t => t.SectionId == sectionId).ToList();

                            foreach (var task in tasks)
                            {
                                var labels = context.TaskLabels.Where(tl => tl.TaskId == task.IdTask).ToList();
                                context.TaskLabels.RemoveRange(labels);

                                var comments = context.Comment.Where(c => c.IdTask == task.IdTask).ToList();
                                context.Comment.RemoveRange(comments);

                                var files = context.Files.Where(f => f.TaskId == task.IdTask).ToList();
                                context.Files.RemoveRange(files);
                            }

                            context.Task.RemoveRange(tasks);

                            context.Section.Remove(section);

                            context.SaveChanges();
                            RefreshTasks();
                        }
                    }
                }
            }
        }

    }
}
