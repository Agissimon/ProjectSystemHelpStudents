using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Remoting.Contexts;
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
using ProjectSystemHelpStudents.Helper;
using ProjectSystemHelpStudents.UsersContent;
using static System.Collections.Specialized.BitVector32;

namespace ProjectSystemHelpStudents
{
    /// <summary>
    /// Логика взаимодействия для AddTaskWindow.xaml
    /// </summary>
    public partial class AddTaskWindow : Window
    {
        private List<LabelViewModel> _labelViewModels;
        private DateTime? _preselectedDate;
        private int? _projectId;
        private int? _sectionId;

        public AddTaskWindow()
        {
            InitializeComponent();
            LoadTags(); // Загрузить метки из БД
        }

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            LoadTags();
        }

        private void LoadTags()
        {
            _labelViewModels = DBClass.entities.Labels
                .Select(l => new LabelViewModel
                {
                    Id = l.Id,
                    Name = l.Name,
                    IsSelected = false
                }).ToList();

            lstTags.ItemsSource = _labelViewModels;
        }

        public AddTaskWindow(int? projectId = null, int? sectionId = null) : this()
        {
            _projectId = projectId;
            _sectionId = sectionId;
        }

        public AddTaskWindow(DateTime preselectedDate) : this()
        {
            _preselectedDate = preselectedDate;
            dpEndDate.SelectedDate = preselectedDate;
        }

        public void SetPreselectedDate(DateTime date)
        {
            dpEndDate.SelectedDate = date;
        }

        private void SaveTask_Click(object sender, RoutedEventArgs e)
        {
            string title = txtTitle.Text;
            string description = txtDescription.Text;
            DateTime? endDate = dpEndDate.SelectedDate;
            string priorityName = (cmbPriority.SelectedItem as ComboBoxItem)?.Content.ToString();

            if (string.IsNullOrWhiteSpace(title))
            {
                MessageBox.Show("Пожалуйста, введите название задачи.", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
                return;
            }

            if (endDate == null)
            {
                MessageBox.Show("Пожалуйста, выберите дату завершения.", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
                return;
            }

            try
            {
                var priority = DBClass.entities.Priority.FirstOrDefault(p => p.Name == priorityName);
                var sectionExists = _sectionId.HasValue && DBClass.entities.Section.Any(s => s.IdSection == _sectionId.Value);
                var projectExists = _projectId.HasValue && DBClass.entities.Project.Any(p => p.ProjectId == _projectId.Value);

                var newTask = new Task
                {
                    Title = title,
                    Description = description,
                    EndDate = endDate.Value,
                    StatusId = DBClass.entities.Status.FirstOrDefault(s => s.Name == "Не завершено")?.StatusId ?? 1,
                    IdUser = UserSession.IdUser,
                    PriorityId = (int)(priority?.PriorityId),
                    ProjectId = projectExists ? _projectId : 1,
                    SectionId = sectionExists ? _sectionId : null
                };
                DBClass.entities.Task.Add(newTask);
                DBClass.entities.SaveChanges(); // Сохраняем задачу и получаем Id

                var selectedLabels = _labelViewModels.Where(l => l.IsSelected).ToList();
                foreach (var label in selectedLabels)
                {
                    var taskLabel = new TaskLabels
                    {
                        TaskId = newTask.IdTask,
                        LabelId = label.Id
                    };
                    DBClass.entities.TaskLabels.Add(taskLabel);
                }
                DBClass.entities.SaveChanges(); // Сохраняем связи

                MessageBox.Show("Задача успешно добавлена.", "Успех", MessageBoxButton.OK, MessageBoxImage.Information);

                this.DialogResult = true;
                this.Close();
            }
            catch (Exception ex)
            {
                MessageBox.Show("Ошибка при добавлении задачи: " + ex.Message, "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }
        private void Cancel_Click(object sender, RoutedEventArgs e)
        {
            this.DialogResult = false;
            this.Close();
        }
    }
}
