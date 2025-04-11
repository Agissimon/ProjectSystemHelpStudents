using ProjectSystemHelpStudents.Helper;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows;
using System.Windows.Controls;

namespace YourApp.Views
{
    public class TaskCalendarView
    {
        public static Grid CreateCalendarView(IEnumerable<TaskViewModel> tasks)
        {
            tasks = tasks.Where(t => !t.IsCompleted).ToList();

            Grid calendarGrid = new Grid
            {
                Margin = new Thickness(10)
            };

            for (int i = 0; i < 7; i++)
            {
                calendarGrid.ColumnDefinitions.Add(new ColumnDefinition());
            }

            var startOfWeek = DateTime.Today.AddDays(-(int)DateTime.Today.DayOfWeek + 1); // Понедельник
            for (int i = 0; i < 7; i++)
            {
                var day = startOfWeek.AddDays(i);
                var dayTasks = tasks.Where(t => t.EndDate.Date == day.Date);
                var columnStack = new StackPanel();
                var header = new TextBlock
                {
                    Text = $"{day:dddd, dd MMM}",
                    FontWeight = FontWeights.Bold,
                    FontSize = 16,
                    Margin = new Thickness(5),
                    HorizontalAlignment = HorizontalAlignment.Center
                };
                columnStack.Children.Add(header);

                foreach (var task in dayTasks)
                {
                    var taskBlock = new TextBlock
                    {
                        Text = task.Title,
                        Margin = new Thickness(5),
                        Background = System.Windows.Media.Brushes.DarkSlateBlue,
                        Foreground = System.Windows.Media.Brushes.White,
                        Padding = new Thickness(5)
                    };
                    columnStack.Children.Add(taskBlock);
                }

                Grid.SetColumn(columnStack, i);
                calendarGrid.Children.Add(columnStack);
            }

            return calendarGrid;
        }
    }
}
