using System;
using System.Linq;
using System.Windows;
using ProjectSystemHelpStudents.Helper;

namespace ProjectSystemHelpStudents.UsersContent
{
    public partial class AddProjectWindow : Window
    {
        public bool IsProjectAdded { get; private set; } = false;

        public AddProjectWindow()
        {
            InitializeComponent();
        }

        private void AddProjectButton_Click(object sender, RoutedEventArgs e)
        {
            string name = NameTextBox.Text;
            string description = DescriptionTextBox.Text;
            DateTime? startDate = StartDatePicker.SelectedDate;
            DateTime? endDate = EndDatePicker.SelectedDate;

            if (string.IsNullOrWhiteSpace(name))
            {
                MessageBox.Show("Название проекта не может быть пустым.", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }
            if (startDate > endDate)
            {
                MessageBox.Show("Дата начала не может быть позже даты окончания.", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            try
            {
                using (var context = new TaskManagementEntities1())
                {
                    var newProject = new Project
                    {
                        Name = name,
                        //Description = description,
                        //StartDate = startDate.Value,
                        //EndDate = endDate.Value
                    };

                    var currentUser = context.Users.FirstOrDefault(u => u.IdUser == UserSession.IdUser);
                    if (currentUser != null)
                    {
                        context.Project.Add(newProject);
                        context.SaveChanges();

                        IsProjectAdded = true;

                        MessageBox.Show("Проект успешно добавлен!", "Успех", MessageBoxButton.OK, MessageBoxImage.Information);
                        this.Close();
                        
                    }
                    else
                    {
                        MessageBox.Show("Не удалось определить текущего пользователя.", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
                    }
                    
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка при добавлении проекта: {ex.Message}", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }
    }
}
