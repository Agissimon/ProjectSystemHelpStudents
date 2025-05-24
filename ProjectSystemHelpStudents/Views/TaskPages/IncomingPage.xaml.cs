using ProjectSystemHelpStudents.Helper;
using ProjectSystemHelpStudents.ViewModels;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Data.Entity;
using System.Windows.Media;

namespace ProjectSystemHelpStudents.UsersContent
{
    public partial class IncomingPage : Page
    {
        private int _inboxProjectId;

        public IncomingPage()
        {
            InitializeComponent();
            EnsureInboxProjectExists();
            RefreshSectionsAndTasks();
        }

        private void EnsureInboxProjectExists()
        {
            using (var ctx = new TaskManagementEntities1())
            {
                var inbox = ctx.Project
                    .FirstOrDefault(p => p.Name == "Входящие" && p.OwnerId == UserSession.IdUser);
                if (inbox == null)
                {
                    inbox = new Project { Name = "Входящие", OwnerId = UserSession.IdUser };
                    ctx.Project.Add(inbox);
                    ctx.SaveChanges();
                }
                _inboxProjectId = inbox.ProjectId;
            }
        }

        private void RefreshSectionsAndTasks()
        {
            var groups = new List<SectionTaskGroupViewModel>();

            using (var ctx = new TaskManagementEntities1())
            {
                // Задачи без раздела
                var unsectioned = ctx.Task
                    .Include(t => t.Status)
                    .Include(t => t.Priority)
                    .Where(t => t.ProjectId == _inboxProjectId && t.SectionId == null)
                    .OrderBy(t => t.EndDate)
                    .ToList()
                    .Select(t => new TaskViewModel
                    {
                        IdTask = t.IdTask,
                        Title = t.Title,
                        Description = t.Description,
                        IsCompleted = t.Status?.Name == "Завершено",
                        EndDate = t.EndDate,
                        PriorityId = t.PriorityId,
                    })
                    .ToList();

                if (unsectioned.Any())
                {
                    groups.Add(new SectionTaskGroupViewModel
                    {
                        SectionId = 0,
                        SectionName = "Без раздела",
                        Tasks = unsectioned
                    });
                }

                // Существующие разделы
                var sections = ctx.Section
                    .Where(s => s.ProjectId == _inboxProjectId)
                    .ToList();

                foreach (var sec in sections)
                {
                    var sectionTasks = ctx.Task
                        .Include(t => t.Status)
                        .Where(t => t.ProjectId == _inboxProjectId && t.SectionId == sec.IdSection)
                        .OrderBy(t => t.EndDate)
                        .ToList()
                        .Select(t => new TaskViewModel
                        {
                            IdTask = t.IdTask,
                            Title = t.Title,
                            Description = t.Description,
                            IsCompleted = t.Status?.Name == "Завершено",
                            EndDate = t.EndDate,
                            PriorityId = t.PriorityId
                        })
                        .ToList();

                    groups.Add(new SectionTaskGroupViewModel
                    {
                        SectionId = sec.IdSection,
                        SectionName = sec.Name,
                        Tasks = sectionTasks
                    });
                }
            }

            SectionsTasksControl.ItemsSource = groups;
        }

        private void OpenAddSectionPopup_Click(object sender, RoutedEventArgs e)
        {
            SectionNameTextBox.Text = string.Empty;
            AddSectionPopup.IsOpen = true;
        }

        private void ConfirmAddSection_Click(object sender, RoutedEventArgs e)
        {
            var name = SectionNameTextBox.Text.Trim();
            if (string.IsNullOrWhiteSpace(name))
            {
                MessageBox.Show("Введите название раздела.", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            using (var ctx = new TaskManagementEntities1())
            {
                ctx.Section.Add(new Section
                {
                    ProjectId = _inboxProjectId,
                    Name = name
                });
                ctx.SaveChanges();
            }

            AddSectionPopup.IsOpen = false;
            RefreshSectionsAndTasks();
        }

        private void CancelAddSection_Click(object sender, RoutedEventArgs e)
        {
            AddSectionPopup.IsOpen = false;
        }

        private void ButtonCreateTask_Click(object sender, RoutedEventArgs e)
        {
            if (sender is Button btn && btn.Tag is int secId)
            {
                int? sectionId = secId == 0 ? (int?)null : secId;
                var addWnd = new AddTaskWindow(_inboxProjectId, sectionId);
                addWnd.ShowDialog();
                RefreshSectionsAndTasks();
            }
        }

        private void TaskListView_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (sender is ListView lv && lv.SelectedItem is TaskViewModel vm)
            {
                var details = new TaskDetailsWindow(vm);
                details.TaskUpdated += RefreshSectionsAndTasks;
                details.ShowDialog();
                lv.SelectedItem = null;
            }
        }

        private void ToggleTaskStatus_Click(object sender, RoutedEventArgs e)
        {
            if (sender is CheckBox cb && cb.DataContext is TaskViewModel task)
            {
                using (var ctx = new TaskManagementEntities1())
                {
                    var dbTask = ctx.Task.Find(task.IdTask);
                    if (dbTask != null)
                    {
                        var done = ctx.Status.FirstOrDefault(s => s.Name == "Завершено");
                        var undone = ctx.Status.FirstOrDefault(s => s.Name == "Не завершено");
                        dbTask.StatusId = cb.IsChecked == true
                            ? done.StatusId
                            : undone.StatusId;
                        ctx.SaveChanges();
                    }
                }
                RefreshSectionsAndTasks();
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
                                RefreshSectionsAndTasks();
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
                            RefreshSectionsAndTasks();
                        }
                    }
                }
            }
        }
    }
}
