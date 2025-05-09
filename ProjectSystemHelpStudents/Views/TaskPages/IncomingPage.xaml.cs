﻿// IncomingPage.xaml.cs
using ProjectSystemHelpStudents.Helper;
using System;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Data.Entity;

namespace ProjectSystemHelpStudents.UsersContent
{
    public partial class IncomingPage : Page
    {
        public IncomingPage()
        {
            InitializeComponent();
            EnsureInboxProjectExists();
            LoadTasks();
        }

        /// <summary>
        /// Проверяем и создаём (если нужно) проект "Входящие"
        /// </summary>
        private void EnsureInboxProjectExists()
        {
            try
            {
                using (var ctx = new TaskManagementEntities1())
                {
                    if (!ctx.Project.Any(p => p.Name == "Входящие"))
                    {
                        ctx.Project.Add(new Project { Name = "Входящие" });
                        ctx.SaveChanges();
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Не удалось убедиться в наличии проекта 'Входящие': " + ex.Message,
                                "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void LoadTasks()
        {
            try
            {
                using (var ctx = new TaskManagementEntities1())
                {
                    var incoming = ctx.Task
                        .Include(t => t.Status)
                        .Include(t => t.Project)
                        .Where(t =>
                            t.Project.Name == "Входящие" &&
                            t.IdUser == UserSession.IdUser)
                        .ToList();

                    var vms = incoming
                        .Select(t => new TaskViewModel
                        {
                            IdTask = t.IdTask,
                            Title = t.Title,
                            Description = t.Description,
                            IsCompleted = t.Status?.Name == "Завершено",
                            EndDate = t.EndDate,
                            EndDateFormatted = t.EndDate != DateTime.MinValue
                                              ? t.EndDate.ToString("dd MMMM yyyy")
                                              : "Без срока"
                        })
                        .Where(vm => !vm.IsCompleted)
                        .ToList();

                    TasksListView.ItemsSource = vms;
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Ошибка при загрузке задач: " + ex.Message,
                                "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void ToggleTaskStatus_Click(object sender, RoutedEventArgs e)
        {
            if (sender is CheckBox cb && cb.DataContext is TaskViewModel task)
            {
                using (var ctx = new TaskManagementEntities1())
                {
                    var dbTask = ctx.Task.Find(task.IdTask);
                    if (dbTask != null)
                    {
                        var done = ctx.Status.First(s => s.Name == "Завершено");
                        var undone = ctx.Status.First(s => s.Name == "Не завершено");
                        dbTask.StatusId = cb.IsChecked == true ? done.StatusId : undone.StatusId;
                        ctx.SaveChanges();
                    }
                }
                LoadTasks();
            }
        }

        private void TaskListView_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (sender is ListView lv && lv.SelectedItem is TaskViewModel vm)
            {
                var wnd = new TaskDetailsWindow(vm);
                wnd.ShowDialog();
                lv.SelectedItem = null;
                LoadTasks();
            }
        }

        private void ButtonCreateTask_Click(object sender, RoutedEventArgs e)
        {
            var addWnd = new AddTaskWindow(DateTime.Today);
            addWnd.ShowDialog();
            LoadTasks();
        }
    }
}
