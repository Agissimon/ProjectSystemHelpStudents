using ProjectSystemHelpStudents.Helper;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Shapes;

namespace ProjectSystemHelpStudents.Views
{
    public class TaskBoardView
    {
        /// <summary>
        /// Создаёт всю панель доски вместе с навигацией.
        /// </summary>
        public static DockPanel CreateBoardView(IEnumerable<TaskViewModel> tasks)
        {
            // Получаем смещение недель из настроек
            int weekOffset = Properties.Settings.Default.BoardWeekOffset;
            // Находим текущий понедельник с учётом смещения
            var refDate = DateTime.Today.AddDays(weekOffset * 7);
            int mondayDelta = ((int)refDate.DayOfWeek + 6) % 7;
            DateTime monday = refDate.AddDays(-mondayDelta);

            var boardGrid = new Grid { Tag = tasks };
            RefreshBoard(boardGrid, tasks, monday);

            var navBar = new StackPanel
            {
                Orientation = Orientation.Horizontal,
                HorizontalAlignment = HorizontalAlignment.Right,
                Margin = new Thickness(0, 5, 5, 5)
            };
            var btnStyle = (Style)Application.Current.FindResource("TransparentButtonStyle");

            var prev = new Button { Content = "⟨", Style = btnStyle, Margin = new Thickness(2) };
            var btnToday = new Button { Content = "Сегодня", Style = btnStyle, Margin = new Thickness(2) };
            var next = new Button { Content = "⟩", Style = btnStyle, Margin = new Thickness(2) };

            prev.Click += (s, e) => ChangeWeekOffset(-1, boardGrid, tasks);
            next.Click += (s, e) => ChangeWeekOffset(+1, boardGrid, tasks);
            btnToday.Click += (s, e) =>
            {
                Properties.Settings.Default.BoardWeekOffset = 0;
                Properties.Settings.Default.Save();

                var today = DateTime.Today;
                int mondayDeltaToday = ((int)today.DayOfWeek + 6) % 7;
                DateTime mondayToday = today.AddDays(-mondayDeltaToday);

                RefreshBoard(boardGrid, tasks, mondayToday);
            };

            navBar.Children.Add(prev);
            navBar.Children.Add(btnToday);
            navBar.Children.Add(next);

            var root = new DockPanel();
            DockPanel.SetDock(navBar, Dock.Top);
            root.Children.Add(navBar);
            root.Children.Add(boardGrid);
            return root;
        }

        private static void ChangeWeekOffset(int delta, Grid grid, IEnumerable<TaskViewModel> tasks)
        {
            int off = Properties.Settings.Default.BoardWeekOffset + delta;
            Properties.Settings.Default.BoardWeekOffset = off;
            Properties.Settings.Default.Save();

            var refDate = DateTime.Today.AddDays(off * 7);
            int mondayDelta = ((int)refDate.DayOfWeek + 6) % 7;
            DateTime monday = refDate.AddDays(-mondayDelta);
            RefreshBoard(grid, tasks, monday);
        }

        private static void RefreshBoard(Grid grid, IEnumerable<TaskViewModel> tasks, DateTime monday)
        {
            grid.Children.Clear();
            grid.ColumnDefinitions.Clear();

            DateTime today = DateTime.Today;
            bool isCurrentWeek = Properties.Settings.Default.BoardWeekOffset == 0;
            bool overdueExpanded = Properties.Settings.Default.OverdueExpanded;

            // Собираем список 
            var columns = new List<(int index, DateTime? date, IEnumerable<TaskViewModel> tasks)>();

            columns.Add((0, null, tasks.Where(t => t.EndDate.Date < today)));

            // Затем — 7 дней недели
            for (int i = 1; i <= 7; i++)
            {
                var dt = monday.AddDays(i - 1);

                if (isCurrentWeek && dt < today)
                    continue;

                var dayTasks = tasks.Where(t => t.EndDate.Date == dt);
                columns.Add((i, dt, dayTasks));
            }

            foreach (var _ in columns)
                grid.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(1, GridUnitType.Star) });

            for (int colIdx = 0; colIdx < columns.Count; colIdx++)
            {
                var (i, columnDate, columnTasks) = columns[colIdx];
                var panel = new StackPanel { Margin = new Thickness(3) };

                if (i == 0)
                {
                    var exp = new Expander
                    {
                        Header = $"Просрочено ({columnTasks.Count()})",
                        IsExpanded = overdueExpanded,
                        FontSize = 16,
                        FontWeight = FontWeights.Bold,
                        Foreground = Brushes.White
                    };
                    exp.Expanded += (s, e) => SaveOverdueState(true);
                    exp.Collapsed += (s, e) => SaveOverdueState(false);

                    var inner = new StackPanel();
                    foreach (var t in columnTasks.OrderBy(t => t.EndDate))
                        inner.Children.Add(CreateTaskCard(t, grid, tasks, monday));

                    exp.Content = new ScrollViewer
                    {
                        Content = inner,
                        VerticalScrollBarVisibility = ScrollBarVisibility.Auto,
                        MaxHeight = 400,
                        Style = (Style)Application.Current.FindResource("MinimalDarkScrollViewer")
                    };
                    panel.Children.Add(exp);
                }
                else
                {
                    DateTime dt = columnDate.Value;
                    string label = GetRelativeLabel(dt, today);
                    panel.Children.Add(new TextBlock
                    {
                        Text = $"{dt:dd MMMM} ‧ {label}",
                        FontSize = 16,
                        FontWeight = dt == today ? FontWeights.Bold : FontWeights.Normal,
                        Foreground = dt == today ? Brushes.Red : Brushes.White,
                        HorizontalAlignment = HorizontalAlignment.Center,
                        Margin = new Thickness(0, 5, 0, 5)
                    });

                    var dayInner = new StackPanel();
                    foreach (var t in columnTasks.OrderBy(t => t.EndDate))
                        dayInner.Children.Add(CreateTaskCard(t, grid, tasks, monday));

                    panel.Children.Add(new ScrollViewer
                    {
                        Content = dayInner,
                        VerticalScrollBarVisibility = ScrollBarVisibility.Auto
                    });
                }

                int idx = i;
                var btn = new Button
                {
                    Content = "+ Добавить задачу",
                    Margin = new Thickness(3),
                    HorizontalAlignment = HorizontalAlignment.Center,
                    Style = (Style)Application.Current.FindResource("AddTaskButtonStyle"),
                };
                btn.Click += (s, e) =>
                {
                    var w = new AddTaskWindow();
                    DateTime pre = (idx == 0) ? today.AddDays(-1) : monday.AddDays(idx - 1);
                    w.SetPreselectedDate(pre);
                    if (w.ShowDialog() == true)
                        RefreshBoard(grid, tasks, monday);
                };
                panel.Children.Add(btn);

                var sv = new ScrollViewer
                {
                    Content = panel,
                    VerticalScrollBarVisibility = ScrollBarVisibility.Auto
                };
                Grid.SetColumn(sv, colIdx);
                grid.Children.Add(sv);
            }
        }

        private static void SaveOverdueState(bool expanded)
        {
            Properties.Settings.Default.OverdueExpanded = expanded;
            Properties.Settings.Default.Save();
        }

        private static string GetRelativeLabel(DateTime date, DateTime today)
        {
            if (date == today) return "Сегодня";
            if (date == today.AddDays(1)) return "Завтра";
            if (date == today.AddDays(-1)) return "Вчера";
            return CultureInfo.GetCultureInfo("ru-RU").DateTimeFormat.GetDayName(date.DayOfWeek);
        }

        private static Border CreateTaskCard(
            TaskViewModel t,
            Grid boardGrid,
            IEnumerable<TaskViewModel> tasks,
            DateTime monday)
        {
            var check = new CheckBox
            {
                IsChecked = t.IsCompleted,
                VerticalAlignment = VerticalAlignment.Top,
                Margin = new Thickness(0, 0, 6, 0),
                BorderThickness = new Thickness(2)
            };
            check.Checked += (s, e) => OnTaskToggled(t, boardGrid, tasks, monday);
            check.Unchecked += (s, e) => OnTaskToggled(t, boardGrid, tasks, monday);

            var info = new StackPanel { Orientation = Orientation.Vertical };
            info.Children.Add(new TextBlock
            {
                Text = t.Title,
                FontWeight = FontWeights.Bold,
                Foreground = Brushes.White,
                FontSize = 14
            });
            if (!string.IsNullOrWhiteSpace(t.Description))
                info.Children.Add(new TextBlock
                {
                    Text = t.Description,
                    Foreground = Brushes.LightGray,
                    FontSize = 12,
                    TextWrapping = TextWrapping.Wrap,
                    Margin = new Thickness(0, 2, 0, 0)
                });
            info.Children.Add(new TextBlock
            {
                Text = t.EndDate.ToString("dd MMM yyyy HH:mm", CultureInfo.GetCultureInfo("ru-RU")),
                Foreground = Brushes.LightBlue,
                FontSize = 11,
                Margin = new Thickness(0, 2, 0, 0)
            });
            if (t.AvailableLabels?.Any() == true)
            {
                var lbls = string.Join(", ", t.AvailableLabels.Select(l => l.Name));
                info.Children.Add(new TextBlock
                {
                    Text = $"Метки: {lbls}",
                    Foreground = Brushes.LightGreen,
                    FontSize = 11,
                    Margin = new Thickness(0, 2, 0, 0)
                });
            }

            var panel = new StackPanel { Orientation = Orientation.Horizontal };
            panel.Children.Add(check);
            panel.Children.Add(info);

            var border = new Border
            {
                CornerRadius = new CornerRadius(6),
                Background = new SolidColorBrush(Color.FromRgb(50, 50, 50)),
                Margin = new Thickness(0, 4, 0, 0),
                Padding = new Thickness(6),
                Child = panel,
                Cursor = Cursors.Hand
            };

            border.MouseLeftButtonUp += (s, e) =>
            {
                if (e.OriginalSource is CheckBox) return;
                var wnd = new TaskDetailsWindow(t);
                if (wnd.ShowDialog() == true)
                    RefreshBoard(boardGrid, tasks, monday);
            };

            return border;
        }

        private static void OnTaskToggled(
            TaskViewModel t,
            Grid boardGrid,
            IEnumerable<TaskViewModel> tasks,
            DateTime monday)
        {
            var db = DBClass.entities.Task.First(x => x.IdTask == t.IdTask);
            var newStatusName = t.IsCompleted ? "Завершено" : "Не завершено";
            db.StatusId = DBClass.entities.Status.First(st => st.Name == newStatusName).StatusId;
            DBClass.entities.SaveChanges();

            RefreshBoard(boardGrid, tasks, monday);
        }
    }
}
