using ProjectSystemHelpStudents.Helper;
using System;
using System.Linq;
using System.Windows;
using System.Windows.Controls;

namespace ProjectSystemHelpStudents.UsersContent
{
    public partial class CompletedPage : Page
    {
        public CompletedPage()
        {
            InitializeComponent();
            LoadTasks();
        }

        private void LoadTasks()
        {
            try
            {
                var completedTasks = DBClass.entities.Task
                    .Where(t => t.Status != null && t.Status.Name == "Завершено" && t.IdUser == UserSession.IdUser)
                    .Select(t => new TaskViewModel
                    {
                        Title = t.Title,
                        Description = t.Description,
                        Status = t.Status.Name,
                        EndDate = t.EndDate,
                        IsCompleted = true
                    })
                    .ToList();

                foreach (var task in completedTasks)
                {
                    task.EndDateFormatted = task.EndDate != DateTime.MinValue
                        ? task.EndDate.ToString("dd MMMM yyyy")
                        : "Без срока";
                }

                TasksListView.ItemsSource = null;
                TasksListView.ItemsSource = completedTasks;
            }
            catch (Exception ex)
            {
                MessageBox.Show("Ошибка при загрузке задач: " + ex.Message);
            }
        }


        private void ToggleTaskStatus_Click(object sender, RoutedEventArgs e)
        {
            var checkBox = sender as CheckBox;
            var task = checkBox.DataContext as TaskViewModel;

            if (task != null)
            {
                var dbTask = DBClass.entities.Task.FirstOrDefault(t => t.Title == task.Title);
                if (dbTask != null)
                {
                    dbTask.StatusId = (int)(checkBox.IsChecked == true
                        ? DBClass.entities.Status.FirstOrDefault(s => s.Name == "Завершено")?.StatusId
                        : DBClass.entities.Status.FirstOrDefault(s => s.Name == "Не завершено")?.StatusId);

                    DBClass.entities.SaveChanges();

                    LoadTasks();
                }
            }
        }

        private void DeleteHistory_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                var completedTasks = DBClass.entities.Task
                    .Where(t => t.Status != null && t.Status.Name == "Завершено")
                    .ToList();

                DBClass.entities.Task.RemoveRange(completedTasks);
                DBClass.entities.SaveChanges();

                MessageBox.Show("История завершенных задач очищена.");

                LoadTasks();
            }
            catch (Exception ex)
            {
                MessageBox.Show("Ошибка при удалении завершенных задач: " + ex.Message);
            }
        }
    
        private void TaskListView_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (sender is ListView listView && listView.SelectedItem is TaskViewModel selectedTask)
            {
                var detailsWindow = new TaskDetailsWindow(selectedTask);
                detailsWindow.ShowDialog();
                listView.SelectedItem = null;
            }
        }

    }
}
