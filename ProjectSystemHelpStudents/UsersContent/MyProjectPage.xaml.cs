using System;
using System.Collections.ObjectModel;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using ProjectSystemHelpStudents.Helper;

namespace ProjectSystemHelpStudents.UsersContent
{
    public partial class MyProjectPage : Page
    {
        public ObservableCollection<ProjectViewModel> Projects { get; set; }
        public string ProjectCountText => $"{Projects.Count} проекта";

        public MyProjectPage()
        {
            InitializeComponent();
            Projects = new ObservableCollection<ProjectViewModel>();
            DataContext = this;

            RefreshProjects();
        }
        private void RefreshProjects()
        {
            try
            {
                using (var context = new TaskManagementEntities1())
                {
                    //int currentUserId = UserSession.IdUser;

                    var userProjects = context.Project
                        .Select(p => new
                        {
                            p.ProjectId,
                            p.Name,
                            p.Description,
                            p.StartDate,
                            p.EndDate
                        })
                        .ToList();

                    Projects.Clear();
                    foreach (var project in userProjects)
                    {
                        Projects.Add(new ProjectViewModel
                        {
                            Name = project.Name,
                            Icon = "📁"
                        });
                    }
                }

                ProjectsListView.ItemsSource = Projects;
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка при загрузке проектов: {ex.Message}", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void AddProject_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                AddProjectWindow addProjectWindow = new AddProjectWindow();
                addProjectWindow.ShowDialog();

                if (addProjectWindow.IsProjectAdded)
                {
                    RefreshProjects();
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка при добавлении проекта: {ex.Message}", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }


        private void ProjectOptions_Click(object sender, RoutedEventArgs e)
        {
            MessageBox.Show("Опции проекта");
        }
    }
}
