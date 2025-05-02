using Microsoft.Win32;
using ProjectSystemHelpStudents.Helper;
using System;
using System.Collections.ObjectModel;
using System.Data.Entity;
using System.Data.Entity.Infrastructure;
using System.IO;
using System.Linq;
using System.Windows;
using System.Windows.Controls;

namespace ProjectSystemHelpStudents
{
    public partial class TaskDetailsWindow : Window
    {
        private readonly TaskViewModel _task;
        public event Action TaskUpdated;

        public TaskDetailsWindow(TaskViewModel task)
        {
            InitializeComponent();
            using (var ctx = new TaskManagementEntities1())
            {
                var dbTask = ctx.Task
                    .Include(t => t.Status)
                    .FirstOrDefault(t => t.IdTask == task.IdTask);
                if (dbTask != null)
                {
                    task.Title = dbTask.Title;
                    task.Description = dbTask.Description;
                    task.EndDate = dbTask.EndDate;
                    task.ProjectId = dbTask.ProjectId ?? 0;
                    task.PriorityId = dbTask.PriorityId;
                    task.IsCompleted = dbTask.Status != null && dbTask.Status.Name == "Завершено";
                }
            }

            _task = task;
            DataContext = _task;

            InitializeComboBoxes();

            LoadLabels();
            LoadComments();
            LoadTaskFiles();
        }

        private void InitializeComboBoxes()
        {
            using (var ctx = new TaskManagementEntities1())
            {
                ProjectComboBox.ItemsSource = ctx.Project.ToList();
                PriorityComboBox.ItemsSource = ctx.Priority.ToList();
            }
            ProjectComboBox.SelectedValue = _task.ProjectId;
            PriorityComboBox.SelectedValue = _task.PriorityId;
        }

        private void LoadLabels()
        {
            using (var ctx = new TaskManagementEntities1())
            {
                var allLabels = ctx.Labels.ToList();
                _task.AvailableLabels = new ObservableCollection<LabelViewModel>(
                    allLabels.Select(label => new LabelViewModel
                    {
                        Id = label.Id,
                        Name = label.Name,
                        IsSelected = ctx.TaskLabels.Any(tl => tl.TaskId == _task.IdTask && tl.LabelId == label.Id)
                    })
                );
            }
            LabelsListBox.ItemsSource = _task.AvailableLabels;
        }

        private void LoadComments()
        {
            using (var ctx = new TaskManagementEntities1())
            {
                var comments = ctx.Comment
                    .Where(c => c.IdTask == _task.IdTask)
                    .OrderByDescending(c => c.CreatedAt)
                    .ToList();
                CommentsListBox.ItemsSource = new ObservableCollection<Comment>(comments);
            }
        }

        private void LoadTaskFiles()
        {
            using (var ctx = new TaskManagementEntities1())
            {
                var files = ctx.Files
                    .Where(f => f.TaskId == _task.IdTask)
                    .ToList()
                    .Select(f => new FileItem
                    {
                        FileName = Path.GetFileName(f.FilePath),
                        FilePath = f.FilePath
                    })
                    .ToList();
                FilesListBox.ItemsSource = new ObservableCollection<FileItem>(files);
            }
        }

        private void SaveButton_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                _task.Title = TitleTextBox.Text.Trim();
                _task.Description = DescriptionTextBox.Text.Trim();
                _task.EndDate = EndDatePicker.SelectedDate ?? DateTime.Now;
                _task.ProjectId = (int?)(ProjectComboBox.SelectedValue) ?? _task.ProjectId;
                _task.PriorityId = (int?)(PriorityComboBox.SelectedValue) ?? _task.PriorityId;

                using (var ctx = new TaskManagementEntities1())
                {
                    var dbTask = ctx.Task
                        .Include(t => t.TaskLabels)
                        .FirstOrDefault(t => t.IdTask == _task.IdTask);
                    if (dbTask == null)
                        throw new InvalidOperationException("Задача не найдена в базе");

                    // Обновляем поля
                    dbTask.Title = _task.Title;
                    dbTask.Description = _task.Description;
                    dbTask.EndDate = _task.EndDate;
                    dbTask.ProjectId = _task.ProjectId;
                    dbTask.PriorityId = _task.PriorityId;

                    // Метки
                    ctx.TaskLabels.RemoveRange(dbTask.TaskLabels);
                    foreach (var labelVm in _task.AvailableLabels.Where(l => l.IsSelected))
                    {
                        ctx.TaskLabels.Add(new TaskLabels
                        {
                            TaskId = _task.IdTask,
                            LabelId = labelVm.Id
                        });
                    }

                    ctx.SaveChanges();
                }

                MessageBox.Show("Задача успешно сохранена!", "Успех", MessageBoxButton.OK, MessageBoxImage.Information);
                TaskUpdated?.Invoke();
                Close();
            }
            catch (DbUpdateException dbEx)
            {
                var msg = dbEx.InnerException?.InnerException?.Message ?? dbEx.Message;
                MessageBox.Show($"Ошибка при сохранении в БД:\n{msg}", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
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
                bool isDone = IsCompletedCheckBox.IsChecked == true;
                using (var ctx = new TaskManagementEntities1())
                {
                    var dbTask = ctx.Task.FirstOrDefault(t => t.IdTask == _task.IdTask);
                    if (dbTask == null)
                        throw new InvalidOperationException("Задача не найдена при смене статуса");

                    var doneStatus = ctx.Status.FirstOrDefault(s => s.Name == "Завершено");
                    var undoneStatus = ctx.Status.FirstOrDefault(s => s.Name == "Не завершено");
                    dbTask.StatusId = isDone
                        ? doneStatus?.StatusId ?? dbTask.StatusId
                        : undoneStatus?.StatusId ?? dbTask.StatusId;

                    ctx.SaveChanges();
                }

                _task.IsCompleted = isDone;
                TaskUpdated?.Invoke();
            }
            catch (DbUpdateException dbEx)
            {
                var msg = dbEx.InnerException?.InnerException?.Message ?? dbEx.Message;
                MessageBox.Show($"Ошибка при обновлении статуса в БД:\n{msg}", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
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
                string content = CommentTextBox.Text.Trim();
                if (string.IsNullOrWhiteSpace(content))
                {
                    MessageBox.Show("Комментарий не может быть пустым.", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Warning);
                    return;
                }
                using (var ctx = new TaskManagementEntities1())
                {
                    ctx.Comment.Add(new Comment
                    {
                        IdUser = UserSession.IdUser,
                        IdTask = _task.IdTask,
                        Content = content,
                        CreatedAt = DateTime.Now
                    });
                    ctx.SaveChanges();
                }
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
            var dlg = new OpenFileDialog { Title = "Выберите файл", Filter = "Все файлы (*.*)|*.*" };
            if (dlg.ShowDialog() != true) return;
            try
            {
                string destDir = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "TaskFiles");
                Directory.CreateDirectory(destDir);
                string destPath = Path.Combine(destDir, Path.GetFileName(dlg.FileName));
                File.Copy(dlg.FileName, destPath, true);
                using (var ctx = new TaskManagementEntities1())
                {
                    ctx.Files.Add(new Files { FilePath = destPath, TaskId = _task.IdTask });
                    ctx.SaveChanges();
                }
                LoadTaskFiles();
                MessageBox.Show("Файл успешно добавлен к задаче!", "Успех", MessageBoxButton.OK, MessageBoxImage.Information);
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка при добавлении файла: {ex.Message}", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void FilesListBox_MouseDoubleClick(object sender, System.Windows.Input.MouseButtonEventArgs e)
        {
            if (FilesListBox.SelectedItem is FileItem file)
            {
                try
                {
                    System.Diagnostics.Process.Start(new System.Diagnostics.ProcessStartInfo { FileName = file.FilePath, UseShellExecute = true });
                }
                catch (Exception ex)
                {
                    MessageBox.Show($"Не удалось открыть файл: {ex.Message}", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
                }
            }
        }

        private void DeleteFileButton_Click(object sender, RoutedEventArgs e)
        {
            if (!(sender is Button btn && btn.DataContext is FileItem file)) return;
            try
            {
                using (var ctx = new TaskManagementEntities1())
                {
                    var dbFile = ctx.Files.FirstOrDefault(f => f.FilePath == file.FilePath);
                    if (dbFile != null)
                    {
                        ctx.Files.Remove(dbFile);
                        ctx.SaveChanges();
                    }
                }
                if (File.Exists(file.FilePath)) File.Delete(file.FilePath);
                LoadTaskFiles();
                MessageBox.Show("Файл успешно удален!", "Успех", MessageBoxButton.OK, MessageBoxImage.Information);
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка при удалении файла: {ex.Message}", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void DeleteCommentButton_Click(object sender, RoutedEventArgs e)
        {
            if (!(sender is Button btn && btn.DataContext is Comment comm)) return;
            try
            {
                using (var ctx = new TaskManagementEntities1())
                {
                    var dbComm = ctx.Comment.FirstOrDefault(c => c.Id == comm.Id);
                    if (dbComm != null)
                    {
                        ctx.Comment.Remove(dbComm);
                        ctx.SaveChanges();
                    }
                }
                LoadComments();
                MessageBox.Show("Комментарий успешно удален!", "Успех", MessageBoxButton.OK, MessageBoxImage.Information);
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка при удалении комментария: {ex.Message}", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }
    }
}
