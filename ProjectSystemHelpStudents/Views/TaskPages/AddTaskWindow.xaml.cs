using ProjectSystemHelpStudents.Helper;
using System;
using System.Collections.Generic;
using System.Data.Entity;   // EF6
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
        }

        private void LoadTags()
        {
            _labelViewModels = DBClass.entities.Labels
                .Select(l => new LabelViewModel { Id = l.Id, Name = l.Name, IsSelected = false })
                .ToList();
            lstTags.ItemsSource = _labelViewModels;
        }

        public void SetPreselectedDate(DateTime date)
        {
            dpEndDate.SelectedDate = date;
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

            try
            {
                using (var ctx = new TaskManagementEntities1())
                {
                    int projectIdToUse;
                    if (_projectId.HasValue && ctx.Project.Any(p => p.ProjectId == _projectId.Value))
                        projectIdToUse = _projectId.Value;
                    else
                    {
                        var inbox = ctx.Project.FirstOrDefault(p => p.Name == "Входящие");
                        if (inbox == null)
                            throw new InvalidOperationException("Не найден проект 'Входящие'");
                        projectIdToUse = inbox.ProjectId;
                    }

                    var priorityName = (cmbPriority.SelectedItem as ComboBoxItem)?.Content?.ToString();
                    var pr = ctx.Priority.FirstOrDefault(p => p.Name == priorityName);
                    int priorityIdToUse = pr?.PriorityId
                                          ?? ctx.Priority.OrderBy(p => p.PriorityId).First().PriorityId;

                    var undone = ctx.Status.FirstOrDefault(s => s.Name == "Не завершено")
                                ?? throw new InvalidOperationException("Не найден статус 'Не завершено'");

                    int? sectionIdToUse = (_sectionId.HasValue && ctx.Section.Any(s => s.IdSection == _sectionId.Value))
                                          ? _sectionId.Value
                                          : (int?)null;

                    var newTask = new ProjectSystemHelpStudents.Task
                    {
                        Title = txtTitle.Text.Trim(),
                        Description = txtDescription.Text.Trim(),
                        EndDate = dpEndDate.SelectedDate.Value,
                        StatusId = undone.StatusId,
                        IdUser = UserSession.IdUser,
                        PriorityId = priorityIdToUse,
                        ProjectId = projectIdToUse,
                        SectionId = sectionIdToUse
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

        private void Cancel_Click(object sender, RoutedEventArgs e)
        {
            DialogResult = false;
            Close();
        }
    }
}
