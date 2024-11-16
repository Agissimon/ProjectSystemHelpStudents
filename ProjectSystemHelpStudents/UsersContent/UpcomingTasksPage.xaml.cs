using ProjectSystemHelpStudents.Helper;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;

namespace ProjectSystemHelpStudents.UsersContent
{
    public partial class UpcomingTasksPage : Page
    {
        private DateTime _startOfWeek;
        private ObservableCollection<TaskGroupViewModel> _groupedTasks;

        public UpcomingTasksPage()
        {
            InitializeComponent();
            _groupedTasks = new ObservableCollection<TaskGroupViewModel>();
            TasksListView.ItemsSource = _groupedTasks;
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

        private void RefreshTasks()
        {
            try
            {
                using (var context = new TaskManagementEntities1())
                {
                    var allTasksFromDb = context.Task
                        .Where(t => t.Status.Name != "Завершено" && t.IdUser == UserSession.IdUser)
                        .Select(t => new
                        {
                            t.IdTask,
                            t.Title,
                            t.Description,
                            StatusName = t.Status != null ? t.Status.Name : "Без статуса",
                            t.EndDate
                        })
                        .ToList();

                    var allTasks = allTasksFromDb
                        .Select(t => new TaskViewModel
                        {
                            IdTask = t.IdTask,
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

                    _groupedTasks.Clear();
                    var upcomingTasks = allTasks.Where(t => t.EndDate >= DateTime.Today).ToList();
                    foreach (var task in upcomingTasks)
                    {
                        var group = _groupedTasks.FirstOrDefault(g => g.DateHeader == task.EndDateFormatted);
                        if (group == null)
                        {
                            group = new TaskGroupViewModel
                            {
                                DateHeader = task.EndDateFormatted
                            };
                            _groupedTasks.Add(group);
                        }
                        group.Tasks.Add(task);
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Ошибка при загрузке задач: " + ex.Message);
            }
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

        private void LoadWeekTimeline()
        {
            WeekDaysTimeline.Items.Clear();
            for (int i = 0; i < 7; i++)
            {
                var day = _startOfWeek.AddDays(i);

                Brush textColor = day == DateTime.Today ? Brushes.Red : Brushes.White;

                var dayButton = new Button
                {
                    Content = new TextBlock
                    {
                        Text = day.ToString("ddd dd"),
                        Foreground = textColor,
                        HorizontalAlignment = HorizontalAlignment.Center,
                        VerticalAlignment = VerticalAlignment.Center
                    },
                    Width = 60,
                    Margin = new Thickness(5),
                    Background = Brushes.Transparent,
                    BorderThickness = new Thickness(1),
                    IsEnabled = true
                };

                WeekDaysTimeline.Items.Add(dayButton);
            }
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
            if (sender is ListView listView && listView.SelectedItem is TaskViewModel task)
            {
                var detailsWindow = new TaskDetailsWindow(task);

                detailsWindow.TaskUpdated += () =>
                {
                    RefreshTasks();
                };

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
                    var dbTask = DBClass.entities.Task.FirstOrDefault(t => t.IdTask == task.IdTask);
                    if (dbTask != null)
                    {
                        dbTask.StatusId = checkBox.IsChecked == true
                            ? DBClass.entities.Status.FirstOrDefault(s => s.Name == "Завершено")?.StatusId ?? dbTask.StatusId
                            : DBClass.entities.Status.FirstOrDefault(s => s.Name == "Не завершено")?.StatusId ?? dbTask.StatusId;

                        DBClass.entities.SaveChanges();
                        RefreshTasks();
                    }
                }
                catch (Exception ex)
                {
                    MessageBox.Show("Ошибка обновления статуса: " + ex.Message);
                }
            }
        }

        private void AddTask_Click(object sender, RoutedEventArgs e)
        {
            var addTaskWindow = new AddTaskWindow();
            addTaskWindow.ShowDialog();
            RefreshTasks();
        }
    }
}
