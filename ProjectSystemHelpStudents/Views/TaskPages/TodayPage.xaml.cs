using ProjectSystemHelpStudents.Helper;
using ProjectSystemHelpStudents.ViewModels;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Windows;
using System.Windows.Controls;

namespace ProjectSystemHelpStudents.UsersContent
{
    public partial class TodayPage : Page
    {
        public TodayPage()
        {
            InitializeComponent();
            UpdateTodayDateText();
            LoadTasks();

            var window = Window.GetWindow(this);
            if (window != null)
            {
                window.Closed += (s, e) => SaveExpanderState();
            }
        }

        private void SaveExpanderState()
        {
            Properties.Settings.Default.Save();
        }

        private void Expander_Expanded(object sender, RoutedEventArgs e)
        {
            Properties.Settings.Default.OverdueExpanderExpanded = true;
        }

        private void Expander_Collapsed(object sender, RoutedEventArgs e)
        {
            Properties.Settings.Default.OverdueExpanderExpanded = false;
        }

        private void UpdateTodayDateText()
        {
            string todayDate = DateTime.Today.ToString("dd MMMM");
            string dayOfWeek = DateTime.Today.ToString("dddd");
            TodayDateTextBlock.Text = $"{todayDate} · Сегодня · {dayOfWeek}";
        }

        private void LoadTasks()
        {
            try
            {
                using (var ctx = new TaskManagementEntities1())
                {
                    int userId = UserSession.IdUser;
                    var today = DateTime.Today;
                    var tomorrow = today.AddDays(1);

                    // Задачи за сегодня (EndDate >= сегодня 00:00 и < завтра 00:00)
                    var todayTasks = ctx.Task
                        .Include("Status")
                        .Include("Priority")
                        .Include("TaskLabels.Labels")
                        .Where(t =>
                            t.EndDate >= today && t.EndDate < tomorrow &&
                            t.Status.Name != "Завершено" &&
                            (t.CreatorId == userId
                             || t.TaskAssignee.Any(ta => ta.UserId == userId))
                        )
                        .ToList();

                    // Просроченные задачи (EndDate < сегодня 00:00)
                    var overdueTasks = ctx.Task
                        .Include("Status")
                        .Include("Priority")
                        .Include("TaskLabels.Labels")
                        .Where(t =>
                            t.EndDate < today &&
                            t.Status.Name != "Завершено" &&
                            (t.CreatorId == userId
                             || t.TaskAssignee.Any(ta => ta.UserId == userId))
                        )
                        .ToList();

                    // Проекция в VM
                    var todayVms = todayTasks
                        .Select(t => new TaskViewModel
                        {
                            IdTask = t.IdTask,
                            Title = t.Title,
                            Description = t.Description,
                            IsCompleted = t.Status.Name == "Завершено",
                            EndDate = t.EndDate,
                            EndDateFormatted = t.EndDate != DateTime.MinValue
                                                  ? t.EndDate.ToString("dd MMMM yyyy")
                                                  : "Без срока",
                            PriorityId = t.PriorityId,
                            AvailableLabels = new ObservableCollection<LabelViewModel>(
                                t.TaskLabels.Select(tl => new LabelViewModel
                                {
                                    Id = tl.Labels.Id,
                                    Name = tl.Labels.Name,
                                    HexColor = tl.Labels.Color,
                                    IsSelected = true
                                })
                            )
                        })
                        .ToList();

                    var overdueVms = overdueTasks
                        .Select(t => new TaskViewModel
                        {
                            IdTask = t.IdTask,
                            Title = t.Title,
                            Description = t.Description,
                            IsCompleted = t.Status.Name == "Завершено",
                            EndDate = t.EndDate,
                            EndDateFormatted = t.EndDate != DateTime.MinValue
                                                  ? t.EndDate.ToString("dd MMMM yyyy")
                                                  : "Без срока",
                            PriorityId = t.PriorityId,
                            AvailableLabels = new ObservableCollection<LabelViewModel>(
                                t.TaskLabels.Select(tl => new LabelViewModel
                                {
                                    Id = tl.Labels.Id,
                                    Name = tl.Labels.Name,
                                    HexColor = tl.Labels.Color,
                                    IsSelected = true
                                })
                            )
                        })
                        .ToList();

                    // Привязываем к ListView
                    OverdueTasksListView.ItemsSource = overdueVms;
                    TasksListView.ItemsSource = todayVms;

                    foreach (var vm in todayVms) vm.RefreshMarker();
                    foreach (var vm in overdueVms) vm.RefreshMarker();
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Ошибка при загрузке задач: " + ex.Message,
                                "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void TaskListView_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (!(sender is ListView listView && listView.SelectedItem is TaskViewModel selectedTask))
                return;

            var detailsWindow = new TaskDetailsWindow(selectedTask);
            detailsWindow.TaskUpdated += () =>
            {
                LoadTasks();
            };
            detailsWindow.ShowDialog();

            listView.SelectedItem = null;
        }

        private void ToggleTaskStatus_Click(object sender, RoutedEventArgs e)
        {
            var cb = (CheckBox)sender;
            var task = (TaskViewModel)cb.DataContext;
            if (task == null) return;

            using (var ctx = new TaskManagementEntities1())
            {
                var dbTask = ctx.Task.FirstOrDefault(t => t.IdTask == task.IdTask);
                if (dbTask == null) return;

                var doneStatus = ctx.Status.First(s => s.Name == "Завершено");
                var undoneStatus = ctx.Status.First(s => s.Name == "Не завершено");

                dbTask.StatusId = cb.IsChecked == true
                    ? doneStatus.StatusId
                    : undoneStatus.StatusId;

                ctx.SaveChanges();
            }

            LoadTasks();
        }

        private void Button_Click(object sender, RoutedEventArgs e)
        {
            var addTaskWindow = new AddTaskWindow(
                projectId: null,
                sectionId: null,
                preselectedDate: DateTime.Today
            );
            if (addTaskWindow.ShowDialog() == true)
                LoadTasks();
        }

    }
}
