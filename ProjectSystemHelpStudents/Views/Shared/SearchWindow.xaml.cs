using ProjectSystemHelpStudents.Helper;
using ProjectSystemHelpStudents.ViewModels;
using System.Linq;
using System.Windows;
using System.Windows.Controls;

namespace ProjectSystemHelpStudents
{
    public partial class SearchWindow : Window
    {
        private SearchViewModel Vm => DataContext as SearchViewModel;

        public SearchWindow()
        {
            InitializeComponent();
        }

        private void HistoryListBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (e.AddedItems.Count > 0)
                Vm.HistorySelectedCommand.Execute(e.AddedItems[0]);
        }

        private void DeleteHistory_Click(object sender, RoutedEventArgs e)
        {
            if (((FrameworkElement)sender).DataContext is string item)
                Vm.RemoveHistoryItem(item);
        }

        private void ClearHistory_Click(object sender, RoutedEventArgs e)
        {
            Vm.ClearHistory();
        }

        private void ResultsListBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (!(lstResults.SelectedItem is string selectedItem) || !selectedItem.StartsWith("Задача:"))
                return;

            int idStart = selectedItem.LastIndexOf("(ID:") + 4;
            int idEnd = selectedItem.LastIndexOf(")");
            if (idStart < 0 || idEnd <= idStart)
                return;

            string idStr = selectedItem.Substring(idStart, idEnd - idStart);
            if (!int.TryParse(idStr, out int taskId))
                return;

            var dbTask = DBClass.entities.Task.FirstOrDefault(t => t.IdTask == taskId);
            if (dbTask == null)
                return;

            var taskViewModel = new TaskViewModel
            {
                IdTask = dbTask.IdTask,
                Title = dbTask.Title,
                Description = dbTask.Description,
                EndDate = dbTask.EndDate,
                ProjectId = (int)dbTask.ProjectId,
                PriorityId = dbTask.PriorityId,
                Status = dbTask.Status.Name
            };

            var taskDetailsWindow = new TaskDetailsWindow(taskViewModel);
            taskDetailsWindow.TaskUpdated += () => { };
            taskDetailsWindow.ShowDialog();
        }
    }
}