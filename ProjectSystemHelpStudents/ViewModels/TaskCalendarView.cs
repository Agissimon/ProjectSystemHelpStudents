using ProjectSystemHelpStudents.Helper;
using ProjectSystemHelpStudents.ViewModels;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Windows.Controls;
using System.Windows;
using System.Windows.Input;
using System.Windows.Media;

namespace ProjectSystemHelpStudents.Views
{
    public class TaskCalendarView
    {
        public static DockPanel CreateCalendarView(IEnumerable<TaskViewModel> tasks)
        {
            int offset = Properties.Settings.Default.CalendarMonthOffset;
            DateTime baseMonth = DateTime.Today.AddMonths(offset);

            var calendarGrid = new Grid { Tag = tasks, Margin = new Thickness(5) };
            SetupGridStructure(calendarGrid);
            RefreshCalendar(calendarGrid, tasks, baseMonth);

            var nav = new StackPanel
            {
                Orientation = Orientation.Horizontal,
                HorizontalAlignment = HorizontalAlignment.Center,
                Margin = new Thickness(0, 0, 0, 5)
            };
            var style = (Style)Application.Current.FindResource("TransparentButtonStyle");

            var prev = new Button { Content = "⟨", Style = style, Margin = new Thickness(2) };
            var todayBtn = new Button { Content = "Сегодня", Style = style, Margin = new Thickness(2) };
            var next = new Button { Content = "⟩", Style = style, Margin = new Thickness(2) };

            var monthLabel = new TextBlock
            {
                Text = baseMonth.ToString("MMMM yyyy", CultureInfo.GetCultureInfo("ru-RU")),
                FontSize = 16,
                FontWeight = FontWeights.Bold,
                Foreground = Brushes.White,
                Margin = new Thickness(2)
            };

            prev.Click += (s, e) => ChangeMonthOffset(-1, calendarGrid, tasks, monthLabel);
            next.Click += (s, e) => ChangeMonthOffset(+1, calendarGrid, tasks, monthLabel);
            todayBtn.Click += (s, e) =>
            {
                Properties.Settings.Default.CalendarMonthOffset = 0;
                Properties.Settings.Default.Save();
                DateTime today = DateTime.Today;
                RefreshCalendar(calendarGrid, tasks, today);
                monthLabel.Text = today.ToString("MMMM yyyy", CultureInfo.GetCultureInfo("ru-RU"));
            };

            nav.Children.Add(prev);
            nav.Children.Add(monthLabel);
            nav.Children.Add(todayBtn);
            nav.Children.Add(next);

            var root = new DockPanel();
            DockPanel.SetDock(nav, Dock.Top);
            root.Children.Add(nav);
            root.Children.Add(calendarGrid);
            return root;
        }

        private static void ChangeMonthOffset(int delta, Grid grid, IEnumerable<TaskViewModel> tasks, TextBlock label)
        {
            int off = Properties.Settings.Default.CalendarMonthOffset + delta;
            Properties.Settings.Default.CalendarMonthOffset = off;
            Properties.Settings.Default.Save();

            DateTime m = DateTime.Today.AddMonths(off);
            RefreshCalendar(grid, tasks, m);
            label.Text = m.ToString("MMMM yyyy", CultureInfo.GetCultureInfo("ru-RU"));
        }

        private static void SetupGridStructure(Grid grid)
        {
            grid.ColumnDefinitions.Clear();
            grid.RowDefinitions.Clear();
            for (int c = 0; c < 7; c++)
                grid.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(1, GridUnitType.Star) });
            for (int r = 0; r < 6; r++)
                grid.RowDefinitions.Add(new RowDefinition { Height = new GridLength(1, GridUnitType.Star) });
        }

        private static void RefreshCalendar(Grid grid, IEnumerable<TaskViewModel> tasks, DateTime baseMonth)
        {
            grid.Children.Clear();
            var today = DateTime.Today;
            var first = new DateTime(baseMonth.Year, baseMonth.Month, 1);
            int offset = ((int)first.DayOfWeek + 6) % 7;
            int days = DateTime.DaysInMonth(baseMonth.Year, baseMonth.Month);

            for (int d = 0; d < days; d++)
            {
                DateTime date = first.AddDays(d);
                int idx = offset + d;
                int row = idx / 7, col = idx % 7;

                var cell = CreateDayCell(date, tasks, grid);
                Grid.SetRow(cell, row);
                Grid.SetColumn(cell, col);
                grid.Children.Add(cell);
            }
        }

        private static Border CreateDayCell(DateTime date, IEnumerable<TaskViewModel> tasks, Grid grid)
        {
            var cell = new Border
            {
                BorderBrush = Brushes.Gray,
                BorderThickness = new Thickness(1),
                Background = new SolidColorBrush(Color.FromRgb(30, 30, 30)),
                Margin = new Thickness(1),
                Tag = date
            };

            var panel = new StackPanel { Margin = new Thickness(4) };

            panel.Children.Add(new TextBlock
            {
                Text = date.Day.ToString(),
                HorizontalAlignment = HorizontalAlignment.Right,
                Foreground = date == DateTime.Today ? Brushes.Red : Brushes.White,
                FontWeight = date == DateTime.Today ? FontWeights.Bold : FontWeights.Normal
            });

            foreach (var t in tasks.Where(t => !t.IsCompleted && t.EndDate.Date == date))
                panel.Children.Add(CreateTaskCard(t, grid, tasks));

            var scroll = new ScrollViewer
            {
                Content = panel,
                VerticalScrollBarVisibility = ScrollBarVisibility.Auto
            };
            cell.Child = scroll;

            cell.MouseLeftButtonUp += (s, e) =>
            {
                var origin = e.OriginalSource;
                if (origin is Border || origin is ScrollViewer ||
                    (origin is TextBlock tb && tb.Text == date.Day.ToString()))
                {
                    var win = new AddTaskWindow(date);
                    if (win.ShowDialog() == true)
                    {
                        int off = Properties.Settings.Default.CalendarMonthOffset;
                        DateTime bm = DateTime.Today.AddMonths(off);
                        RefreshCalendar(grid, tasks, bm);
                    }
                }
            };

            return cell;
        }

        private static UIElement CreateTaskCard(TaskViewModel t, Grid grid, IEnumerable<TaskViewModel> tasks)
        {
            var cb = new CheckBox
            {
                Margin = new Thickness(0, 2, 6, 0),
                VerticalAlignment = VerticalAlignment.Center,
                BorderThickness = new Thickness(2)
            };
            cb.Checked += (s, e) =>
            {
                var dbt = DBClass.entities.Task.Find(t.IdTask);
                var done = DBClass.entities.Status.First(st => st.Name == "Завершено");
                dbt.StatusId = done.StatusId;
                DBClass.entities.SaveChanges();
                int off = Properties.Settings.Default.CalendarMonthOffset;
                DateTime bm = DateTime.Today.AddMonths(off);
                RefreshCalendar(grid, tasks, bm);
            };

            var title = new TextBlock
            {
                Text = t.Title,
                Foreground = Brushes.White,
                FontSize = 14,
                VerticalAlignment = VerticalAlignment.Center,
                TextWrapping = TextWrapping.Wrap
            };

            // маркер
            var marker = new Border
            {
                Width = 4,
                Background = TaskMarkerHelper.GetMarkerBrush(t),
                Margin = new Thickness(0, 4, 6, 0),
                VerticalAlignment = VerticalAlignment.Stretch
            };

            var stack = new StackPanel { Orientation = Orientation.Horizontal };
            stack.Children.Add(marker);
            stack.Children.Add(cb);
            stack.Children.Add(title);

            var border = new Border
            {
                CornerRadius = new CornerRadius(6),
                Background = new SolidColorBrush(Color.FromRgb(50, 50, 50)),
                Margin = new Thickness(0, 4, 0, 0),
                Padding = new Thickness(6),
                Child = stack,
                Cursor = Cursors.Hand
            };

            border.MouseLeftButtonUp += (s, e) =>
            {
                if (e.OriginalSource is CheckBox) return;
                var wnd = new TaskDetailsWindow(t);
                wnd.ShowDialog();
            };

            return border;
        }
    }
}
