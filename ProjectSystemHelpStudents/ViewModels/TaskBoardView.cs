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
        // Создает и возвращает Grid-доску, заполненную задачами.
        public static Grid CreateBoardView(IEnumerable<TaskViewModel> tasks)
        {
            var boardGrid = new Grid { Tag = tasks };
            RefreshBoard(boardGrid, tasks);
            return boardGrid;
        }

        // Перестраивает доску, заполняя её данными из tasks.
        private static void RefreshBoard(Grid boardGrid, IEnumerable<TaskViewModel> tasks)
        {
            boardGrid.Children.Clear();
            boardGrid.ColumnDefinitions.Clear();

            DateTime today = DateTime.Today;

            for (int i = 0; i < 8; i++)
            {
                boardGrid.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(250) });
            }

            for (int i = 0; i < 8; i++)
            {
                string headerText;
                IEnumerable<TaskViewModel> columnTasks;
                DateTime columnDate = today.AddDays(i - 1);

                if (i == 0)
                {
                    headerText = "Просрочено";
                    columnTasks = tasks.Where(t => t.EndDate < today);
                }
                else
                {
                    string label = GetRelativeLabel(columnDate, today);
                    headerText = $"{columnDate:dd MMMM} ‧ {label}";
                    columnTasks = tasks.Where(t => t.EndDate.Date == columnDate.Date);
                }

                var columnStack = new StackPanel();
                columnStack.Children.Add(new TextBlock
                {
                    Text = headerText,
                    FontWeight = FontWeights.Bold,
                    FontSize = 18,
                    Margin = new Thickness(5),
                    HorizontalAlignment = HorizontalAlignment.Center,
                    Foreground = Brushes.White
                });

                // Создание Expander для колонки
                var expander = new Expander
                {
                    Header = headerText,  // Заголовок для каждой группы задач
                    IsExpanded = ExpanderStateManager.GetState(headerText), // Восстановление состояния
                    Margin = new Thickness(0, 0, 0, 10)
                };

                // Добавление задач в Expander
                foreach (var task in columnTasks.OrderBy(t => t.EndDate))
                {
                    expander.Content = CreateTaskCard(task, boardGrid); // Добавляем задачи внутрь Expander
                }

                columnStack.Children.Add(expander); // Добавляем Expander в колонку


                // Добавляем карточки задач для данной колонки
                foreach (var task in columnTasks.OrderBy(t => t.EndDate))
                {
                    columnStack.Children.Add(CreateTaskCard(task, boardGrid));
                }

                // Кнопка добавления задачи
                var addButton = new Button
                {
                    Content = "+ Добавить задачу",
                    Margin = new Thickness(5),
                    Style = Application.Current.FindResource("AddTaskButtonStyle") as Style
                };

                addButton.Click += (s, e) =>
                {
                    var window = new AddTaskWindow();
                    window.SetPreselectedDate(i == 0 ? today.AddDays(-1) : columnDate);
                    if (window.ShowDialog() == true)
                    {
                        // Получаем только задачи, которые не завершены
                        var updatedTasks = DBClass.entities.Task
                            .Where(t => t.Status.Name != "Завершено")
                            .ToList()
                            .Select(t => new TaskViewModel
                            {
                                IdTask = t.IdTask,
                                Title = t.Title,
                                Description = t.Description,
                                EndDate = t.EndDate,
                                Status = t.Status?.Name,
                                IsCompleted = t.Status?.Name == "Завершено",
                                AvailableLabels = new System.Collections.ObjectModel.ObservableCollection<LabelViewModel>(
                                                      t.TaskLabels.Select(l => new LabelViewModel { Name = l.Labels.Name })),
                                EndDateFormatted = t.EndDate.ToString("dd MMMM yyyy", CultureInfo.GetCultureInfo("ru-RU"))
                            })
                            .ToList();

                        boardGrid.Tag = updatedTasks;
                        RefreshBoard(boardGrid, updatedTasks);
                    }
                };

                columnStack.Children.Add(addButton);

                var scrollViewer = new ScrollViewer
                {
                    VerticalScrollBarVisibility = ScrollBarVisibility.Auto,
                    HorizontalScrollBarVisibility = ScrollBarVisibility.Disabled,
                    Content = columnStack
                };

                Grid.SetColumn(scrollViewer, i);
                boardGrid.Children.Add(scrollViewer);
            }
        }

        private static string GetRelativeLabel(DateTime date, DateTime today)
        {
            if (date == today)
                return "Сегодня";
            if (date == today.AddDays(1))
                return "Завтра";
            if (date == today.AddDays(-1))
                return "Вчера";

            return CultureInfo.GetCultureInfo("ru-RU").DateTimeFormat.GetDayName(date.DayOfWeek);
        }

        private static Border CreateTaskCard(TaskViewModel task, Grid boardGrid)
        {
            var circle = new Ellipse
            {
                Width = 12,
                Height = 12,
                Stroke = Brushes.Gray,
                StrokeThickness = 2,
                Fill = task.Status == "Завершено" ? Brushes.Green : Brushes.Transparent,
                Margin = new Thickness(0, 0, 8, 0),
                VerticalAlignment = VerticalAlignment.Center,
                Cursor = Cursors.Hand
            };

            var titleText = new TextBlock
            {
                Text = task.Title,
                FontWeight = FontWeights.Bold,
                Foreground = Brushes.White,
                FontSize = 14
            };

            var dateText = new TextBlock
            {
                Text = task.EndDate.ToString("dd MMMM yyyy", CultureInfo.GetCultureInfo("ru-RU")),
                Foreground = Brushes.LightGray,
                FontSize = 12
            };

            var taskInfo = new StackPanel();
            taskInfo.Children.Add(titleText);
            taskInfo.Children.Add(dateText);

            var taskPanel = new StackPanel { Orientation = Orientation.Horizontal };
            taskPanel.Children.Add(circle);
            taskPanel.Children.Add(taskInfo);

            var border = new Border
            {
                Margin = new Thickness(5),
                Background = Brushes.DimGray,
                CornerRadius = new CornerRadius(6),
                Padding = new Thickness(8),
                Cursor = Cursors.Hand,
                Child = taskPanel
            };

            border.MouseEnter += (s, e) => border.Background = new SolidColorBrush(Color.FromRgb(80, 80, 80));
            border.MouseLeave += (s, e) => border.Background = Brushes.DimGray;

            // Обработчик клика по кружочку для смены статуса задачи
            circle.MouseLeftButtonUp += (s, e) =>
            {
                var dbTask = DBClass.entities.Task.FirstOrDefault(t => t.IdTask == task.IdTask);
                if (dbTask != null)
                {
                    var newStatusName = dbTask.Status.Name == "Завершено" ? "Не начато" : "Завершено";
                    var newStatus = DBClass.entities.Status.FirstOrDefault(st => st.Name == newStatusName);
                    if (newStatus != null)
                    {
                        dbTask.StatusId = newStatus.StatusId;
                        DBClass.entities.SaveChanges();

                        var updatedTasks = DBClass.entities.Task
                            .Where(t => t.Status.Name != "Завершено")
                            .ToList()
                            .Select(t => new TaskViewModel
                            {
                                IdTask = t.IdTask,
                                Title = t.Title,
                                Description = t.Description,
                                EndDate = t.EndDate,
                                Status = t.Status?.Name,
                                IsCompleted = t.Status?.Name == "Завершено",
                                AvailableLabels = new System.Collections.ObjectModel.ObservableCollection<LabelViewModel>(
                                                        t.TaskLabels.Select(l => new LabelViewModel { Name = l.Labels.Name })),
                                EndDateFormatted = t.EndDate.ToString("dd MMMM yyyy", CultureInfo.GetCultureInfo("ru-RU"))
                            })
                            .ToList();

                        boardGrid.Tag = updatedTasks;
                        RefreshBoard(boardGrid, updatedTasks);
                    }
                }
                e.Handled = true;
            };

            // Обработчик клика по задаче (открытие окна с деталями)
            border.MouseLeftButtonUp += (s, e) =>
            {
                if (e.OriginalSource is Ellipse)
                    return;

                var detailsWindow = new TaskDetailsWindow(task);
                if (detailsWindow.ShowDialog() == true)
                {
                    var updatedTasks = DBClass.entities.Task
                        .Where(t => t.Status.Name != "Завершено")
                        .ToList()
                        .Select(t => new TaskViewModel
                        {
                            IdTask = t.IdTask,
                            Title = t.Title,
                            Description = t.Description,
                            EndDate = t.EndDate,
                            Status = t.Status?.Name,
                            IsCompleted = t.Status?.Name == "Завершено",
                            AvailableLabels = new System.Collections.ObjectModel.ObservableCollection<LabelViewModel>(
                                                        t.TaskLabels.Select(l => new LabelViewModel { Name = l.Labels.Name })),
                            EndDateFormatted = t.EndDate.ToString("dd MMMM yyyy", CultureInfo.GetCultureInfo("ru-RU"))
                        })
                        .ToList();

                    boardGrid.Tag = updatedTasks;
                    RefreshBoard(boardGrid, updatedTasks);
                }
                e.Handled = true;
            };

            return border;
        }
    }
}
