using ProjectSystemHelpStudents.Helper;
using System;
using System.Linq;
using System.Windows;

namespace ProjectSystemHelpStudents
{
    public partial class TaskDetailsWindow : Window
    {
        private TaskViewModel _task;

        public event Action TaskUpdated;

        public TaskDetailsWindow(TaskViewModel task)
        {
            InitializeComponent();
            _task = task;
            DataContext = _task;

            InitializeComboBoxes();
        }

        private void InitializeComboBoxes()
        {
            var projects = DBClass.entities.Project.ToList();
            ProjectComboBox.ItemsSource = projects;
            ProjectComboBox.SelectedValue = _task.ProjectId;

            var priorities = DBClass.entities.Priority.ToList();
            PriorityComboBox.ItemsSource = priorities;
            PriorityComboBox.SelectedValue = _task.PriorityId;
        }

        private void SaveButton_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                _task.Title = TitleTextBox.Text;
                _task.Description = DescriptionTextBox.Text;
                _task.EndDate = EndDatePicker.SelectedDate ?? DateTime.Now;
                _task.ProjectId = (int?)ProjectComboBox.SelectedValue ?? 0;
                _task.PriorityId = (int?)PriorityComboBox.SelectedValue ?? 0;

                var dbTask = DBClass.entities.Task.FirstOrDefault(t => t.IdTask == _task.IdTask);
                if (dbTask != null)
                {
                    dbTask.Title = _task.Title;
                    dbTask.Description = _task.Description;
                    dbTask.EndDate = _task.EndDate;
                    dbTask.ProjectId = _task.ProjectId;
                    dbTask.PriorityId = _task.PriorityId;
                    DBClass.entities.SaveChanges();
                }

                MessageBox.Show("Задача успешно сохранена!", "Успех", MessageBoxButton.OK, MessageBoxImage.Information);

                TaskUpdated?.Invoke();

                Close();
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка: {ex.Message}", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void ToggleTaskStatus_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                var dbTask = DBClass.entities.Task.FirstOrDefault(t => t.IdTask == _task.IdTask);
                if (dbTask != null)
                {
                    dbTask.StatusId = IsCompletedCheckBox.IsChecked == true
                        ? DBClass.entities.Status.FirstOrDefault(s => s.Name == "Завершено")?.StatusId ?? dbTask.StatusId
                        : DBClass.entities.Status.FirstOrDefault(s => s.Name == "Не завершено")?.StatusId ?? dbTask.StatusId;
                    DBClass.entities.SaveChanges();
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка обновления статуса: {ex.Message}", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void Button_Click(object sender, RoutedEventArgs e)
        {
            MessageBox.Show("Добавление подзадач в разработке.", "Информация", MessageBoxButton.OK, MessageBoxImage.Information);
        }
    }
}
