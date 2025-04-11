using ProjectSystemHelpStudents;
using ProjectSystemHelpStudents.Helper;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;

namespace YourApp.Views
{
    public class TaskBoardView
    {
        public static Grid CreateBoardView(IEnumerable<TaskViewModel> tasks)
        {
            Grid boardGrid = new Grid
            {
                Margin = new Thickness(10)
            };

            string[] dateCategories = { "Просрочено", "Сегодня", "Завтра", "Следующие дни" };
            foreach (var _ in dateCategories)
                boardGrid.ColumnDefinitions.Add(new ColumnDefinition());

            DateTime today = DateTime.Today;
            DateTime tomorrow = today.AddDays(1);

            for (int i = 0; i < dateCategories.Length; i++)
            {
                string category = dateCategories[i];
                var columnStack = new StackPanel();

                var header = new TextBlock
                {
                    Text = category,
                    FontWeight = FontWeights.Bold,
                    FontSize = 18,
                    Margin = new Thickness(5),
                    HorizontalAlignment = HorizontalAlignment.Center,
                    Foreground = Brushes.White
                };

                columnStack.Children.Add(header);

                IEnumerable<TaskViewModel> categoryTasks;

                if (category == "Просрочено")
                {
                    categoryTasks = tasks.Where(t => t.EndDate < today);
                }
                else if (category == "Сегодня")
                {
                    categoryTasks = tasks.Where(t => t.EndDate == today);
                }
                else if (category == "Завтра")
                {
                    categoryTasks = tasks.Where(t => t.EndDate == tomorrow);
                }
                else
                {
                    categoryTasks = tasks.Where(t => t.EndDate > tomorrow);
                }

                foreach (var task in categoryTasks.OrderBy(t => t.EndDate))
                {
                    var taskCard = CreateTaskCard(task);
                    columnStack.Children.Add(taskCard);
                }

                Grid.SetColumn(columnStack, i);
                boardGrid.Children.Add(columnStack);
            }

            return boardGrid;
        }

        private static Border CreateTaskCard(TaskViewModel task)
        {
            var border = new Border
            {
                Margin = new Thickness(5),
                Background = Brushes.DimGray,
                CornerRadius = new CornerRadius(5),
                Padding = new Thickness(8),
                Cursor = Cursors.Hand
            };

            var stack = new StackPanel();

            var completeCheckBox = new CheckBox
            {
                Content = "Завершить",
                IsChecked = task.Status == "Завершено",
                Margin = new Thickness(0, 5, 0, 0),
                Foreground = Brushes.White
            };

            stack.Children.Add(new TextBlock
            {
                Text = task.Title,
                FontWeight = FontWeights.Bold,
                Foreground = Brushes.White,
                FontSize = 14
            });

            stack.Children.Add(new TextBlock
            {
                Text = task.EndDate.ToString("dd MMMM yyyy"),
                Foreground = Brushes.LightGray,
                FontSize = 12
            });

            completeCheckBox.Click += (s, e) =>
            {
                var dbTask = DBClass.entities.Task.FirstOrDefault(t => t.IdTask == task.IdTask);
                if (dbTask != null)
                {
                    var newStatusName = completeCheckBox.IsChecked == true ? "Завершено" : "Не начато";
                    var newStatus = DBClass.entities.Status.FirstOrDefault(st => st.Name == newStatusName);
                    if (newStatus != null)
                    {
                        dbTask.StatusId = newStatus.StatusId;
                        DBClass.entities.SaveChanges();
                    }
                }
            };

            stack.Children.Add(completeCheckBox);
            border.Child = stack;

            // Обработка клика по задаче — открытие окна деталей
            border.MouseLeftButtonUp += (s, e) =>
            {
                var detailsWindow = new TaskDetailsWindow(task);
                detailsWindow.ShowDialog();
                // При необходимости здесь можно обновить список задач после закрытия
            };

            return border;
        }
    }
}
