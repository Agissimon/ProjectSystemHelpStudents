using System;
using System.Linq;
using System.Windows;
using ProjectSystemHelpStudents.Helper;

namespace ProjectSystemHelpStudents.UsersContent
{
    public partial class AddProjectWindow : Window
    {
        private readonly TaskManagementEntities1 _ctx = new TaskManagementEntities1();
        public int? ProjectId { get; set; }

        public bool IsProjectAdded { get; private set; }
        public bool IsProjectUpdated { get; private set; }

        public event Action<Project> ProjectAdded;

        public string WindowTitle => ProjectId.HasValue ? "Редактировать проект" : "Добавить проект";
        public string AddButtonText => ProjectId.HasValue ? "Сохранить изменения" : "Добавить проект";

        public AddProjectWindow(int? projectId = null)
        {
            InitializeComponent();
            DataContext = this;
            ProjectId = projectId;
        }

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            int uid = UserSession.IdUser;

            var memberTeamIds = _ctx.TeamMember
                                    .Where(tm => tm.UserId == uid)
                                    .Select(tm => tm.TeamId)
                                    .ToList();

            var teams = _ctx.Team
                            .Where(t => t.LeaderId == uid 
                                     || memberTeamIds.Contains(t.TeamId)) 
                            .OrderBy(t => t.Name)
                            .ToList();

            teams.Insert(0, new Team { TeamId = 0, Name = "<Без команды>" });

            cmbTeams.ItemsSource = teams;
            cmbTeams.DisplayMemberPath = "Name";
            cmbTeams.SelectedValuePath = "TeamId";

            if (ProjectId.HasValue)
            {
                var project = _ctx.Project.Find(ProjectId.Value);
                if (project == null)
                {
                    MessageBox.Show("Проект не найден.", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
                    Close();
                    return;
                }

                NameTextBox.Text = project.Name;
                DescriptionTextBox.Text = project.Description;
                StartDatePicker.SelectedDate = project.StartDate;
                EndDatePicker.SelectedDate = project.EndDate;
                cmbTeams.SelectedValue = project.TeamId ?? 0;
            }
            else
            {
                cmbTeams.SelectedValue = 0;
            }
        }

        private void AddProjectButton_Click(object sender, RoutedEventArgs e)
        {
            string name = NameTextBox.Text.Trim();
            string description = DescriptionTextBox.Text.Trim();
            DateTime? start = StartDatePicker.SelectedDate;
            DateTime? end = EndDatePicker.SelectedDate;
            int teamId = (int)(cmbTeams.SelectedValue ?? 0);

            if (string.IsNullOrWhiteSpace(name))
            {
                MessageBox.Show("Введите название проекта.", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }
            if (start.HasValue && end.HasValue && start > end)
            {
                MessageBox.Show("Дата начала не может быть позже даты окончания.", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            try
            {
                if (ProjectId.HasValue)
                {
                    // обновление
                    var proj = _ctx.Project.Find(ProjectId.Value);
                    proj.Name = name;
                    proj.Description = description;
                    proj.StartDate = start ?? proj.StartDate;
                    proj.EndDate = end ?? proj.EndDate;
                    proj.TeamId = teamId == 0 ? (int?)null : teamId;

                    _ctx.SaveChanges();
                    IsProjectUpdated = true;
                    MessageBox.Show("Проект обновлён.", "Успех", MessageBoxButton.OK, MessageBoxImage.Information);
                }
                else
                {
                    // создание
                    var proj = new Project
                    {
                        Name = name,
                        Description = description,
                        StartDate = start ?? DateTime.Now,
                        EndDate = end ?? DateTime.Now.AddYears(1),
                        TeamId = teamId == 0 ? (int?)null : teamId,
                        OwnerId = UserSession.IdUser
                    };
                    _ctx.Project.Add(proj);
                    _ctx.SaveChanges();
                    IsProjectAdded = true;

                    ProjectAdded?.Invoke(proj);

                    MessageBox.Show("Проект добавлен.", "Успех", MessageBoxButton.OK, MessageBoxImage.Information);
                }

                Close();
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка при сохранении проекта:\n{ex.Message}", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }
    }
}
