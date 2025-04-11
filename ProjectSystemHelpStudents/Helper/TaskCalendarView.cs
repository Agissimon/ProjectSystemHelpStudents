using ProjectSystemHelpStudents;
using ProjectSystemHelpStudents.Helper;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Shapes;

namespace YourApp.Views
{
    public class TaskCalendarView
    {
        public static Grid CreateCalendarView(IEnumerable<TaskViewModel> allTasks)
        {
            Grid calendarGrid = new Grid { Margin = new Thickness(10), Tag = allTasks };

            for (int i = 0; i < 7; i++)
                calendarGrid.ColumnDefinitions.Add(new ColumnDefinition());

            // Максимум 6 строк (недель)
            for (int i = 0; i < 6; i++)
                calendarGrid.RowDefinitions.Add(new RowDefinition());

            RefreshCalendar(calendarGrid, allTasks);
            return calendarGrid;
        }

        private static void RefreshCalendar(Grid grid, IEnumerable<TaskViewModel> allTasks)
        {
            grid.Children.Clear();

            var today = DateTime.Today;
            var firstDayOfMonth = new DateTime(today.Year, today.Month, 1);
            var lastDayOfMonth = firstDayOfMonth.AddMonths(1).AddDays(-1);
            int daysInMonth = (lastDayOfMonth - firstDayOfMonth).Days + 1;
            int offset = (int)firstDayOfMonth.DayOfWeek;

            for (int dayIndex = 0; dayIndex < daysInMonth; dayIndex++)
            {
                DateTime currentDate = firstDayOfMonth.AddDays(dayIndex);
                int cellIndex = offset + dayIndex;
                int row = cellIndex / 7;
                int col = cellIndex % 7;

                var dayTasks = allTasks
                    .Where(t => !t.IsCompleted && t.EndDate.Date == currentDate.Date)
                    .ToList();

                var dayCell = new Border
                {
                    BorderBrush = new SolidColorBrush(Color.FromRgb(60, 60, 60)),
                    BorderThickness = new Thickness(1),
                    Background = new SolidColorBrush(Color.FromRgb(30, 30, 30)),
                    Margin = new Thickness(1),
                    Tag = currentDate
                };

                var dayStack = new StackPanel { Margin = new Thickness(4) };

                var dayLabel = new TextBlock
                {
                    Text = currentDate.Day.ToString(),
                    HorizontalAlignment = HorizontalAlignment.Right,
                    Foreground = currentDate == today ? Brushes.Red : Brushes.White,
                    FontWeight = currentDate == today ? FontWeights.Bold : FontWeights.Normal
                };

                dayStack.Children.Add(dayLabel);

                foreach (var task in dayTasks)
                {
                    var taskPanel = new StackPanel
                    {
                        Orientation = Orientation.Horizontal,
                        Margin = new Thickness(0, 6, 0, 0) // отступ между задачами
                    };

                    var circle = new Ellipse
                    {
                        Width = 12,
                        Height = 12,
                        Stroke = Brushes.Gray,
                        StrokeThickness = 2,
                        Fill = task.IsCompleted ? Brushes.Green : Brushes.Transparent,
                        Margin = new Thickness(0, 2, 6, 0),
                        Cursor = Cursors.Hand
                    };

                    var taskText = new TextBlock
                    {
                        Text = task.Title,
                        FontSize = 12,
                        Foreground = Brushes.White,
                        Background = new SolidColorBrush(Color.FromRgb(50, 50, 50)),
                        Padding = new Thickness(6, 3, 6, 3),
                        TextWrapping = TextWrapping.Wrap
                    };

                    var taskBorder = new Border
                    {
                        CornerRadius = new CornerRadius(6),
                        Background = new SolidColorBrush(Color.FromRgb(45, 45, 45)),
                        Margin = new Thickness(0),
                        Child = taskPanel,
                        Cursor = Cursors.Hand
                    };

                    taskPanel.Children.Add(circle);
                    taskPanel.Children.Add(taskText);

                    // Обработчик завершения задачи
                    circle.MouseLeftButtonUp += (s, e) =>
                    {
                        task.IsCompleted = true;
                        DBClass.entities.SaveChanges();
                        RefreshCalendar(grid, (IEnumerable<TaskViewModel>)grid.Tag);
                        e.Handled = true;
                    };

                    // Открытие окна с деталями задачи
                    taskBorder.MouseLeftButtonUp += (s, e) =>
                    {
                        if (e.OriginalSource is Ellipse) return;
                        new TaskDetailsWindow(task).ShowDialog();
                        RefreshCalendar(grid, (IEnumerable<TaskViewModel>)grid.Tag);
                        e.Handled = true;
                    };

                    dayStack.Children.Add(taskBorder);
                }

                dayCell.Child = dayStack;

                // Клик по пустой ячейке -> создать задачу
                dayCell.MouseLeftButtonUp += (s, e) =>
                {
                    // если клик был по задаче — не реагируем
                    if (e.OriginalSource is Ellipse || e.OriginalSource is TextBlock text && !char.IsDigit(text.Text.FirstOrDefault()))
                        return;

                    var cell = (Border)s;
                    var date = (DateTime)cell.Tag;
                    var addWindow = new AddTaskWindow();
                    if (addWindow.ShowDialog() == true)
                    {
                        // Заново загружаем задачи и обновляем
                        var updatedTasks = DBClass.entities.Task.ToList().Select(t => new TaskViewModel());
                        grid.Tag = updatedTasks;
                        RefreshCalendar(grid, updatedTasks);
                    }
                };

                Grid.SetRow(dayCell, row);
                Grid.SetColumn(dayCell, col);
                grid.Children.Add(dayCell);
            }
        }
    }
}
