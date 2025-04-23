using ProjectSystemHelpStudents.Helper;
using System.Collections.Generic;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;

namespace ProjectSystemHelpStudents.ViewModels
{
    public class BoardViewControl
    {
        private int? _projectId;
        private string _pageKey;
        private bool _isProjectBoard;

        public BoardViewControl(int projectId)
        {
            _projectId = projectId;
            _isProjectBoard = true;
        }

        public BoardViewControl(string pageKey)
        {
            _pageKey = pageKey;
            _isProjectBoard = false;
        }

        public UIElement CreateBoardView(List<TaskViewModel> tasks, List<Section> sections)
        {
            var boardGrid = new Grid { Margin = new Thickness(10) };
            boardGrid.RowDefinitions.Add(new RowDefinition { Height = GridLength.Auto });

            for (int i = 0; i < sections.Count + 2; i++)
            {
                boardGrid.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(1, GridUnitType.Auto) });
            }

            var topPanel = new StackPanel
            {
                Orientation = Orientation.Horizontal,
                HorizontalAlignment = HorizontalAlignment.Right,
                Margin = new Thickness(0, 0, 10, 10)
            };

            var addTaskButton = new Button
            {
                Content = "+ Добавить задачу",
                Style = (Style)Application.Current.FindResource("AddTaskButtonStyle"),
                Margin = new Thickness(0)
            };
            addTaskButton.Click += AddTask_Click;
            topPanel.Children.Add(addTaskButton);

            var scrollViewer = new ScrollViewer
            {
                HorizontalScrollBarVisibility = ScrollBarVisibility.Auto,
                VerticalScrollBarVisibility = ScrollBarVisibility.Disabled,
                Content = boardGrid
            };

            var noSectionColumn = CreateColumn("(Без раздела)", tasks.Where(t => t.Section == null).ToList(), null, 0);
            Grid.SetRow(noSectionColumn, 0);
            Grid.SetColumn(noSectionColumn, 0);
            boardGrid.Children.Add(noSectionColumn);

            for (int i = 0; i < sections.Count; i++)
            {
                var section = sections[i];
                var sectionTasks = tasks.Where(t => t.Section == section.Name).ToList();
                var column = CreateColumn(section.Name, sectionTasks, section.IdSection, i + 1);
                Grid.SetRow(column, 0);
                Grid.SetColumn(column, i + 1);
                boardGrid.Children.Add(column);
            }

            var addSectionColumn = CreateColumn("", new List<TaskViewModel>(), null, sections.Count + 1, isAddSectionColumn: true);
            Grid.SetRow(addSectionColumn, 0);
            Grid.SetColumn(addSectionColumn, sections.Count + 1);
            boardGrid.Children.Add(addSectionColumn);

            return scrollViewer;
        }

        private Border CreateColumn(string title, List<TaskViewModel> tasks, int? sectionId, int columnIndex, bool isAddSectionColumn = false)
        {
            var columnStack = new StackPanel { Margin = new Thickness(10) };

            if (!isAddSectionColumn)
            {
                columnStack.Children.Add(new TextBlock
                {
                    Text = title,
                    FontWeight = FontWeights.Bold,
                    FontSize = 16,
                    Foreground = Brushes.White,
                    Margin = new Thickness(0, 0, 0, 10)
                });

                foreach (var task in tasks)
                {
                    columnStack.Children.Add(CreateTaskCard(task));
                }

                var addButton = new Button
                {
                    Content = "+ Добавить задачу",
                    Style = (Style)Application.Current.FindResource("AddTaskButtonStyle"),
                    Tag = sectionId,
                    Margin = new Thickness(0, 5, 0, 0)
                };
                addButton.Click += AddTaskToSection_Click;
                columnStack.Children.Add(addButton);
            }
            else
            {
                var addSectionButton = new Button
                {
                    Content = "Добавить раздел",
                    Margin = new Thickness(10),
                    Width = 200,
                    Height = 50,
                    Style = (Style)Application.Current.FindResource("AddTaskButtonStyle")
                };
                addSectionButton.Click += AddSection_Click;
                columnStack.Children.Add(addSectionButton);
            }

            return new Border
            {
                Child = columnStack
            };
        }

        private UIElement CreateTaskCard(TaskViewModel task)
        {
            var stack = new StackPanel();

            stack.Children.Add(new TextBlock
            {
                Text = task.Title,
                FontWeight = FontWeights.Bold,
                Foreground = Brushes.White
            });

            stack.Children.Add(new TextBlock
            {
                Text = task.Description,
                Foreground = Brushes.Gray,
                TextWrapping = TextWrapping.Wrap
            });

            stack.Children.Add(new TextBlock
            {
                Text = task.EndDateFormatted,
                Foreground = Brushes.LightCoral
            });

            return new Border
            {
                Background = new SolidColorBrush(Color.FromRgb(40, 40, 40)),
                Margin = new Thickness(0, 0, 0, 10),
                Padding = new Thickness(8),
                CornerRadius = new CornerRadius(6),
                Child = stack
            };
        }

        private void AddTaskToSection_Click(object sender, RoutedEventArgs e)
        {
            if (sender is Button button && button.Tag is int sectionId)
            {
                var addTaskWindow = new AddTaskWindow(_projectId, sectionId);
                if (addTaskWindow.ShowDialog() == true)
                {
                    // RefreshTasks();
                }
            }
        }

        private void AddTask_Click(object sender, RoutedEventArgs e)
        {
            var addTaskWindow = new AddTaskWindow(_projectId, null); 
            if (addTaskWindow.ShowDialog() == true)
            {
                // RefreshTasks();
            }
        }

        private void AddSection_Click(object sender, RoutedEventArgs e)
        {
            var dialog = new InputDialog("Введите название раздела:");
            if (dialog.ShowDialog() == true)
            {
                var sectionName = dialog.InputText;
                if (!string.IsNullOrWhiteSpace(sectionName))
                {
                    using (var context = new TaskManagementEntities1())
                    {
                        var newSection = new Section
                        {
                            Name = sectionName,
                            PageKey = !_isProjectBoard ? _pageKey : null
                        };

                        if (_isProjectBoard && _projectId.HasValue)
                        {
                            newSection.ProjectId = _projectId.Value;
                        }

                        context.Section.Add(newSection);
                        context.SaveChanges();
                    }

                    // RefreshTasks();
                }
            }
        }
    }
}
