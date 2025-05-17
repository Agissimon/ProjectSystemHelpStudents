using ProjectSystemHelpStudents.Helper;
using ProjectSystemHelpStudents.ViewModels;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Data.Entity;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace ProjectSystemHelpStudents.Views.TaskPages
{
    /// <summary>
    /// Логика взаимодействия для FilteredTasksPage.xaml
    /// </summary>
    public partial class FilteredTasksPage : Page
    {
        private readonly Filters _filter;

        public FilteredTasksPage(Filters filter)
        {
            InitializeComponent();
            _filter = filter;
            HeaderText.Text = $"Фильтр: {_filter.Name}";
            LoadFilteredTasks();
        }

        private void LoadFilteredTasks()
        {
            using (var ctx = new TaskManagementEntities1())
            {
                // только собственные задачи
                var baseQuery = ctx.Task
                    .Include(t => t.Status)
                    .Include(t => t.TaskAssignee)
                    .Include(t => t.TaskLabels.Select(tl => tl.Labels))
                    .Where(t =>
                        t.CreatorId == UserSession.IdUser ||
                        t.TaskAssignee.Any(ta => ta.UserId == UserSession.IdUser));

                var filtered = baseQuery
                    .Apply(_filter.Query)
                    .ToList();

                var vms = filtered.Select(t => new TaskViewModel
                {
                    IdTask = t.IdTask,
                    Title = t.Title,
                    Description = t.Description,
                    EndDate = t.EndDate,
                    AvailableLabels = new ObservableCollection<LabelViewModel>(
                        t.TaskLabels.Select(l => new LabelViewModel
                        {
                            Id = l.Labels.Id,
                            Name = l.Labels.Name,
                            HexColor = l.Labels.Color
                        }))
                }).ToList();

                TasksList.ItemsSource = vms;
            }
        }

        private void TasksList_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (TasksList.SelectedItem is TaskViewModel vm)
            {
                var wnd = new ProjectSystemHelpStudents.TaskDetailsWindow(vm);
                wnd.TaskUpdated += () => LoadFilteredTasks();
                wnd.ShowDialog();
                TasksList.SelectedItem = null;
            }
        }
    }
}
