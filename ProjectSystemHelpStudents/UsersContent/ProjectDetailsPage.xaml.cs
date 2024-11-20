using ProjectSystemHelpStudents.Helper;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows;
using System.Windows.Controls;

namespace ProjectSystemHelpStudents.UsersContent
{
    public partial class ProjectDetailsPage : Page
    {
        private int _projectId;

        public ProjectDetailsPage(int projectId)
        {
            InitializeComponent();
            _projectId = projectId;
            LoadProjectDetails();
            RefreshTasks();
        }

        private void LoadProjectDetails()
        {
            using (var context = new TaskManagementEntities1())
            {
                var project = context.Project.FirstOrDefault(p => p.ProjectId == _projectId);
                if (project != null)
                {
                    ProjectTitle.Text = project.Name;
                    ProjectDescription.Text = project.Description;
                    ProjectDates.Text = $"С {project.StartDate:dd.MM.yyyy} по {project.EndDate:dd.MM.yyyy}";
                }
            }
        }

        private void RefreshTasks()
        {
            try
            {
                var completedTasks = DBClass.entities.Task
                    .Where(t => t.Status != null && t.Status.Name == "Не Завершено" && t.IdUser == UserSession.IdUser && t.ProjectId == _projectId)
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

        private void TaskListView_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (sender is ListView listView && listView.SelectedItem is TaskViewModel task)
            {
                var detailsWindow = new TaskDetailsWindow(task);
                detailsWindow.TaskUpdated += RefreshTasks;
                detailsWindow.ShowDialog();
                listView.SelectedItem = null;
            }
        }

        private void ToggleTaskStatus_Click(object sender, RoutedEventArgs e)
        {
            if (sender is CheckBox checkBox && checkBox.DataContext is TaskViewModel task)
            {
                try
                {
                    using (var context = new TaskManagementEntities1())
                    {
                        var dbTask = context.Task.FirstOrDefault(t => t.IdTask == task.IdTask);
                        if (dbTask != null)
                        {
                            dbTask.StatusId = checkBox.IsChecked == true
                                ? context.Status.FirstOrDefault(s => s.Name == "Завершено")?.StatusId ?? dbTask.StatusId
                                : context.Status.FirstOrDefault(s => s.Name == "Не завершено")?.StatusId ?? dbTask.StatusId;

                            context.SaveChanges();
                            RefreshTasks();
                        }
                    }
                }
                catch (Exception ex)
                {
                    MessageBox.Show("Ошибка обновления статуса: " + ex.Message);
                }
            }
        }

        private void AddTask_Click(object sender, RoutedEventArgs e)
        {
            var addTaskWindow = new AddTaskWindow();
            addTaskWindow.ShowDialog();
            RefreshTasks();
        }
    }
}
