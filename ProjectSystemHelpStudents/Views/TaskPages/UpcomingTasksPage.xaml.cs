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

namespace ProjectSystemHelpStudents.UsersContent
{
    public partial class UpcomingTasksPage : Page
    {
        private DateTime _startOfWeek;
        private ObservableCollection<TaskGroupViewModel> _groupedTasks;
        private bool _isRefreshingTasks = false;
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

                // Восстанавливаем состояние экспандеров
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
            SortComboBox.SelectedIndex = Properties.Settings.Default.SortOption;
            ExecutorComboBox.SelectedIndex = Properties.Settings.Default.ExecutorFilter;
            PriorityComboBox.SelectedIndex = Properties.Settings.Default.PriorityFilter;
            LabelComboBox.SelectedIndex = Properties.Settings.Default.LabelFilter;

            UpdateTodayDateText();
            //UpdateWeekText();
            RefreshTasks();
            LoadWeekTimeline();

            RestoreExpandersState();
        }

        private void RefreshTasks()
        {
            // Выбираем активный режим отображения до загрузки данных,
            // чтобы при обновлении (например, после добавления) сразу отобразился нужный контейнер.
            string currentMode = Properties.Settings.Default.LastViewMode;

            if (_groupedTasks == null)
                _groupedTasks = new ObservableCollection<TaskGroupViewModel>();

            try
            {
                using (var context = new TaskManagementEntities1())
                {
                    int selectedPriority = Properties.Settings.Default.PriorityFilter;
                    int selectedLabel = Properties.Settings.Default.LabelFilter;
                    int selectedExecutorIndex = Properties.Settings.Default.ExecutorFilter;
                    int sortOption = Properties.Settings.Default.SortOption;

                    var query = context.Task
                        .Where(t => t.Status.Name != "Завершено" && t.IdUser == UserSession.IdUser);

                    // Фильтр по приоритету
                    if (selectedPriority > 0)
                    {
                        var selectedPriorityName = context.Priority
                            .OrderBy(p => p.PriorityId)
                            .Skip(selectedPriority - 1)
                            .Select(p => p.Name)
                            .FirstOrDefault();

                        if (!string.IsNullOrEmpty(selectedPriorityName))
                        {
                            query = query.Where(t => t.Priority.Name == selectedPriorityName);
                        }
                    }

                    // Фильтр по метке
                    if (selectedLabel > 0)
                    {
                        var selectedLabelName = context.Labels
                            .OrderBy(l => l.Id)
                            .Skip(selectedLabel - 1)
                            .Select(l => l.Name)
                            .FirstOrDefault();

                        if (!string.IsNullOrEmpty(selectedLabelName))
                        {
                            query = query.Where(t => t.TaskLabels.Any(tl => tl.Labels.Name == selectedLabelName));
                        }
                    }

                    // Фильтр по исполнителю 
                    if (selectedExecutorIndex > 0)
                    {
                        var selectedExecutorItem = (ComboBoxItem)ExecutorComboBox.Items[selectedExecutorIndex];
                        if (selectedExecutorItem?.Tag is int executorId)
                        {
                            // query = query.Where(t => t.ExecutorId == executorId);
                        }
                    }

                    // Сортировка
                    switch (sortOption)
                    {
                        case 1:
                            query = query.OrderBy(t => t.EndDate);
                            break;
                        case 2:
                            query = query.OrderBy(t => t.Priority.PriorityId);
                            break;
                        default:
                            query = query.OrderBy(t => t.EndDate);
                            break;
                    }

                    var tasksFromDb = query.ToList();
                    List<TaskViewModel> allTasks = new List<TaskViewModel>();

                    foreach (var t in tasksFromDb)
                    {
                        string formattedDate;
                        if (t.EndDate.Date == DateTime.Today)
                        {
                            formattedDate = string.Format("{0:dd MMMM} ‧ Сегодня ‧ {1:dddd}", DateTime.Today, DateTime.Today);
                        }
                        else if (t.EndDate.Date == DateTime.Today.AddDays(1))
                        {
                            formattedDate = string.Format("{0:dd MMMM} ‧ Завтра ‧ {1:dddd}", DateTime.Today.AddDays(1), DateTime.Today.AddDays(1));
                        }
                        else
                        {
                            formattedDate = string.Format("{0:dd MMMM} ‧ {0:dddd}", t.EndDate);
                        }

                        allTasks.Add(new TaskViewModel
                        {
                            IdTask = t.IdTask,
                            Title = t.Title,
                            Description = t.Description,
                            Status = t.Status?.Name,
                            EndDate = t.EndDate,
                            IsCompleted = t.Status?.Name == "Завершено",
                            AvailableLabels = new ObservableCollection<LabelViewModel>(
                                t.TaskLabels.Select(l => new LabelViewModel { Name = l.Labels.Name })),
                            EndDateFormatted = formattedDate,
                        });
                    }

                    _groupedTasks.Clear();
                    var upcomingTasks = allTasks.Where(t => t.EndDate >= DateTime.Today).ToList();
                    foreach (var task in upcomingTasks)
                    {
                        var group = _groupedTasks.FirstOrDefault(g => g.DateHeader == task.EndDateFormatted);
                        if (group == null)
                        {
                            group = new TaskGroupViewModel { DateHeader = task.EndDateFormatted };
                            _groupedTasks.Add(group);
                        }
                        group.Tasks.Add(task);
                    }

                    if (TasksListView != null)
                    {
                        TasksListView.ItemsSource = _groupedTasks;

                        Dispatcher.BeginInvoke(new Action(() =>
                        {
                            RestoreExpandersState();
                        }), System.Windows.Threading.DispatcherPriority.Background);
                    }

                    if (OverdueTasksListView != null)
                        OverdueTasksListView.ItemsSource = allTasks.Where(t => t.EndDate < DateTime.Today).ToList();

                    Grid newBoard = TaskBoardView.CreateBoardView(allTasks);
                    newBoard.Tag = allTasks;
                    Grid newCalendar = TaskCalendarView.CreateCalendarView(allTasks);
                    newCalendar.Tag = allTasks;

                    if (BoardViewSection != null)
                    {
                        BoardViewSection.Children.Clear();
                        BoardViewSection.Children.Add(newBoard);
                    }
                    if (CalendarViewSection != null)
                    {
                        CalendarViewSection.Children.Clear();
                        CalendarViewSection.Children.Add(newCalendar);
                    }

                    switch (currentMode)
                    {
                        case "List":
                            ShowListView();
                            break;
                        case "Calendar":
                            ShowCalendarView();
                            break;
                        case "Board":
                            ShowBoardView();
                            break;
                        default:
                            ShowListView();
                            break;
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Ошибка при загрузке задач: " + ex.Message);
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

        //private void UpdateWeekText()
        //{
        //    var endOfWeek = _startOfWeek.AddDays(6);
        //    CurrentWeekText.Text = string.Format("{0:dd MMMM} - {1:dd MMMM}", _startOfWeek, endOfWeek);
        //}

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
            if (_isRefreshingTasks) return;
            _isRefreshingTasks = true;
            Properties.Settings.Default.SortOption = SortComboBox.SelectedIndex;
            Properties.Settings.Default.Save();
            RefreshTasks();
            _isRefreshingTasks = false;
        }

        private void ExecutorComboBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (_isRefreshingTasks) return;
            _isRefreshingTasks = true;
            Properties.Settings.Default.ExecutorFilter = ExecutorComboBox.SelectedIndex;
            Properties.Settings.Default.Save();
            RefreshTasks();
            _isRefreshingTasks = false;
        }

        private void PriorityComboBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (_isRefreshingTasks) return;
            _isRefreshingTasks = true;
            Properties.Settings.Default.PriorityFilter = PriorityComboBox.SelectedIndex;
            Properties.Settings.Default.Save();
            RefreshTasks();
            _isRefreshingTasks = false;
        }

        private void LabelComboBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (_isRefreshingTasks) return;
            _isRefreshingTasks = true;
            Properties.Settings.Default.LabelFilter = LabelComboBox.SelectedIndex;
            Properties.Settings.Default.Save();
            RefreshTasks();
            _isRefreshingTasks = false;
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
            // Проверка на null для TasksListView
            if (TasksListView == null)
            {
                // Можно добавить логирование или вывод сообщения об ошибке
                Console.WriteLine("TasksListView is null!");
                return;
            }

            var expanded = new List<string>();

            // Перебираем все элементы в TasksListView, чтобы сохранить состояние экспандеров
            foreach (var item in TasksListView.Items)
            {
                if (item is Expander expander && expander.IsExpanded)
                {
                    expanded.Add(expander.Header.ToString());
                }
            }

            // Сохраняем заголовки развернутых групп в настройки
            Properties.Settings.Default.ExpandersState = string.Join(";", expanded);
            Properties.Settings.Default.Save();
        }

        private bool IsGroupExpanded(string header)
        {
            if (header == null) return false;

            string saved = Properties.Settings.Default.ExpandersState;
            if (string.IsNullOrEmpty(saved)) return false;

            var list = saved.Split(';');
            return list.Contains(header);
        }

        private void RestoreExpandersState()
        {
            string savedState = Properties.Settings.Default.ExpandersState;
            if (!string.IsNullOrEmpty(savedState))
            {
                var expandedHeaders = savedState.Split(';');

                // Отдельная обработка для OverdueTasksExpander
                if (OverdueTasksExpander != null)
                {
                    OverdueTasksExpander.IsExpanded = expandedHeaders.Contains("Просрочено");
                }

                foreach (var item in TasksListView.Items)
                {
                    if (item is Expander expander && expander.Header != null)
                    {
                        expander.IsExpanded = expandedHeaders.Contains(expander.Header.ToString());
                    }
                }
            }
        }
    }
}