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
                    .Select(t => new TaskViewModel
                    {
                        Title = t.Title,
                        Description = t.Description,
                        Status = t.Status != null ? t.Status.Name : "Без статуса",
                        EndDate = t.EndDate,
                        IsCompleted = t.Status != null && t.Status.Name == "Завершено"
                    })
                    .ToList();

                foreach (var task in allTasks)
                {
                    task.EndDateFormatted = task.EndDate != DateTime.MinValue
                        ? task.EndDate.ToString("dd MMMM yyyy")
                        : "Без срока";
                }

                // Просроченные задачи
                var overdueTasks = allTasks
                    .Where(t => t.EndDateFormatted != "Без срока" && DateTime.Parse(t.EndDateFormatted) < DateTime.Today && !t.IsCompleted)
                    .ToList();

                // Задачи на текущую неделю
                var weekTasks = allTasks
                    .Where(t => t.EndDateFormatted != "Без срока" && DateTime.Parse(t.EndDateFormatted) >= _startOfWeek && DateTime.Parse(t.EndDateFormatted) < _startOfWeek.AddDays(7) && !t.IsCompleted)
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
                    dbTask.StatusId = (int)(checkBox.IsChecked == true
                        ? DBClass.entities.Status.FirstOrDefault(s => s.Name == "Завершено")?.StatusId
                        : DBClass.entities.Status.FirstOrDefault(s => s.Name == "В процессе")?.StatusId);

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