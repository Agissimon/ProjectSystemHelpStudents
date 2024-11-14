using ProjectSystemHelpStudents.Helper;
using System;
using System.Linq;
using System.Windows;
using System.Windows.Controls;

namespace ProjectSystemHelpStudents
{
    public partial class TaskDetailsWindow : Window
    {
        private TaskViewModel task;

        public TaskDetailsWindow(TaskViewModel task)
        {
            InitializeComponent();
            this.task = task;
            DataContext = task;

            InitializeFields();
        }

        private void InitializeFields()
        {
            // Заполняем данные для ProjectComboBox и PriorityComboBox
            ProjectComboBox.ItemsSource = DBClass.entities.Project.ToList();
            PriorityComboBox.ItemsSource = DBClass.entities.Priority.ToList();

            // Отображаем Name поля и привязываем идентификаторы
            ProjectComboBox.DisplayMemberPath = "Name";
            ProjectComboBox.SelectedValuePath = "ProjectId";
            ProjectComboBox.SelectedValue = task.ProjectId;

            PriorityComboBox.DisplayMemberPath = "Name";
            PriorityComboBox.SelectedValuePath = "PriorityId";
            PriorityComboBox.SelectedValue = task.PriorityId;
        }

        private void SaveButton_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                // Обновляем свойства task на основе введенных данных
                task.Title = TitleTextBox.Text;
                task.Description = DescriptionTextBox.Text;
                task.EndDate = (DateTime)EndDatePicker.SelectedDate;

                // Сохраняем выбор ProjectId и PriorityId
                task.ProjectId = (int)(ProjectComboBox.SelectedValue ?? task.ProjectId);
                task.PriorityId = (int)(PriorityComboBox.SelectedValue ?? task.PriorityId);

                // Обновление задачи в базе данных
                var dbTask = DBClass.entities.Task.FirstOrDefault(t => t.IdTask == task.IdTask);
                if (dbTask != null)
                {
                    dbTask.Title = task.Title;
                    dbTask.Description = task.Description;
                    dbTask.EndDate = task.EndDate;
                    dbTask.PriorityId = task.PriorityId;
                    dbTask.ProjectId = task.ProjectId;

                    DBClass.entities.SaveChanges();
                }

                MessageBox.Show("Задача успешно сохранена.");
                this.Close();
            }
            catch (Exception ex)
            {
                MessageBox.Show("Ошибка при сохранении задачи: " + ex.Message);
            }
        }

        private void ToggleTaskStatus_Click(object sender, RoutedEventArgs e)
        {
            var checkBox = sender as CheckBox;
            var task = checkBox.DataContext as TaskViewModel;

            if (task != null)
            {
                var dbTask = DBClass.entities.Task.FirstOrDefault(t => t.Title == task.Title);
                if (dbTask != null)
                {
                    dbTask.StatusId = (int)(checkBox.IsChecked == true ? DBClass.entities.Status.FirstOrDefault(s => s.Name == "Завершено")?.StatusId : DBClass.entities.Status.FirstOrDefault(s => s.Name == "Не завершено")?.StatusId);
                    DBClass.entities.SaveChanges();
                }
            }
        }

        private void Button_Click(object sender, RoutedEventArgs e)
        {
            MessageBox.Show("Эта функция пока не работает");
        }

    }
}
