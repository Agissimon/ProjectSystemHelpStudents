using ProjectSystemHelpStudents.Helper;
using ProjectSystemHelpStudents.ViewModels;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Data;
using System.Windows.Input;
using System.Windows.Media;

namespace ProjectSystemHelpStudents.Views
{
    public class TaskBoardView
    {
        private static DateTime GetCurrentMonday()
        {
            int offset = Properties.Settings.Default.BoardWeekOffset;
            DateTime baseDate = DateTime.Today.AddDays(offset * 7);
            int d2m = ((int)baseDate.DayOfWeek + 6) % 7;
            return baseDate.AddDays(-d2m);
        }

        public static DockPanel CreateBoardView(IEnumerable<TaskViewModel> tasks)
        {
            var grid = new Grid { Tag = tasks };
            RefreshBoard(grid, tasks, GetCurrentMonday());

            // Навигация
            var nav = new StackPanel
            {
                Orientation = Orientation.Horizontal,
                HorizontalAlignment = HorizontalAlignment.Right,
                Margin = new Thickness(0, 5, 5, 5)
            };
            var style = (Style)Application.Current.FindResource("TransparentButtonStyle");
            var btnPrev = new Button { Content = "⟨", Style = style, Margin = new Thickness(2) };
            var btnToday = new Button { Content = "Сегодня", Style = style, Margin = new Thickness(2) };
            var btnNext = new Button { Content = "⟩", Style = style, Margin = new Thickness(2) };

            btnPrev.Click += (s, e) => ChangeWeekOffset(-1, grid, tasks);
            btnNext.Click += (s, e) => ChangeWeekOffset(+1, grid, tasks);
            btnToday.Click += (s, e) =>
            {
                Properties.Settings.Default.BoardWeekOffset = 0;
                Properties.Settings.Default.Save();
                RefreshBoard(grid, tasks, GetCurrentMonday());
            };

            nav.Children.Add(btnPrev);
            nav.Children.Add(btnToday);
            nav.Children.Add(btnNext);

            var root = new DockPanel();
            DockPanel.SetDock(nav, Dock.Top);
            root.Children.Add(nav);
            root.Children.Add(grid);
            return root;
        }

        private static void ChangeWeekOffset(int delta, Grid grid, IEnumerable<TaskViewModel> tasks)
        {
            Properties.Settings.Default.BoardWeekOffset += delta;
            Properties.Settings.Default.Save();
            RefreshBoard(grid, tasks, GetCurrentMonday());
        }

        private static void RefreshBoard(Grid grid, IEnumerable<TaskViewModel> tasks, DateTime monday)
        {
            grid.Children.Clear();
            grid.ColumnDefinitions.Clear();

            DateTime today = DateTime.Today;
            bool isCurrent = Properties.Settings.Default.BoardWeekOffset == 0;
            bool overdueExpanded = Properties.Settings.Default.OverdueExpanded;

            var columns = new List<(int idx, DateTime? date, IEnumerable<TaskViewModel> items)>();
            columns.Add((0, null, tasks.Where(t => t.EndDate < today)));
            for (int i = 0; i < 7; i++)
            {
                DateTime dt = monday.AddDays(i);
                if (isCurrent && dt < today) continue;
                columns.Add((i + 1, dt, tasks.Where(t => t.EndDate == dt)));
            }
            foreach (var _ in columns)
                grid.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(1, GridUnitType.Star) });

            for (int colIdx = 0; colIdx < columns.Count; colIdx++)
            {
                var (idx, date, items) = columns[colIdx];
                var panel = new StackPanel { Margin = new Thickness(3) };
                if (idx == 0)
                {
                    var exp = new Expander
                    {
                        Header = $"Просрочено ({items.Count()})",
                        IsExpanded = overdueExpanded,
                        FontSize = 16,
                        FontWeight = FontWeights.Bold,
                        Foreground = Brushes.White
                    };
                    exp.Expanded += (_, __) => SaveOverdueState(true);
                    exp.Collapsed += (_, __) => SaveOverdueState(false);
                    var inner = new StackPanel();
                    foreach (var t in items.OrderBy(t => t.EndDate))
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
                    DateTime dt = date.Value;
                    panel.Children.Add(new TextBlock
                    {
                        Text = $"{dt:dd MMMM} ‧ {GetRelativeLabel(dt, today)}",
                        FontSize = 16,
                        FontWeight = dt == today ? FontWeights.Bold : FontWeights.Normal,
                        Foreground = dt == today ? Brushes.Red : Brushes.White,
                        HorizontalAlignment = HorizontalAlignment.Center,
                        Margin = new Thickness(0, 5, 0, 5)
                    });
                    var inner = new StackPanel();
                    foreach (var t in items.OrderBy(t => t.EndDate))
                        inner.Children.Add(CreateTaskCard(t, grid, tasks, monday));
                    panel.Children.Add(new ScrollViewer
                    {
                        Content = inner,
                        VerticalScrollBarVisibility = ScrollBarVisibility.Auto
                    });
                }
                var btn = new Button
                {
                    Content = "+ Добавить задачу",
                    Margin = new Thickness(3),
                    HorizontalAlignment = HorizontalAlignment.Center,
                    Style = (Style)Application.Current.FindResource("AddTaskButtonStyle")
                };
                btn.Click += (s, e) =>
                {
                    var w = new AddTaskWindow();
                    DateTime pre = (idx == 0 ? DateTime.Today.AddDays(-1) : monday.AddDays(idx - 1));
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

        private static void SaveOverdueState(bool exp)
        {
            Properties.Settings.Default.OverdueExpanded = exp;
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
            Grid grid,
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
            check.Checked += (s, e) => t.IsCompleted = true;
            check.Unchecked += (s, e) => t.IsCompleted = false;

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

            var labelsPanel = new WrapPanel { Orientation = Orientation.Horizontal, Margin = new Thickness(0, 2, 0, 0) };
            void RebuildLabels()
            {
                labelsPanel.Children.Clear();
                foreach (var lbl in t.AvailableLabels.Where(l => l.IsSelected))
                {
                    var tb = new TextBlock
                    {
                        Text = lbl.Name,
                        FontSize = 11,
                        Margin = new Thickness(0, 0, 6, 0),
                        VerticalAlignment = VerticalAlignment.Center
                    };
                    tb.SetBinding(TextBlock.ForegroundProperty,
                        new Binding(nameof(LabelViewModel.BackgroundBrush)) { Source = lbl, Mode = BindingMode.OneWay });
                    labelsPanel.Children.Add(tb);
                }
            }
            RebuildLabels();
            info.Children.Add(labelsPanel);

            t.PropertyChanged += (s, e) =>
            {
                if (e.PropertyName == nameof(TaskViewModel.LabelsFormatted))
                    RebuildLabels();
            };

            var row = new StackPanel { Orientation = Orientation.Horizontal };
            var marker = new Border { Width = 4, Margin = new Thickness(0, 4, 6, 0), VerticalAlignment = VerticalAlignment.Stretch };
            marker.SetBinding(Border.BackgroundProperty,
                new Binding(nameof(TaskViewModel.MarkerBrush)) { Source = t });
            row.Children.Add(marker);
            row.Children.Add(check);
            row.Children.Add(info);

            var card = new Border
            {
                CornerRadius = new CornerRadius(6),
                Background = new SolidColorBrush(Color.FromRgb(50, 50, 50)),
                Margin = new Thickness(0, 4, 0, 0),
                Padding = new Thickness(6),
                Child = row,
                Cursor = Cursors.Hand
            };
            card.MouseLeftButtonUp += (s, e) =>
            {
                if (e.OriginalSource is CheckBox) return;
                var w = new TaskDetailsWindow(t);
                if (w.ShowDialog() == true)
                    RefreshBoard(grid, tasks, monday);
            };
            return card;
        }
    }
}