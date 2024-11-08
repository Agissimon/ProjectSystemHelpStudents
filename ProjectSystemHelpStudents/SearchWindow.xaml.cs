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
using System.Windows.Shapes;

namespace ProjectSystemHelpStudents
{
    /// <summary>
    /// Логика взаимодействия для SearchWindow.xaml
    /// </summary>
    public partial class SearchWindow : Window
    {
        private List<string> searchHistory = new List<string>();

        public SearchWindow()
        {
            InitializeComponent();
            //this.PreviewKeyDown += new KeyEventHandler(MainWindow_KeyDown);
        }

        private void txtSearchQuery_TextChanged(object sender, TextChangedEventArgs e)
        {
            string query = txtSearchQuery.Text.Trim();
            if (!string.IsNullOrEmpty(query))
            {
                Search(query);
            }
        }

        private void SearchButton_Click(object sender, RoutedEventArgs e)
        {
            string query = txtSearchQuery.Text.Trim();
            if (!string.IsNullOrEmpty(query))
            {
                searchHistory.Add(query);
                lstSearchHistory.ItemsSource = null;
                lstSearchHistory.ItemsSource = searchHistory;

                Search(query);
            }
        }

        private void Search(string query)
        {
            var results = new List<string>();

            // Получение задач с TaskId и Title
            var taskResults = DBClass.entities.Task
                .Where(t => t.Title.Contains(query) || t.Description.Contains(query))
                .Select(t => new { t.IdTask, t.Title })
                .ToList()
                .Select(task => $"Задача: {task.Title} (ID: {task.IdTask})");

            var commentResults = DBClass.entities.Comment
                .Where(c => c.Content.Contains(query))
                .Select(c => c.Content)
                .ToList()
                .Select(content => $"Комментарий: {content}");

            var fileResults = DBClass.entities.Files
                .Where(f => f.FilePath.Contains(query))
                .Select(f => f.FilePath)
                .ToList()
                .Select(filePath => $"Файл: {filePath}");

            results.AddRange(taskResults);
            results.AddRange(commentResults);
            results.AddRange(fileResults);

            lstSearchResults.ItemsSource = results;
        }

        private void lstSearchResults_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (lstSearchResults.SelectedItem is string selectedItem && selectedItem.StartsWith("Задача:"))
            {
                string taskTitle = selectedItem.Replace("Задача: ", "");
                var task = DBClass.entities.Task.FirstOrDefault(t => t.Title == taskTitle);

                if (task != null)
                {
                    var taskViewModel = ConvertToTaskViewModel(task);
                    var taskDetailsWindow = new TaskDetailsWindow(taskViewModel);
                    taskDetailsWindow.ShowDialog();
                }
            }
        }

        private TaskViewModel ConvertToTaskViewModel(Task task)
        {
            return new TaskViewModel
            {
                Title = task.Title,
                Description = task.Description,
                Status = DBClass.entities.Status.FirstOrDefault(s => s.StatusId == task.StatusId)?.Name ?? "Неизвестно",
                EndDate = task.EndDate,
                EndDateFormatted = task.EndDate.ToString("dd.MM.yyyy"),
                IsCompleted = task.StatusId == DBClass.entities.Status.FirstOrDefault(s => s.Name == "Завершено")?.StatusId
            };
        }

        //private void MainWindow_KeyDown(object sender, KeyEventArgs e)
        //{
        //    if (e.Key == Key.F && Keyboard.Modifiers == ModifierKeys.Control)
        //    {
        //        var searchWindow = new SearchWindow();
        //        searchWindow.Show();
        //    }
        //}
    }
}
