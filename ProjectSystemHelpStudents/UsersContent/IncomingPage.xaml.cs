using ProjectSystemHelpStudents.Helper;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace ProjectSystemHelpStudents.UsersContent
{
    /// <summary>
    /// Логика взаимодействия для IncomingPage.xaml
    /// </summary>
    public partial class IncomingPage : Page
    {
        public IncomingPage()
        {
            InitializeComponent();
            LoadTasks();
        }

        private void LoadTasks()
        {
            try
            {
                var completedTasks = DBClass.entities.Task
                    .Where(t => t.Project != null && t.Project.Name == "Входящие" && t.Status.Name == "Не завершено" && t.IdUser == UserSession.IdUser)
                    .Select(t => new TaskViewModel
                    {
                        Title = t.Title,
                        Description = t.Description,
                        Status = t.Status.Name,
                        EndDate = t.EndDate,
                        IsCompleted = false
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
                    dbTask.StatusId = (int)(checkBox.IsChecked == true ? DBClass.entities.Status.FirstOrDefault(s => s.Name == "Завершено")?.StatusId : DBClass.entities.Status.FirstOrDefault(s => s.Name == "Не завершено")?.StatusId);
                    DBClass.entities.SaveChanges();

                    LoadTasks();
                }
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

        private void ButtonCreateTask_Click(object sender, RoutedEventArgs e)
        {
            AddTaskWindow addTaskWindow = new AddTaskWindow();
            addTaskWindow.ShowDialog();
            LoadTasks();
        }
    }
}
