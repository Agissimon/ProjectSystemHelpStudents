using ProjectSystemHelpStudents.Helper;
using ProjectSystemHelpStudents.ViewModels;
using System;
using System.Collections.Generic;
using System.Data.Entity.Infrastructure;
using System.Linq;
using System.Windows;
using System.Windows.Controls;

namespace ProjectSystemHelpStudents
{
    public partial class AddTaskWindow : Window
    {
        private List<LabelViewModel> _labelViewModels;
        private DateTime? _preselectedDate;
        private int? _projectId;
        private int? _sectionId;
        private List<Users> _allUsers;

        public AddTaskWindow()
        {
            InitializeComponent();
            Loaded += AddTaskWindow_Loaded;

            dpEndDate.SelectedDateChanged += EndDateOrTime_Changed;
            tbEndTime.TextChanged += EndDateOrTime_Changed;

            LoadTags();
        }

        // Конструктор, задающий проект, секцию и предустановленную дату
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
                _allUsers = ctx.Users.OrderBy(u => u.Name).ToList();
            }
            cmbAssignedTo.ItemsSource = _allUsers;
            cmbAssignedTo.SelectedValue = UserSession.IdUser;
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
            if (string.IsNullOrWhiteSpace(txtTitle.Text))
            {
                MessageBox.Show("Введите название задачи.", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
                return;
            }
            if (!dpEndDate.SelectedDate.HasValue)
            {
                MessageBox.Show("Выберите дату завершения.", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
                return;
            }

            var endDate = dpEndDate.SelectedDate.Value.Date;
            TimeSpan endTs;
            if (!TimeSpan.TryParse(tbEndTime.Text, out endTs))
                endTs = new TimeSpan(12, 0, 0);
            var endDateTime = endDate.Add(endTs);

            DateTime? remindDateTime = null;
            if (dpRemindDate.SelectedDate.HasValue)
            {
                TimeSpan remTs;
                if (!TimeSpan.TryParse(tbRemindTime.Text, out remTs))
                    remTs = new TimeSpan(11, 0, 0);
                remindDateTime = dpRemindDate.SelectedDate.Value.Date.Add(remTs);
            }

            try
            {
                using (var ctx = new TaskManagementEntities1())
                {
                    int userId = UserSession.IdUser;
                    var newTask = new Task
                    {
                        Title = txtTitle.Text.Trim(),
                        Description = txtDescription.Text.Trim(),
                        EndDate = endDateTime,
                        ReminderDate = remindDateTime,
                        StatusId = ctx.Status.First(s => s.Name == "Не завершено").StatusId,
                        CreatorId = userId,
                        PriorityId = GetSelectedPriorityId(ctx),
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

        private int GetSelectedPriorityId(TaskManagementEntities1 ctx)
        {
            if (cmbPriority.SelectedItem is ComboBoxItem comboItem && comboItem.Content != null)
            {
                string name = comboItem.Content.ToString();
                var pr = ctx.Priority.FirstOrDefault(p => p.Name == name);
                if (pr != null) return pr.PriorityId;
            }
            return ctx.Priority.OrderBy(p => p.PriorityId).First().PriorityId;
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