using ProjectSystemHelpStudents.Helper;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;

namespace ProjectSystemHelpStudents.UsersContent
{
    public partial class UpcomingTasksPage : Page
    {
        private DateTime _startOfWeek;

        public UpcomingTasksPage()
        {
            InitializeComponent();
            _startOfWeek = DateTime.Today;
            RefreshPage();
        }

        private void RefreshPage()
        {
            UpdateTodayDateText();
            UpdateWeekText();
            RefreshTasks();
            LoadWeekTimeline();
        }

        private void UpdateTodayDateText()
        {
            string todayDate = DateTime.Today.ToString("dd MMMM");
            string dayOfWeek = DateTime.Today.ToString("dddd");
            TodayDateTextBlock.Text = $"{todayDate} - Сегодня - {dayOfWeek}";
        }

        private void UpdateWeekText()
        {
            var endOfWeek = _startOfWeek.AddDays(6);
            CurrentWeekText.Text = $"{_startOfWeek:dd MMMM} - {endOfWeek:dd MMMM}";
        }

        private void RefreshTasks()
        {
            try
            {
                var allTasksFromDb = DBClass.entities.Task
                    .Where(t => t.Status.Name != "Завершено")
                    .Select(t => new
                    {
                        t.Title,
                        t.Description,
                        StatusName = t.Status != null ? t.Status.Name : "Без статуса",
                        t.EndDate
                    })
                    .ToList();

                var allTasks = allTasksFromDb
                    .Select(t => new TaskViewModel
                    {
                        Title = t.Title,
                        Description = t.Description,
                        Status = t.StatusName,
                        EndDate = t.EndDate,
                        IsCompleted = t.StatusName == "Завершено",
                        EndDateFormatted = t.EndDate.Date == DateTime.Today ? "Сегодня" :
                                           t.EndDate.Date == DateTime.Today.AddDays(1) ? "Завтра" :
                                           $"{t.EndDate:dd MMMM yyyy} - {t.EndDate:dddd}"
                    })
                    .OrderBy(t => t.EndDate)
                    .ToList();

                var overdueTasks = allTasks.Where(t => t.EndDate < DateTime.Today).ToList();
                OverdueTasksListView.ItemsSource = overdueTasks;

                var upcomingTasks = allTasks.Where(t => t.EndDate >= DateTime.Today).ToList();
                var groupedTasks = new List<TaskGroupViewModel>();

                foreach (var task in upcomingTasks)
                {
                    var group = groupedTasks.FirstOrDefault(g => g.DateHeader == task.EndDateFormatted);
                    if (group == null)
                    {
                        group = new TaskGroupViewModel { DateHeader = task.EndDateFormatted, Tasks = new List<TaskViewModel>() };
                        groupedTasks.Add(group);
                    }
                    group.Tasks.Add(task);
                }

                TasksListView.ItemsSource = groupedTasks;
            }
            catch (Exception ex)
            {
                MessageBox.Show("Ошибка при загрузке задач: " + ex.Message);
            }
        }

        private void LoadWeekTimeline()
        {
            WeekDaysTimeline.Items.Clear();
            for (int i = 0; i < 7; i++)
            {
                var day = _startOfWeek.AddDays(i);
                var dayButton = new Button
                {
                    Content = day.ToString("ddd dd"),
                    Width = 60,
                    Margin = new Thickness(5),
                    Background = GetButtonBackgroundColor(day),
                    Foreground = Brushes.Black,
                    BorderThickness = new Thickness(0),
                    IsEnabled = false
                };
                WeekDaysTimeline.Items.Add(dayButton);
            }
        }

        private Brush GetButtonBackgroundColor(DateTime day)
        {
            if (day == DateTime.Today) return Brushes.Red;
            else if (day < DateTime.Today) return Brushes.Gray;
            else return Brushes.LightGray;
        }

        private void Today_Click(object sender, RoutedEventArgs e)
        {
            _startOfWeek = DateTime.Today;
            RefreshPage();
        }

        private void PreviousWeek_Click(object sender, RoutedEventArgs e)
        {
            _startOfWeek = _startOfWeek.AddDays(-7);
            RefreshPage();
        }

        private void NextWeek_Click(object sender, RoutedEventArgs e)
        {
            _startOfWeek = _startOfWeek.AddDays(7);
            RefreshPage();
        }

        private void TaskListView_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (sender is ListView listView && listView.SelectedItem is TaskViewModel selectedTask)
            {
                var detailsWindow = new TaskDetailsWindow(selectedTask);
                detailsWindow.ShowDialog();
                listView.SelectedItem = null;
            }
        }

        private void ToggleTaskStatus_Click(object sender, RoutedEventArgs e)
        {
            var checkBox = sender as CheckBox;
            var task = checkBox.DataContext as TaskViewModel;

            if (task != null)
            {
                var dbTask = DBClass.entities.Task.FirstOrDefault(t => t.Title == task.Title);
                if (dbTask != null)
                {
                    dbTask.StatusId = (int)(checkBox.IsChecked == true ? DBClass.entities.Status.FirstOrDefault(s => s.Name == "Завершено")?.StatusId : DBClass.entities.Status.FirstOrDefault(s => s.Name == "Не завершено")?.StatusId);
                    DBClass.entities.SaveChanges();

                    RefreshTasks();
                }
            }
        }

        private void Button_Click(object sender, RoutedEventArgs e)
        {
            AddTaskWindow addTaskWindow = new AddTaskWindow();
            addTaskWindow.ShowDialog();
            RefreshTasks();
        }
    }
}
