using ProjectSystemHelpStudents.Helper;
using System;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Data.Entity;
using ProjectSystemHelpStudents.ViewModels;

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
                using (var ctx = new TaskManagementEntities1())
                {
                    int userId = UserSession.IdUser;
                    var completedTasks = ctx.Task
                        .Include("Status")
                        .Include("TaskAssignee")
                        .ForUser(userId)
                        .Where(t => t.Status.Name == "Завершено")
                        .Select(t => new TaskViewModel
                        {
                            IdTask = t.IdTask,
                            CreatorId = t.CreatorId,
                            Title = t.Title,
                            Description = t.Description,
                            Status = t.Status.Name,
                            EndDate = t.EndDate,
                            IsCompleted = true
                        })
                        .ToList();

                    completedTasks.ForEach(vm =>
                        vm.EndDateFormatted = vm.EndDate != DateTime.MinValue
                            ? vm.EndDate.ToString("dd MMMM yyyy")
                            : "Без срока");

                    TasksListView.ItemsSource = completedTasks;
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Ошибка при загрузке задач: " + ex.Message);
            }
        }

        private void ToggleTaskStatus_Click(object sender, RoutedEventArgs e)
        {
            var checkBox = sender as CheckBox;
            var vm = checkBox?.DataContext as TaskViewModel;
            if (vm == null) return;

            var dbTask = DBClass.entities.Task
                             .FirstOrDefault(t => t.IdTask == vm.IdTask);
            if (dbTask == null) return;

            var newStatusName = checkBox.IsChecked == true
                                ? "Завершено"
                                : "Не завершено";
            var newStatus = DBClass.entities.Status
                            .FirstOrDefault(s => s.Name == newStatusName);
            if (newStatus == null) return;

            dbTask.StatusId = newStatus.StatusId;
            DBClass.entities.SaveChanges();

            LoadTasks();
        }

        private void DeleteHistory_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                var completedTasks = DBClass.entities.Task
                    .Where(t => t.Status != null
                                && t.Status.Name == "Завершено"
                                && t.CreatorId == UserSession.IdUser)
                    .ToList();

                if (completedTasks.Any())
                {
                    DBClass.entities.Task.RemoveRange(completedTasks);
                    DBClass.entities.SaveChanges();
                    MessageBox.Show("История ваших завершённых задач очищена.");
                }
                else
                {
                    MessageBox.Show("У вас нет завершённых задач для очистки.");
                }

                LoadTasks();
            }
            catch (Exception ex)
            {
                MessageBox.Show("Ошибка при удалении завершённых задач: " + ex.Message);
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
