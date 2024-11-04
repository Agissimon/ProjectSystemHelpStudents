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
            LoadTasks();
            LoadWeekTimeline();
        }

        private void UpdateTodayDateText()
        {
            // Форматируем сегодняшнюю дату в виде "29 октября - Сегодня - Вторник"
            string todayDate = DateTime.Today.ToString("dd MMMM");
            string dayOfWeek = DateTime.Today.ToString("dddd");

            TodayDateTextBlock.Text = $"{todayDate} - Сегодня - {dayOfWeek}";
        }

        private void UpdateWeekText()
        {
            var endOfWeek = _startOfWeek.AddDays(6);
            CurrentWeekText.Text = $"{_startOfWeek:dd MMMM} - {endOfWeek:dd MMMM}";
        }

        private void LoadTasks()
        {
            try
            {
                var allTasks = DBClass.entities.Task
                    .AsEnumerable()
                    .Select(t => new TaskViewModel
                    {
                        Title = t.Title,
                        Description = t.Description,
                        StatusTask = t.StatusTask,
                        EndDate = t.EndDate != DateTime.MinValue ? t.EndDate.ToString("dd MMMM yyyy") : "Без срока",
                        IsCompleted = t.StatusTask == "Завершено"
                    })
                    .ToList();

                var overdueTasks = allTasks
                    .Where(t => t.EndDate != "Без срока" && DateTime.Parse(t.EndDate) < DateTime.Today && !t.IsCompleted)
                    .ToList();

                var weekTasks = allTasks
                    .Where(t => t.EndDate != "Без срока" && DateTime.Parse(t.EndDate) >= _startOfWeek && DateTime.Parse(t.EndDate) < _startOfWeek.AddDays(7) && !t.IsCompleted)
                    .ToList();

                OverdueTasksListView.ItemsSource = overdueTasks;
                TasksListView.ItemsSource = weekTasks;
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
            if (day == DateTime.Today)
                return Brushes.Red;
            else if (day < DateTime.Today)
                return Brushes.Gray;
            else
                return Brushes.LightGray;
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
                    dbTask.StatusTask = checkBox.IsChecked == true ? "Завершено" : "В процессе";
                    DBClass.entities.SaveChanges();
                    LoadTasks();
                }
            }
        }

        private void Button_Click(object sender, RoutedEventArgs e)
        {
            AddTaskWindow addTaskWindow = new AddTaskWindow();
            addTaskWindow.ShowDialog();
        }
    }
}
