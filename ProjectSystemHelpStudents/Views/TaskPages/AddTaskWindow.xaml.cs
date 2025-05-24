using ProjectSystemHelpStudents.Helper;
using ProjectSystemHelpStudents.ViewModels;
using System;
using System.Collections.Generic;
using System.Data.Entity.Infrastructure;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Data.Entity;

namespace ProjectSystemHelpStudents
{
    public partial class AddTaskWindow : Window
    {
        private List<LabelViewModel> _labelViewModels;
        private DateTime? _preselectedDate;
        private int? _projectId;
        private int? _sectionId;

        public AddTaskWindow()
        {
            InitializeComponent();
            Loaded += AddTaskWindow_Loaded;

            dpEndDate.SelectedDateChanged += EndDateOrTime_Changed;
            tbEndTime.TextChanged += EndDateOrTime_Changed;

            LoadTags();
        }

        public AddTaskWindow(int? projectId = null, int? sectionId = null, DateTime? preselectedDate = null)
            : this()
        {
            _projectId = projectId;
            _sectionId = sectionId;
            _preselectedDate = preselectedDate;
        }

        public AddTaskWindow(DateTime preselectedDate)
            : this(null, null, preselectedDate)
        {
        }

        private void AddTaskWindow_Loaded(object sender, RoutedEventArgs e)
        {
            if (_preselectedDate.HasValue)
                dpEndDate.SelectedDate = _preselectedDate.Value;

            SetDefaultRemind();

            using (var ctx = new TaskManagementEntities1())
            {
                var priorities = ctx.Priority
                                    .OrderBy(p => p.Name)
                                    .ToList();
                cmbPriority.ItemsSource = priorities;
                cmbPriority.SelectedValue = priorities.FirstOrDefault()?.PriorityId;

                int me = UserSession.IdUser;

                var myTeamIds = ctx.Team
                                   .Where(t => t.LeaderId == me)
                                   .Select(t => t.TeamId)
                                   .Union(
                                     ctx.TeamMember
                                        .Where(tm => tm.UserId == me)
                                        .Select(tm => tm.TeamId)
                                   )
                                   .Distinct()
                                   .ToList();

                if (!myTeamIds.Any())
                {
                    // Если ни в одной команде — оставляем только пользователя
                    cmbAssignedTo.ItemsSource = new[] { ctx.Users.Find(me) };
                    cmbAssignedTo.SelectedValue = me;
                    return;
                }

                var userIds = ctx.TeamMember
                                 .Where(tm => myTeamIds.Contains(tm.TeamId))
                                 .Select(tm => tm.UserId)
                                 .Union(
                                   ctx.Team
                                      .Where(t => myTeamIds.Contains(t.TeamId))
                                      .Select(t => t.LeaderId)
                                 )
                                 .Distinct()
                                 .ToList();

                var executors = ctx.Users
                                   .Where(u => userIds.Contains(u.IdUser))
                                   .OrderBy(u => u.Name)
                                   .ToList();

                cmbAssignedTo.ItemsSource = executors;
                cmbAssignedTo.SelectedValue = me;
            }
        }

        private void EndDateOrTime_Changed(object sender, EventArgs e)
        {
            SetDefaultRemind();
        }

        // Устанавливает поля "Напомнить" за час до окончания
        private void SetDefaultRemind()
        {
            if (!dpEndDate.SelectedDate.HasValue)
                return;

            TimeSpan endTs;
            if (!TimeSpan.TryParse(tbEndTime.Text, out endTs))
                endTs = new TimeSpan(12, 0, 0);

            var endDateTime = dpEndDate.SelectedDate.Value.Date.Add(endTs);
            var remindDateTime = endDateTime.AddHours(-1);

            dpRemindDate.SelectedDate = remindDateTime.Date;
            tbRemindTime.Text = remindDateTime.ToString("HH:mm");
        }

        // Внешний вызов для предустановки даты до открытия окна
        public void SetPreselectedDate(DateTime date)
        {
            _preselectedDate = date;
            if (IsLoaded)
            {
                dpEndDate.SelectedDate = date;
                SetDefaultRemind();
            }
        }

        private void LoadTags()
        {
            _labelViewModels = DBClass.entities.Labels
                .Where(l => l.UserId == UserSession.IdUser)
                .Select(l => new LabelViewModel
                {
                    Id = l.Id,
                    Name = l.Name,
                    HexColor = l.Color,
                    IsSelected = false
                })
                .ToList();

            lstTags.ItemsSource = _labelViewModels;
        }

        private void SaveTask_Click(object sender, RoutedEventArgs e)
        {
            var title = txtTitle.Text.Trim();
            if (string.IsNullOrEmpty(title))
            {
                MessageBox.Show("Введите название задачи.", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Warning);
                txtTitle.Focus();
                return;
            }

            if (!dpEndDate.SelectedDate.HasValue)
            {
                MessageBox.Show("Выберите дату завершения.", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Warning);
                dpEndDate.Focus();
                return;
            }

            if (!TimeSpan.TryParse(tbEndTime.Text.Trim(), out var endTs))
            {
                MessageBox.Show("Время окончания некорректно. Используйте формат ЧЧ:ММ.", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Warning);
                tbEndTime.Focus();
                return;
            }

            var endDateTime = dpEndDate.SelectedDate.Value.Date.Add(endTs);

            DateTime? remindDateTime = null;
            if (dpRemindDate.SelectedDate.HasValue || !string.IsNullOrEmpty(tbRemindTime.Text.Trim()))
            {
                if (!dpRemindDate.SelectedDate.HasValue)
                {
                    MessageBox.Show("Укажите дату напоминания или очистите поля.", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Warning);
                    dpRemindDate.Focus();
                    return;
                }
                if (!TimeSpan.TryParse(tbRemindTime.Text.Trim(), out var remTs))
                {
                    MessageBox.Show("Время напоминания некорректно. Используйте формат ЧЧ:ММ.", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Warning);
                    tbRemindTime.Focus();
                    return;
                }
                remindDateTime = dpRemindDate.SelectedDate.Value.Date.Add(remTs);

                if (remindDateTime >= endDateTime)
                {
                    MessageBox.Show("Время напоминания должно быть раньше времени окончания задачи.", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Warning);
                    dpRemindDate.Focus();
                    return;
                }
            }

            if (cmbPriority.SelectedItem == null)
            {
                MessageBox.Show("Выберите приоритет задачи.", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Warning);
                cmbPriority.Focus();
                return;
            }

            if (cmbAssignedTo.SelectedItem == null)
            {
                MessageBox.Show("Выберите исполнителя задачи.", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Warning);
                cmbAssignedTo.Focus();
                return;
            }

            try
            {
                using (var ctx = new TaskManagementEntities1())
                {
                    int userId = UserSession.IdUser;
                    var newTask = new Task
                    {
                        Title = title,
                        Description = txtDescription.Text.Trim(),
                        EndDate = endDateTime,
                        ReminderDate = remindDateTime,
                        StatusId = ctx.Status.First(s => s.Name == "Не завершено").StatusId,
                        CreatorId = userId,
                        PriorityId = (int)cmbPriority.SelectedValue,
                        ProjectId = GetProjectId(ctx, userId),
                        SectionId = _sectionId
                    };
                    ctx.Task.Add(newTask);
                    ctx.SaveChanges();

                    foreach (var lbl in _labelViewModels.Where(l => l.IsSelected))
                    {
                        ctx.TaskLabels.Add(new TaskLabels
                        {
                            TaskId = newTask.IdTask,
                            LabelId = lbl.Id
                        });
                    }
                    ctx.SaveChanges();
                }

                MessageBox.Show("Задача успешно добавлена.", "Успех", MessageBoxButton.OK, MessageBoxImage.Information);
                DialogResult = true;
                Close();
            }
            catch (DbUpdateException dbEx)
            {
                var msg = dbEx.InnerException?.InnerException?.Message ?? dbEx.Message;
                MessageBox.Show("Ошибка при сохранении в БД:\n" + msg, "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
            }
            catch (Exception ex)
            {
                MessageBox.Show("Ошибка: " + ex.Message, "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private int GetProjectId(TaskManagementEntities1 ctx, int userId)
        {
            var inbox = ctx.Project.FirstOrDefault(p => p.Name == "Входящие" && p.OwnerId == userId)
                        ?? new Project { Name = "Входящие", OwnerId = userId };
            if (inbox.ProjectId == 0)
            {
                ctx.Project.Add(inbox);
                ctx.SaveChanges();
            }
            return _projectId.HasValue
                ? (ctx.Project.Find(_projectId.Value)?.ProjectId ?? inbox.ProjectId)
                : inbox.ProjectId;
        }

        private void Cancel_Click(object sender, RoutedEventArgs e)
        {
            DialogResult = false;
            Close();
        }
    }
}