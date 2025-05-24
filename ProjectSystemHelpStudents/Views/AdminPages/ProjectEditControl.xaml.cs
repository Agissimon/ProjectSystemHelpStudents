using System.Windows;
using System.Windows.Controls;

namespace ProjectSystemHelpStudents.Views.AdminPages
{
    /// <summary>
    /// Логика взаимодействия для ProjectEditControl.xaml
    /// </summary>
    public partial class ProjectEditControl : UserControl
    {
        private readonly Project _project;
        private readonly TaskManagementEntities1 _ctx;

        public ProjectEditControl(Project project, TaskManagementEntities1 ctx)
        {
            InitializeComponent();
            _project = project;
            _ctx = ctx;

            NameBox.Text = _project.Name;
            TeamBox.Text = _project.TeamId?.ToString() ?? "";
        }

        private void Save_Click(object sender, RoutedEventArgs e)
        {
            _project.Name = NameBox.Text.Trim();
            if (int.TryParse(TeamBox.Text, out int teamId))
                _project.TeamId = teamId;

            _ctx.SaveChanges();
            Window.GetWindow(this)?.Close();
        }
    }
}
