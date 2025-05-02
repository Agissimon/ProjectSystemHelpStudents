using ProjectSystemHelpStudents.Helper;
using System;
using System.Collections.Generic;
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
                using (var context = new TaskManagementEntities1())
                {
                    var allTasks = context.Task
                        .Include("Status")
                        .Where(t => t.IdUser == UserSession.IdUser)
                        .ToList();

                    var vms = allTasks.Select(t => new TaskViewModel
                    {
                        IdTask = t.IdTask,
                        Title = t.Title,
                        Description = t.Description,
                        IsCompleted = t.Status != null && t.Status.Name == "Завершено",
                        EndDate = t.EndDate,
                        EndDateFormatted = t.EndDate != DateTime.MinValue
                            ? t.EndDate.ToString("dd MMMM yyyy")
                            : "Без срока"
                    })
                    .ToList();

                    var overdue = vms
                        .Where(vm => vm.EndDate != DateTime.MinValue
                                  && vm.EndDate.Date < DateTime.Today
                                  && !vm.IsCompleted)
                        .ToList();

                    var today = vms
                        .Where(vm => vm.EndDate.Date == DateTime.Today
                                  && !vm.IsCompleted)
                        .ToList();

                    OverdueTasksListView.ItemsSource = overdue;
                    TasksListView.ItemsSource = today;
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Ошибка при загрузке задач: " + ex.Message);
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
