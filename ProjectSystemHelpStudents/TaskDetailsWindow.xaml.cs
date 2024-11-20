using Microsoft.Win32;
using ProjectSystemHelpStudents.Helper;
using System;
using System.IO;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Xml.Linq;

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
            LoadTaskFiles();
            LoadComments();
        }
        private void LoadComments()
        {
            var comments = DBClass.entities.Comment
                .Where(c => c.IdTask == _task.IdTask)
                .OrderByDescending(c => c.CreatedAt)
                .ToList()
                .Select(c => new
                {
                    c.Content,
                    CreatedAt = c.CreatedAt.HasValue ? c.CreatedAt.Value.ToString("dd.MM.yyyy HH:mm") : "Не указано"
                })
                .ToList();

            CommentsListBox.ItemsSource = comments
                .Select(c => $"{c.CreatedAt}: {c.Content}");
        }

        private void LoadTaskFiles()
        {
            var files = DBClass.entities.Files
                .Where(f => f.TaskId == _task.IdTask)
                .Select(f => new FileItem
                {
                    Id = f.Id,
                    FilePath = f.FilePath
                })
                .ToList();

            FilesListBox.ItemsSource = files;
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

        private void ButtonAddSubTasks_Click(object sender, RoutedEventArgs e)
        {
            MessageBox.Show("Добавление подзадач в разработке.", "Информация", MessageBoxButton.OK, MessageBoxImage.Information);
        }

        private void SendingCommentButton_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                string commentContent = CommentTextBox.Text.Trim();
                if (string.IsNullOrWhiteSpace(commentContent))
                {
                    MessageBox.Show("Комментарий не может быть пустым.", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Warning);
                    return;
                }

                var newComment = new Comment
                {
                    IdUser = UserSession.IdUser,
                    IdTask = _task.IdTask,
                    Content = commentContent,
                    CreatedAt = DateTime.Now
                };

                DBClass.entities.Comment.Add(newComment);
                DBClass.entities.SaveChanges();

                CommentTextBox.Clear();
                LoadComments();

                MessageBox.Show("Комментарий добавлен!", "Успех", MessageBoxButton.OK, MessageBoxImage.Information);
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка добавления комментария: {ex.Message}", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void AddFileButton_Click(object sender, RoutedEventArgs e)
        {
            var openFileDialog = new OpenFileDialog
            {
                Title = "Выберите файл",
                Filter = "Все файлы (*.*)|*.*"
            };

            if (openFileDialog.ShowDialog() == true)
            {
                try
                {
                    string selectedFilePath = openFileDialog.FileName;

                    string projectDirectory = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "TaskFiles");
                    Directory.CreateDirectory(projectDirectory);

                    string fileName = Path.GetFileName(selectedFilePath);
                    string destinationPath = Path.Combine(projectDirectory, fileName);

                    File.Copy(selectedFilePath, destinationPath, true);

                    // Добавление записи в таблицу Files
                    var newFile = new Files
                    {
                        FilePath = destinationPath,
                        TaskId = _task.IdTask
                    };
                    DBClass.entities.Files.Add(newFile);
                    DBClass.entities.SaveChanges();

                    LoadTaskFiles();
                    MessageBox.Show("Файл успешно добавлен к задаче!", "Успех", MessageBoxButton.OK, MessageBoxImage.Information);
                }
                catch (Exception ex)
                {
                    MessageBox.Show($"Ошибка при добавлении файла: {ex.Message}", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
                }
            }
        }
        private void FilesListBox_MouseDoubleClick(object sender, System.Windows.Input.MouseButtonEventArgs e)
        {
            if (FilesListBox.SelectedItem is FileItem selectedFile)
            {
                try
                {
                    System.Diagnostics.Process.Start("explorer.exe", selectedFile.FilePath);
                }
                catch (Exception ex)
                {
                    MessageBox.Show($"Не удалось открыть файл: {ex.Message}", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
                }
            }
        }
    }
}
