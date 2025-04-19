using System;
using System.Linq;
using System.Windows;
using ProjectSystemHelpStudents.Helper;

namespace ProjectSystemHelpStudents.UsersContent
{
    public partial class AddProjectWindow : Window
    {
        public bool IsProjectAdded { get; private set; } = false;
        public event Action<Project> ProjectAdded;
        public int? ProjectId { get; set; }
        public bool IsProjectUpdated { get; private set; } = false;

        public AddProjectWindow()
        {
            InitializeComponent();
        }

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            if (ProjectId.HasValue)
            {
                using (var context = new TaskManagementEntities1())
                {
                    var project = context.Project.FirstOrDefault(p => p.ProjectId == ProjectId.Value);
                    if (project != null)
                    {
                        NameTextBox.Text = project.Name;
                        DescriptionTextBox.Text = project.Description;
                        StartDatePicker.SelectedDate = project.StartDate;
                        EndDatePicker.SelectedDate = project.EndDate;
                    }
                    else
                    {
                        MessageBox.Show("Проект не найден.", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
                    }
                }
            }
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
                    // Если это редактирование, то обновляем проект, иначе создаем новый
                    if (ProjectId.HasValue)
                    {
                        var existingProject = context.Project.FirstOrDefault(p => p.ProjectId == ProjectId.Value);
                        if (existingProject != null)
                        {
                            // Обновляем проект
                            existingProject.Name = name;
                            existingProject.Description = description;
                            existingProject.StartDate = startDate ?? DateTime.Now;
                            existingProject.EndDate = endDate ?? DateTime.Now.AddYears(1);

                            context.SaveChanges();
                            IsProjectUpdated = true;
                            MessageBox.Show("Проект успешно обновлен!", "Успех", MessageBoxButton.OK, MessageBoxImage.Information);
                        }
                        else
                        {
                            MessageBox.Show("Проект не найден для редактирования.", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
                        }
                    }
                    else
                    {
                        // Если это создание нового проекта
                        var newProject = new Project
                        {
                            Name = name,
                            Description = description,
                            StartDate = startDate ?? DateTime.Now,
                            EndDate = endDate ?? DateTime.Now.AddYears(1)
                        };

                        var currentUser = context.Users.FirstOrDefault(u => u.IdUser == UserSession.IdUser);
                        if (currentUser != null)
                        {
                            context.Project.Add(newProject);
                            context.SaveChanges();
                            IsProjectAdded = true;

                            // Уведомляем подписчиков о новом проекте
                            ProjectAdded?.Invoke(newProject);

                            MessageBox.Show("Проект успешно добавлен!", "Успех", MessageBoxButton.OK, MessageBoxImage.Information);
                        }
                        else
                        {
                            MessageBox.Show("Не удалось определить текущего пользователя.", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
                        }
                    }

                    this.Close();
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка при добавлении или обновлении проекта: {ex.Message}", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }
    }
}
