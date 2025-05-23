﻿using Microsoft.Win32;
using ProjectSystemHelpStudents.Helper;
using ProjectSystemHelpStudents.ViewModels;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Data.Entity;
using System.Data.Entity.Validation;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;

namespace ProjectSystemHelpStudents
{
    public partial class TaskDetailsWindow : Window
    {
        private readonly TaskViewModel _task;
        public event Action TaskUpdated;
        private const long MAX_FILE_BYTES = 20 * 1024 * 1024; // 20 MB

        public TaskDetailsWindow(TaskViewModel task)
        {
            InitializeComponent();

            using (var ctx = new TaskManagementEntities1())
            {
                var dbTask = ctx.Task
                    .Include(t => t.Status)
                    .Include(t => t.TaskAssignee)
                    .FirstOrDefault(t => t.IdTask == task.IdTask);

                if (dbTask == null)
                {
                    MessageBox.Show("Задача не найдена.", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Warning);
                    this.Loaded += (_, __) => this.Close();
                    return;
                }

                // проверка, на автора, либо запись всё ещё в TaskAssignee
                bool amIAssigned = dbTask.TaskAssignee.Any(ta => ta.UserId == UserSession.IdUser);
                if (!amIAssigned && dbTask.CreatorId != UserSession.IdUser)
                {
                    MessageBox.Show("У вас нет прав на просмотр этой задачи.", "Доступ запрещён",
                                    MessageBoxButton.OK, MessageBoxImage.Warning);
                    this.Loaded += (_, __) => this.Close();
                    return;
                }

                task.Title = dbTask.Title;
                task.Description = dbTask.Description;
                task.EndDate = dbTask.EndDate;
                task.ProjectId = dbTask.ProjectId ?? 0;
                task.PriorityId = dbTask.PriorityId;
                task.IsCompleted = dbTask.Status?.Name == "Завершено";
                task.IdUser = dbTask.CreatorId;
                task.ReminderDate = dbTask.ReminderDate;
            }

            _task = task;
            DataContext = _task;

            InitializeComboBoxes();
            LoadAssignees();
            LoadLabels();
            LoadComments();
            LoadTaskFiles();
        }

        private void InitializeComboBoxes()
        {
            int currentUser = UserSession.IdUser;
            using (var ctx = new TaskManagementEntities1())
            {
                var personal = ctx.Project.Where(p => p.OwnerId == currentUser).ToList();

                var teamIds = ctx.TeamMember
                                 .Where(tm => tm.UserId == currentUser)
                                 .Select(tm => tm.TeamId)
                                 .Union(
                                    ctx.Team.Where(t => t.LeaderId == currentUser).Select(t => t.TeamId)
                                 )
                                 .Distinct()
                                 .ToList();

                var teamProjects = ctx.Project
                                      .Where(p => p.TeamId != null && teamIds.Contains(p.TeamId.Value))
                                      .ToList();

                var allowedProjects = personal
                    .Union(teamProjects)
                    .Distinct()
                    .ToList();

                ProjectComboBox.ItemsSource = allowedProjects;
                PriorityComboBox.ItemsSource = ctx.Priority.ToList();
            }

            ProjectComboBox.SelectedValue = _task.ProjectId;
            PriorityComboBox.SelectedValue = _task.PriorityId;
        }

        private void LoadLabels()
        {
            int currentUser = UserSession.IdUser;

            using (var ctx = new TaskManagementEntities1())
            {
                var userLabels = ctx.Labels
                    .Where(l => l.UserId == currentUser)
                    .ToList();

                _task.AvailableLabels = new ObservableCollection<LabelViewModel>(
                    userLabels.Select(label => new LabelViewModel
                    {
                        Id = label.Id,
                        Name = label.Name,
                        IsSelected = ctx.TaskLabels
                                        .Any(tl => tl.TaskId == _task.IdTask
                                                && tl.LabelId == label.Id)
                    })
                );
            }

            LabelsListBox.ItemsSource = _task.AvailableLabels;
        }

        private void LoadComments()
        {
            using (var ctx = new TaskManagementEntities1())
            {
                var items = (from c in ctx.Comment
                             where c.IdTask == _task.IdTask
                             join u in ctx.Users on c.IdUser equals u.IdUser
                             orderby c.CreatedAt descending
                             select new CommentItem
                             {
                                 Id = c.Id,
                                 IdUser = (int)c.IdUser,
                                 UserName = u.Name,
                                 Content = c.Content,
                                 CreatedAt = (DateTime)c.CreatedAt
                             })
                            .ToList();

                CommentsListBox.ItemsSource = new ObservableCollection<CommentItem>(items);
            }
        }

        private void LoadTaskFiles()
        {
            using (var ctx = new TaskManagementEntities1())
            {
                var files = ctx.Files
                    .Where(f => f.TaskId == _task.IdTask)
                    .Select(f => new FileItem
                    {
                        Id = f.Id,
                        FileName = f.FileName,
                        FileType = f.FileType
                    })
                    .ToList();

                FilesListBox.ItemsSource = new ObservableCollection<FileItem>(files);
            }
        }

        private void LoadAssignees()
        {
            int currentUser = UserSession.IdUser;
            using (var ctx = new TaskManagementEntities1())
            {
                // Все пользователи из команды/проекта
                var teamIds = ctx.TeamMember
                                 .Where(tm => tm.UserId == currentUser)
                                 .Select(tm => tm.TeamId)
                                 .Union(ctx.Team.Where(t => t.LeaderId == currentUser).Select(t => t.TeamId))
                                 .ToList();

                var allowedIds = ctx.TeamMember
                                   .Where(tm => teamIds.Contains(tm.TeamId))
                                   .Select(tm => tm.UserId)
                               .Union(ctx.Team.Where(t => teamIds.Contains(t.TeamId)).Select(t => t.LeaderId))
                                   .Distinct()
                                   .ToList();

                var users = ctx.Users.Where(u => allowedIds.Contains(u.IdUser)).ToList();

                // Загрузка автора и уже назначенных
                var dbTask = ctx.Task.Find(_task.IdTask);
                var creatorId = dbTask.CreatorId;
                var assignedIds = ctx.TaskAssignee
                                     .Where(ta => ta.TaskId == _task.IdTask)
                                     .Select(ta => ta.UserId)
                                     .ToList();

                _task.Assignees.Clear();
                foreach (var u in users)
                {
                    _task.Assignees.Add(new AssigneeViewModel
                    {
                        UserId = u.IdUser,
                        Name = u.Name,
                        IsCreator = u.IdUser == creatorId,
                        IsAssigned = assignedIds.Contains(u.IdUser) || u.IdUser == creatorId
                    });
                }
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

                DateTime? newRem = null;
                if (dpEditRemindDate.SelectedDate.HasValue
                    && TimeSpan.TryParse(tbEditRemindTime.Text, out var ts))
                {
                    newRem = dpEditRemindDate.SelectedDate.Value.Date + ts;
                }

                using (var ctx = new TaskManagementEntities1())
                {
                    var dbTask = ctx.Task
                        .Include(t => t.TaskLabels)
                        .Include(t => t.TaskAssignee)
                        .FirstOrDefault(t => t.IdTask == _task.IdTask);

                    if (dbTask == null)
                        throw new InvalidOperationException("Задача не найдена");

                    // 🔒 Проверка на владельца задачи
                    if (dbTask.CreatorId != UserSession.IdUser)
                    {
                        MessageBox.Show("Только автор может изменять задачу.", "Доступ запрещён", MessageBoxButton.OK, MessageBoxImage.Warning);
                        return;
                    }

                    dbTask.Title = _task.Title;
                    dbTask.Description = _task.Description;
                    dbTask.EndDate = _task.EndDate;
                    dbTask.ProjectId = _task.ProjectId;
                    dbTask.PriorityId = _task.PriorityId;
                    dbTask.ReminderDate = newRem;

                    ctx.TaskLabels.RemoveRange(dbTask.TaskLabels);
                    foreach (var lab in _task.AvailableLabels.Where(l => l.IsSelected))
                        ctx.TaskLabels.Add(new TaskLabels { TaskId = _task.IdTask, LabelId = lab.Id });

                    // Обновление назначений (кроме автора)
                    var toRemove = dbTask.TaskAssignee
                        .Where(ta => !_task.Assignees.Any(vm => vm.IsAssigned && vm.UserId == ta.UserId)
                                     && ta.UserId != dbTask.CreatorId)
                        .ToList();
                    ctx.TaskAssignee.RemoveRange(toRemove);

                    var exist = dbTask.TaskAssignee.Select(ta => ta.UserId).ToList();
                    var toAdd = _task.Assignees
                        .Where(vm => vm.IsAssigned && !exist.Contains(vm.UserId))
                        .Select(vm => new TaskAssignee
                        {
                            TaskId = _task.IdTask,
                            UserId = vm.UserId,
                            IsNew = true,
                            AssignedAt = DateTime.Now
                        })
                        .ToList();

                    ctx.TaskAssignee.AddRange(toAdd);
                    ctx.SaveChanges();
                    UserSession.RaiseNotificationsChanged();

                    var selectedIds = ctx.TaskLabels
                        .Where(tl => tl.TaskId == _task.IdTask)
                        .Select(tl => tl.LabelId)
                        .ToList();

                    foreach (var labVm in _task.AvailableLabels)
                    {
                        labVm.IsSelected = selectedIds.Contains(labVm.Id);
                    }
                }

                MessageBox.Show("Задача сохранена", "Успех", MessageBoxButton.OK, MessageBoxImage.Information);
                TaskUpdated?.Invoke();
                Close();
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка при сохранении: {ex.Message}", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
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
                        throw new InvalidOperationException("Задача не найдена");

                    var done = ctx.Status.First(s => s.Name == "Завершено");
                    var undone = ctx.Status.First(s => s.Name == "Не завершено");
                    dbTask.StatusId = isDone ? done.StatusId : undone.StatusId;

                    ctx.SaveChanges();
                }

                _task.IsCompleted = isDone;
                TaskUpdated?.Invoke();
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка при смене статуса: {ex.Message}", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
            }
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
            var dlg = new OpenFileDialog
            {
                Title = "Выберите файл",
                Filter = "Все файлы (*.*)|*.*"
            };
            if (dlg.ShowDialog() != true)
                return;

            var fi = new FileInfo(dlg.FileName);
            if (fi.Length > MAX_FILE_BYTES)
            {
                MessageBox.Show(
                    $"Файл слишком большой. Максимум {MAX_FILE_BYTES / 1024 / 1024} МБ.",
                    "Ошибка размера",
                    MessageBoxButton.OK,
                    MessageBoxImage.Warning);
                return;
            }

            try
            {
                byte[] data = File.ReadAllBytes(dlg.FileName);
                using (var ctx = new TaskManagementEntities1())
                {
                    var fileEntity = new Files
                    {
                        TaskId = _task.IdTask,
                        FileName = fi.Name,
                        FileType = fi.Extension,
                        FileData = data
                    };
                    ctx.Files.Add(fileEntity);
                    ctx.SaveChanges();
                }

                LoadTaskFiles();
                MessageBox.Show("Файл успешно добавлен!", "Успех", MessageBoxButton.OK, MessageBoxImage.Information);
            }
            catch (DbEntityValidationException dbEx)
            {
                var sb = new System.Text.StringBuilder("Ошибка валидации при сохранении файла:\n");
                foreach (var eve in dbEx.EntityValidationErrors)
                {
                    sb.AppendLine($"Entity \"{eve.Entry.Entity.GetType().Name}\":");
                    foreach (var ve in eve.ValidationErrors)
                        sb.AppendLine($" - Property: \"{ve.PropertyName}\", Error: \"{ve.ErrorMessage}\"");
                }
                MessageBox.Show(sb.ToString(), "Ошибка валидации", MessageBoxButton.OK, MessageBoxImage.Error);
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка при добавлении файла: {ex.Message}", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void FilesListBox_MouseDoubleClick(object sender, MouseButtonEventArgs e)
        {
            if (!(FilesListBox.SelectedItem is FileItem vm)) return;

            using (var ctx = new TaskManagementEntities1())
            {
                var dbFile = ctx.Files.FirstOrDefault(f => f.Id == vm.Id);
                if (dbFile == null) return;

                string temp = Path.Combine(Path.GetTempPath(), dbFile.FileName);
                File.WriteAllBytes(temp, dbFile.FileData);
                Process.Start(new ProcessStartInfo { FileName = temp, UseShellExecute = true });
            }
        }

        private void DeleteFileButton_Click(object sender, RoutedEventArgs e)
        {
            if (!(sender is Button btn && btn.DataContext is FileItem fileVm))
                return;

            try
            {
                using (var ctx = new TaskManagementEntities1())
                {
                    var dbFile = ctx.Files.FirstOrDefault(f => f.Id == fileVm.Id);
                    if (dbFile != null)
                    {
                        ctx.Files.Remove(dbFile);
                        ctx.SaveChanges();
                    }
                }

                LoadTaskFiles();
                MessageBox.Show("Файл успешно удалён!", "Успех", MessageBoxButton.OK, MessageBoxImage.Information);
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка при удалении файла: {ex.Message}", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void DeleteCommentButton_Click(object sender, RoutedEventArgs e)
        {
            if (!(sender is Button btn && btn.CommandParameter is int commentId))
                return;

            try
            {
                using (var ctx = new TaskManagementEntities1())
                {
                    var dbComm = ctx.Comment.FirstOrDefault(c => c.Id == commentId);
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
