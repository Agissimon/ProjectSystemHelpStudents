using ProjectSystemHelpStudents.Helper;
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

            using (var ctx = new TaskManagementEntities1())
            {
                _allUsers = ctx.Users
                               .OrderBy(u => u.Name)
                               .ToList();
            }
            cmbAssignedTo.ItemsSource = _allUsers;
            cmbAssignedTo.SelectedValue = UserSession.IdUser;
        }

        /// <summary>
        /// Позволяет программно установить дату завершения до открытия окна
        /// </summary>
        public void SetPreselectedDate(DateTime date)
        {
            _preselectedDate = date;
            if (IsLoaded)
                dpEndDate.SelectedDate = date;
        }

        private void LoadTags()
        {
            _labelViewModels = DBClass.entities.Labels
                .Select(l => new LabelViewModel { Id = l.Id, Name = l.Name, IsSelected = false })
                .ToList();
            lstTags.ItemsSource = _labelViewModels;
        }

        private void SaveTask_Click(object sender, RoutedEventArgs e)
        {
            // 1. Валидация
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

            DateTime endDate = dpEndDate.SelectedDate.Value.Date;
            TimeSpan endTs;
            if (!TimeSpan.TryParse(tbEndTime.Text, out endTs))
                endTs = new TimeSpan(12, 0, 0);
            DateTime endDateTime = endDate.Add(endTs);

            DateTime? remindDateTime = null;
            if (dpRemindDate.SelectedDate.HasValue)
            {
                TimeSpan remTs;
                if (TimeSpan.TryParse(tbRemindTime.Text, out remTs))
                    remindDateTime = dpRemindDate.SelectedDate.Value.Date.Add(remTs);
            }

            try
            {
                using (var ctx = new TaskManagementEntities1())
                {
                    int userId = UserSession.IdUser;

                    Project inbox = ctx.Project
                        .FirstOrDefault(p => p.Name == "Входящие" && p.OwnerId == userId);
                    if (inbox == null)
                    {
                        inbox = new Project { Name = "Входящие", OwnerId = userId };
                        ctx.Project.Add(inbox);
                        ctx.SaveChanges();
                    }
                    int inboxId = inbox.ProjectId;

                    int projectIdToUse;
                    if (_projectId.HasValue)
                    {
                        Project proj = ctx.Project
                            .FirstOrDefault(p => p.ProjectId == _projectId.Value && p.OwnerId == userId);
                        if (proj != null)
                            projectIdToUse = proj.ProjectId;
                        else
                            projectIdToUse = inboxId;
                    }
                    else
                    {
                        projectIdToUse = inboxId;
                    }

                    Status undoneStatus = ctx.Status
                        .FirstOrDefault(s => s.Name == "Не завершено");
                    if (undoneStatus == null)
                        throw new InvalidOperationException("Не найден статус 'Не завершено'");

                    int priorityIdToUse;
                    if (cmbPriority.SelectedItem is ComboBoxItem comboItem
                        && comboItem.Content != null)
                    {
                        string priorityName = comboItem.Content.ToString();
                        Priority pr = ctx.Priority
                            .FirstOrDefault(p => p.Name == priorityName);
                        if (pr != null)
                            priorityIdToUse = pr.PriorityId;
                        else
                            priorityIdToUse = ctx.Priority.OrderBy(p => p.PriorityId).First().PriorityId;
                    }
                    else
                    {
                        priorityIdToUse = ctx.Priority.OrderBy(p => p.PriorityId).First().PriorityId;
                    }

                    int? sectionIdToUse = null;
                    if (_sectionId.HasValue)
                    {
                        Section sec = ctx.Section
                            .FirstOrDefault(s => s.IdSection == _sectionId.Value);
                        if (sec != null)
                            sectionIdToUse = sec.IdSection;
                    }

                    Task newTask = new Task
                    {
                        Title = txtTitle.Text.Trim(),
                        Description = txtDescription.Text.Trim(),
                        EndDate = endDateTime,
                        ReminderDate = remindDateTime,
                        StatusId = undoneStatus.StatusId,
                        CreatorId = userId,
                        PriorityId = priorityIdToUse,
                        ProjectId = projectIdToUse,
                        SectionId = sectionIdToUse
                    };
                    ctx.Task.Add(newTask);
                    ctx.SaveChanges();

                    foreach (var lbl in _labelViewModels)
                    {
                        if (lbl.IsSelected)
                        {
                            ctx.TaskLabels.Add(new TaskLabels
                            {
                                TaskId = newTask.IdTask,
                                LabelId = lbl.Id
                            });
                        }
                    }
                    ctx.SaveChanges();
                }

                MessageBox.Show("Задача успешно добавлена.", "Успех", MessageBoxButton.OK, MessageBoxImage.Information);
                this.DialogResult = true;
                this.Close();
            }
            catch (DbUpdateException dbEx)
            {
                string msg = dbEx.InnerException != null && dbEx.InnerException.InnerException != null
                    ? dbEx.InnerException.InnerException.Message
                    : dbEx.Message;
                MessageBox.Show("Ошибка при сохранении в БД:\n" + msg, "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
            }
            catch (Exception ex)
            {
                MessageBox.Show("Ошибка: " + ex.Message, "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void Cancel_Click(object sender, RoutedEventArgs e)
        {
            DialogResult = false;
            Close();
        }
    }
}
