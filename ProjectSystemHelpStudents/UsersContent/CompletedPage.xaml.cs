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
    /// Логика взаимодействия для CompletedPage.xaml
    /// </summary>
    public partial class CompletedPage : Page
    {
        public CompletedPage()
        {
            InitializeComponent();
            UpdateTodayDateText();
            LoadTasks();
        }

        private void UpdateTodayDateText()
        {
            string todayDate = DateTime.Today.ToString("dd MMMM");
            string dayOfWeek = DateTime.Today.ToString("dddd");
            
        }

        private void LoadTasks()
        {
            try
            {
                var allTasks = DBClass.entities.Task
                    .Select(t => new TaskViewModel
                    {
                        Title = t.Title,
                        Description = t.Description,
                        Status = t.Status != null ? t.Status.Name : "Без статуса",
                        EndDate = t.EndDate,
                        IsCompleted = t.Status != null && t.Status.Name == "Завершено"
                    })
                    .ToList();

                foreach (var task in allTasks)
                {
                    task.EndDateFormatted = task.EndDate != DateTime.MinValue
                        ? task.EndDate.ToString("dd MMMM yyyy")
                        : "Без срока";
                }

                var todayTasks = allTasks
                    .Where(t => t.EndDateFormatted != "Без срока" && DateTime.Parse(t.EndDateFormatted) == DateTime.Today && !t.IsCompleted)
                    .ToList();

                TasksListView.ItemsSource = todayTasks;
            }
            catch (Exception ex)
            {
                MessageBox.Show("Ошибка при загрузке задач: " + ex.Message);
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
                        : DBClass.entities.Status.FirstOrDefault(s => s.Name == "В процессе")?.StatusId);

                    DBClass.entities.SaveChanges();
                    LoadTasks();
                }
            }
        }
    }
}
