using ProjectSystemHelpStudents.Helper;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using ProjectSystemHelpStudents.Views;
using System.Globalization;
using ProjectSystemHelpStudents.ViewModels;

namespace ProjectSystemHelpStudents.UsersContent
{
    public partial class UpcomingTasksPage : Page
    {
        private DateTime _startOfWeek;
        private ObservableCollection<TaskGroupViewModel> _groupedTasks;
        private bool _isDateManuallyChanged = false;

        public UpcomingTasksPage()
        {
            InitializeComponent();
            _groupedTasks = new ObservableCollection<TaskGroupViewModel>();
            TasksListView.ItemsSource = _groupedTasks;
            _startOfWeek = DateTime.Today;
            Loaded += UpcomingTasksPage_Loaded;
        }
        private void MonthDayPicker_CalendarClosed(object sender, RoutedEventArgs e)
        {
            _isDateManuallyChanged = true;
        }

        private void UpcomingTasksPage_Loaded(object sender, RoutedEventArgs e)
        {
            if (TasksListView != null)
            {
                RefreshPage();
                RestoreExpandersState();
                if (MonthDayPicker.SelectedDate.HasValue)
                {
                    UpdateMonthText(MonthDayPicker.SelectedDate.Value);
                }
                else
                {
                    MonthDayPicker.SelectedDate = DateTime.Today;
                    UpdateMonthText(DateTime.Today);
                }
            }
        }

        private void RefreshPage()
        {
            LoadFilters();

            SortComboBox.SelectedIndex = Properties.Settings.Default.SortOption;
            ExecutorComboBox.SelectedValue = Properties.Settings.Default.ExecutorFilter;
            PriorityComboBox.SelectedValue = Properties.Settings.Default.PriorityFilter;
            LabelComboBox.SelectedValue = Properties.Settings.Default.LabelFilter;

            UpdateTodayDateText();
            RefreshTasks();
            LoadWeekTimeline();

            RestoreExpandersState();
        }

        private void LoadFilters()
        {
            using (var ctx = new TaskManagementEntities1())
            {
                // Сортировка
                SortComboBox.ItemsSource = new List<string> { "Умная", "Дата", "Приоритет" };

                // Исполнители
                var executors = ctx.Users
                    .Select(u => new ExecutorViewModel { IdUser = u.IdUser, FullName = u.Name })
                    .ToList();
                executors.Insert(0, new ExecutorViewModel { IdUser = 0, FullName = "Все" });
                ExecutorComboBox.ItemsSource = executors;

                // Приоритеты
                var priorities = ctx.Priority
                    .Select(p => new PriorityViewModel { PriorityId = p.PriorityId, Name = p.Name })
                    .OrderBy(p => p.PriorityId)
                    .ToList();
                priorities.Insert(0, new PriorityViewModel { PriorityId = 0, Name = "Все" });
                PriorityComboBox.ItemsSource = priorities;

                // Метки
                var labels = ctx.Labels
                    .Select(l => new LabelViewModel { Id = l.Id, Name = l.Name })
                    .OrderBy(l => l.Id)
                    .ToList();
                labels.Insert(0, new LabelViewModel { Id = 0, Name = "Все" });
                LabelComboBox.ItemsSource = labels;
            }
        }

        private void RefreshTasks()
        {
            string currentMode = Properties.Settings.Default.LastViewMode;
            if (_groupedTasks == null)
                _groupedTasks = new ObservableCollection<TaskGroupViewModel>();

            try
            {
                using (var ctx = new TaskManagementEntities1())
                {
                    int selExecutor = (int)(ExecutorComboBox.SelectedValue ?? 0);
                    int selPriority = (int)(PriorityComboBox.SelectedValue ?? 0);
                    int selLabel = (int)(LabelComboBox.SelectedValue ?? 0);
                    int sortOption = Properties.Settings.Default.SortOption;

                    var query = ctx.Task
                        .Include("Status")
                        .Include("Priority")
                        .Include("TaskLabels.Labels")
                        .Where(t => t.Status.Name != "Завершено"
                                 && t.IdUser == UserSession.IdUser);

                    //if (selExecutor > 0)
                    //    query = query.Where(t => t.ExecutorId == selExecutor);
                    if (selPriority > 0)
                        query = query.Where(t => t.PriorityId == selPriority);
                    if (selLabel > 0)
                        query = query.Where(t => t.TaskLabels.Any(tl => tl.LabelId == selLabel));

                    switch (sortOption)
                    {
                        case 0:
                            query = query.OrderBy(t => t.EndDate);
                            break;
                        case 1:
                            query = query.OrderBy(t => t.Priority.PriorityId);
                            break;
                        default:
                            query = query
                                .OrderByDescending(t => t.EndDate)
                                .ThenBy(t => t.Priority.PriorityId);
                            break;
                    }

                    var allTasks = query
                        .ToList()
                        .Select(t =>
                        {
                            string hdr;
                            if (t.EndDate.Date == DateTime.Today)
                                hdr = $"{DateTime.Today:dd MMMM} ‧ Сегодня ‧ {DateTime.Today:dddd}";
                            else if (t.EndDate.Date == DateTime.Today.AddDays(1))
                                hdr = $"{DateTime.Today.AddDays(1):dd MMMM} ‧ Завтра ‧ {DateTime.Today.AddDays(1):dddd}";
                            else
                                hdr = $"{t.EndDate:dd MMMM} ‧ {t.EndDate:dddd}";

                            return new TaskViewModel
                            {
                                IdTask = t.IdTask,
                                Title = t.Title,
                                Description = t.Description,
                                Status = t.Status?.Name,
                                EndDate = t.EndDate,
                                IsCompleted = t.Status?.Name == "Завершено",
                                EndDateFormatted = hdr,
                                AvailableLabels = new ObservableCollection<LabelViewModel>(
                                    t.TaskLabels.Select(l => new LabelViewModel
                                    {
                                        Id = l.LabelId,
                                        Name = l.Labels.Name
                                    }))
                            };
                        })
                        .ToList();

                    _groupedTasks.Clear();
                    foreach (var vm in allTasks.Where(x => x.EndDate.Date >= DateTime.Today))
                    {
                        var grp = _groupedTasks.FirstOrDefault(g => g.DateHeader == vm.EndDateFormatted);
                        if (grp == null)
                        {
                            grp = new TaskGroupViewModel { DateHeader = vm.EndDateFormatted };
                            grp.Expander = new Expander
                            {
                                Header = grp.DateHeader,
                                FontSize = 18,
                                Foreground = Brushes.White,
                                Margin = new Thickness(0, 10, 0, 0),
                                Content = new ListBox
                                {
                                    ItemsSource = grp.Tasks,
                                    Style = (Style)FindResource("TransparentListViewStyle")
                                }
                            };
                            grp.Expander.Expanded += Expander_ExpandedCollapsed;
                            grp.Expander.Collapsed += Expander_ExpandedCollapsed;
                            _groupedTasks.Add(grp);
                        }
                        grp.Tasks.Add(vm);
                    }

                    TasksListView.ItemsSource = _groupedTasks;
                    OverdueTasksListView.ItemsSource = allTasks.Where(x => x.EndDate.Date < DateTime.Today).ToList();

                    var board = TaskBoardView.CreateBoardView(allTasks);
                    var calendar = TaskCalendarView.CreateCalendarView(allTasks);
                    BoardViewSection.Children.Clear();
                    BoardViewSection.Children.Add(board);
                    CalendarViewSection.Children.Clear();
                    CalendarViewSection.Children.Add(calendar);

                    switch (currentMode)
                    {
                        case "Board": ShowBoardView(); break;
                        case "Calendar": ShowCalendarView(); break;
                        default: ShowListView(); break;
                    }

                    Dispatcher.BeginInvoke((Action)RestoreExpandersState,
                        System.Windows.Threading.DispatcherPriority.Background);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка при загрузке задач: {ex.Message}",
                                "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void UpdateMonthText(DateTime date)
        {
            string monthYear = date.ToString("MMMM yyyy", new CultureInfo("ru-RU"));
            MonthTextBlock.Text = char.ToUpper(monthYear[0]) + monthYear.Substring(1);
        }

        private void MonthPickerButton_Click(object sender, RoutedEventArgs e)
        {
            MonthDayPicker.Focus();
            MonthDayPicker.IsDropDownOpen = true;
        }

        private void MonthDayPicker_SelectedDateChanged(object sender, SelectionChangedEventArgs e)
        {
            if (!_isDateManuallyChanged)
                return;

            if (MonthDayPicker.SelectedDate is DateTime selectedDate)
            {
                _startOfWeek = StartOfWeek(selectedDate);
                RefreshPage();

                // Отображаем только задачи за выбранную дату
                var filteredGroups = new ObservableCollection<TaskGroupViewModel>();
                foreach (var group in _groupedTasks)
                {
                    var matchingTasks = group.Tasks
                        .Where(task => task.EndDate.Date == selectedDate.Date)
                        .ToList();

                    if (matchingTasks.Any())
                    {
                        filteredGroups.Add(new TaskGroupViewModel
                        {
                            DateHeader = group.DateHeader,
                            Tasks = new ObservableCollection<TaskViewModel>(matchingTasks)
                        });
                    }
                }
                TasksListView.ItemsSource = filteredGroups;
            }
        }

        private DateTime StartOfWeek(DateTime date)
        {
            int diff = (7 + (date.DayOfWeek - DayOfWeek.Monday)) % 7;
            return date.AddDays(-1 * diff).Date;
        }

        private void UpdateTodayDateText()
        {
            string todayDate = DateTime.Today.ToString("dd MMMM");
            string dayOfWeek = DateTime.Today.ToString("dddd");
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
                    IsEnabled = true,
                    Tag = day
                };

                dayButton.Click += DayButton_Click;
                WeekDaysTimeline.Items.Add(dayButton);
            }
        }

        private void DayButton_Click(object sender, RoutedEventArgs e)
        {
            if (sender is Button button && button.Tag is DateTime selectedDate)
            {
                var filteredGroups = new ObservableCollection<TaskGroupViewModel>();
                foreach (var group in _groupedTasks)
                {
                    var matchingTasks = group.Tasks
                        .Where(task => task.EndDate.Date == selectedDate.Date)
                        .ToList();

                    if (matchingTasks.Any())
                    {
                        filteredGroups.Add(new TaskGroupViewModel
                        {
                            DateHeader = group.DateHeader,
                            Tasks = new ObservableCollection<TaskViewModel>(matchingTasks)
                        });
                    }
                }
                TasksListView.ItemsSource = filteredGroups;
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
                detailsWindow.TaskUpdated += () => RefreshTasks();
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
                        if (checkBox.IsChecked == true)
                        {
                            var completedStatus = DBClass.entities.Status.FirstOrDefault(s => s.Name == "Завершено");
                            if (completedStatus != null)
                                dbTask.StatusId = completedStatus.StatusId;
                        }
                        else
                        {
                            var notCompletedStatus = DBClass.entities.Status.FirstOrDefault(s => s.Name == "Не завершено");
                            if (notCompletedStatus != null)
                                dbTask.StatusId = notCompletedStatus.StatusId;
                        }
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

        private void DisplayOptionsButton_Click(object sender, RoutedEventArgs e)
        {
            DisplayOptionsMenu.IsOpen = !DisplayOptionsMenu.IsOpen;
        }

        private void SortComboBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            Properties.Settings.Default.SortOption = SortComboBox.SelectedIndex;
            Properties.Settings.Default.Save();
            RefreshTasks();
        }
        private void ExecutorComboBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            Properties.Settings.Default.ExecutorFilter = (int)(ExecutorComboBox.SelectedValue ?? 0);
            Properties.Settings.Default.Save();
            RefreshTasks();
        }
        private void PriorityComboBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            Properties.Settings.Default.PriorityFilter = (int)(PriorityComboBox.SelectedValue ?? 0);
            Properties.Settings.Default.Save();
            RefreshTasks();
        }
        private void LabelComboBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            Properties.Settings.Default.LabelFilter = (int)(LabelComboBox.SelectedValue ?? 0);
            Properties.Settings.Default.Save();
            RefreshTasks();
        }

        private void ShowListView()
        {
            if (ListViewSection == null || CalendarViewSection == null || BoardViewSection == null)
                return;
            ListViewSection.Visibility = Visibility.Visible;
            CalendarViewSection.Visibility = Visibility.Collapsed;
            BoardViewSection.Visibility = Visibility.Collapsed;
        }

        private void ShowCalendarView()
        {
            if (ListViewSection == null || CalendarViewSection == null || BoardViewSection == null)
                return;
            ListViewSection.Visibility = Visibility.Collapsed;
            CalendarViewSection.Visibility = Visibility.Visible;
            BoardViewSection.Visibility = Visibility.Collapsed;
        }

        private void ShowBoardView()
        {
            if (ListViewSection == null || CalendarViewSection == null || BoardViewSection == null)
                return;
            ListViewSection.Visibility = Visibility.Collapsed;
            CalendarViewSection.Visibility = Visibility.Collapsed;
            BoardViewSection.Visibility = Visibility.Visible;
        }

        private void ListTab_Click(object sender, RoutedEventArgs e)
        {
            Properties.Settings.Default.LastViewMode = "List";
            Properties.Settings.Default.Save();
            ShowListView();
        }

        private void CalendarTab_Click(object sender, RoutedEventArgs e)
        {
            Properties.Settings.Default.LastViewMode = "Calendar";
            Properties.Settings.Default.Save();
            ShowCalendarView();
        }

        private void BoardTab_Click(object sender, RoutedEventArgs e)
        {
            Properties.Settings.Default.LastViewMode = "Board";
            Properties.Settings.Default.Save();
            ShowBoardView();
        }

        private void Expander_ExpandedCollapsed(object sender, RoutedEventArgs e)
        {
            SaveExpandedGroupsState();
        }

        private void SaveExpandedGroupsState()
        {

            var expanded = new List<string>();

            if (OverdueTasksExpander != null && OverdueTasksExpander.IsExpanded)
            {
                expanded.Add("Просрочено");
            }

            if (TasksListView != null)
            {
                foreach (var item in TasksListView.Items)
                {
                    if (item is TaskGroupViewModel group && group.Expander != null && group.Expander.IsExpanded)
                    {
                        expanded.Add(group.DateHeader);
                    }
                }
            }

            Properties.Settings.Default.ExpandersState = string.Join(";", expanded);
            Properties.Settings.Default.Save();
        }

        private void RestoreExpandersState()
        {
            string savedState = Properties.Settings.Default.ExpandersState;

            if (!string.IsNullOrEmpty(savedState))
            {
                var expandedHeaders = savedState.Split(';');

                if (OverdueTasksExpander != null)
                {
                    OverdueTasksExpander.IsExpanded = expandedHeaders.Contains("Просрочено");
                }

                if (TasksListView != null)
                {
                    foreach (var item in TasksListView.Items)
                    {
                        if (item is TaskGroupViewModel group && group.Expander != null)
                        {
                            group.Expander.IsExpanded = expandedHeaders.Contains(group.DateHeader);
                        }
                    }
                }
            }
        }
    }
}