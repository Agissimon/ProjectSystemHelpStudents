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

namespace ProjectSystemHelpStudents
{
    /// <summary>
    /// Логика взаимодействия для AddTaskWindow.xaml
    /// </summary>
    public partial class AddTaskWindow : Window
    {
        public AddTaskWindow()
        {
            InitializeComponent();
        }

        private void SaveTask_Click(object sender, RoutedEventArgs e)
        {
            // Считываем данные из полей
            string title = txtTitle.Text;
            string description = txtDescription.Text;
            DateTime? endDate = dpEndDate.SelectedDate;
            string category = (cmbCategory.SelectedItem as ComboBoxItem)?.Content.ToString();
            string priority = (cmbPriority.SelectedItem as ComboBoxItem)?.Content.ToString();

            // Проверка, что все обязательные поля заполнены
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
            int maxIdTask = DBClass.entities.Task.Max(t => (int?)t.IdTask) ?? 0;
            int maxIdTaskPrioritie = DBClass.entities.TaskPriorities.Max(t => (int?)t.IDTaskPriorities) ?? 0;
    
            var newTask = new Task
            {
                IdTask = maxIdTask + 1,
                Title = title,
                Description = description,
                EndDate = endDate.Value,
                StatusTask = "Не завершено",
                IdUser = UserSession.IdUser
            };
            TaskPriorities taskpriorities = new TaskPriorities
            {
                IDTaskPriorities = maxIdTaskPrioritie + 1,
                IdTask = newTask.IdTask,
                Category = category,
                Prioritie = priority,
                Label = title
            };

            try
            {
                DBClass.entities.TaskPriorities.Add(taskpriorities);
                DBClass.entities.Task.Add(newTask);
                DBClass.entities.SaveChanges();
                MessageBox.Show("Задача успешно добавлена.", "Успех", MessageBoxButton.OK, MessageBoxImage.Information);

                // Закрываем окно после сохранения
                this.Close();
            }
            catch (Exception ex)
            {
                MessageBox.Show("Ошибка при добавлении задачи: " + ex.Message, "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }
    }
}

