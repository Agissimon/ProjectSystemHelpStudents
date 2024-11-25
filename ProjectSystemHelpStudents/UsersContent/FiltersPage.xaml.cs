using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;

namespace ProjectSystemHelpStudents.UsersContent
{
    public partial class FiltersPage : Page
    {
        public FiltersPage()
        {
            InitializeComponent();
            LoadFilters();
            LoadLabels();
        }

        private void LoadFilters()
        {
            using (var context = new TaskManagementEntities1())
            {
                var filters = context.Filters.ToList();
                FiltersList.ItemsSource = filters;
            }
        }

        private void LoadLabels()
        {
            using (var context = new TaskManagementEntities1())
            {
                var labels = context.Labels.ToList();
                LabelsList.ItemsSource = labels;
            }
        }

        private void AddFilter_Click(object sender, RoutedEventArgs e)
        {
            var newFilterName = PromptForInput("Введите название фильтра:");
            if (!string.IsNullOrEmpty(newFilterName))
            {
                using (var context = new TaskManagementEntities1())
                {
                    var newFilter = new Filters { Name = newFilterName };
                    context.Filters.Add(newFilter);
                    context.SaveChanges();
                    LoadFilters();
                }
            }
        }

        private void AddLabel_Click(object sender, RoutedEventArgs e)
        {
            var newLabelName = PromptForInput("Введите название метки:");
            var newLabelColor = PromptForInput("Введите цвет метки (например, #FF5733):");

            if (!string.IsNullOrEmpty(newLabelName) && !string.IsNullOrEmpty(newLabelColor))
            {
                using (var context = new TaskManagementEntities1())
                {
                    var newLabel = new Labels { Name = newLabelName, Color = newLabelColor };
                    context.Labels.Add(newLabel);
                    context.SaveChanges();
                    LoadLabels();
                }
            }
        }

        private void DeleteFilter_Clik(object sender, RoutedEventArgs e)
        {
            var button = sender as Button;
            if (button != null && button.CommandParameter != null)
            {
                var filterId = (int)button.CommandParameter;
                using (var context = new TaskManagementEntities1())
                {
                    var filter = context.Filters.FirstOrDefault(f => f.Id == filterId);
                    if (filter != null)
                    {
                        context.Filters.Remove(filter);
                        context.SaveChanges();
                        LoadFilters();
                    }
                }
            }
        }

        private void DeleteLabel_Clik(object sender, RoutedEventArgs e)
        {
            var button = sender as Button;
            if (button != null && button.CommandParameter != null)
            {
                var labelId = (int)button.CommandParameter;
                using (var context = new TaskManagementEntities1())
                {
                    var label = context.Labels.FirstOrDefault(l => l.Id == labelId);
                    if (label != null)
                    {
                        context.Labels.Remove(label);
                        context.SaveChanges();
                        LoadLabels();
                    }
                }
            }
        }

        private string PromptForInput(string message)
        {
            var inputDialog = new InputDialog(message);
            return inputDialog.ShowDialog() == true ? inputDialog.InputText : string.Empty;
        }
    }
}
