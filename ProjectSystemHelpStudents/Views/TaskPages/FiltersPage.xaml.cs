using ProjectSystemHelpStudents.Helper;
using ProjectSystemHelpStudents.Views.TaskPages;
using ProjectSystemHelpStudents.Views.UserPages;
using System;
using System.Linq;
using System.Windows;
using System.Windows.Controls;

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
            try
            {
                int currentUser = UserSession.IdUser;
                using (var context = new TaskManagementEntities1())
                {
                    var filters = context.Filters
                                         .Where(f => f.UserId == currentUser)
                                         .ToList();

                    FiltersList.ItemsSource = null;
                    FiltersList.ItemsSource = filters;
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show(
                    $"Не удалось загрузить список фильтров:\n{ex.Message}",
                    "Ошибка загрузки",
                    MessageBoxButton.OK,
                    MessageBoxImage.Error);
            }
        }

        private void LoadLabels()
        {
            try
            {
                int currentUser = UserSession.IdUser;
                using (var ctx = new TaskManagementEntities1())
                {
                    var labels = ctx.Labels
                                    .Where(l => l.UserId == currentUser)
                                    .ToList();

                    LabelsList.ItemsSource = null;
                    LabelsList.ItemsSource = labels;
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show(
                    $"Не удалось загрузить список меток:\n{ex.Message}",
                    "Ошибка загрузки",
                    MessageBoxButton.OK,
                    MessageBoxImage.Error);
            }
        }

        private void FiltersList_MouseDoubleClick(object sender, System.Windows.Input.MouseButtonEventArgs e)
        {
            if (FiltersList.SelectedItem is Filters filter)
            {
                var page = new FilteredTasksPage(filter);
                FrmClass.frmContentUser.Content = page;
                FrmClass.frmStackPanelButton.Content = new StackPanelButtonPage();
            }
        }

        private void AddFilter_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                var window = new LabelOrFilterWindow(isFilter: true);
                if (window.ShowDialog() != true) return;

                if (window.InputName.Contains(" "))
                {
                    MessageBox.Show("Название фильтра не может содержать пробел.", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Warning);
                    return;
                }

                int currentUser = UserSession.IdUser;
                using (var context = new TaskManagementEntities1())
                {
                    if (context.Filters.Any(f => f.Name == window.InputName && f.UserId == currentUser))
                    {
                        MessageBox.Show("Фильтр с таким названием уже существует.", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Warning);
                        return;
                    }

                    var filter = new Filters
                    {
                        Name = window.InputName,
                        Query = window.InputQuery,
                        Color = window.InputColor,
                        UserId = currentUser
                    };

                    context.Filters.Add(filter);
                    context.SaveChanges();
                }

                LoadFilters();
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка при добавлении фильтра:\n{ex.Message}", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void AddLabel_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                var window = new LabelOrFilterWindow(isFilter: false);
                if (window.ShowDialog() != true) return;

                int currentUser = UserSession.IdUser;
                using (var ctx = new TaskManagementEntities1())
                {
                    if (ctx.Labels.Any(l => l.Name == window.InputName && l.UserId == currentUser))
                    {
                        MessageBox.Show("Метка с таким именем уже существует.", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Warning);
                        return;
                    }

                    var label = new Labels
                    {
                        Name = window.InputName,
                        Color = window.InputColor,
                        UserId = currentUser
                    };
                    ctx.Labels.Add(label);
                    ctx.SaveChanges();
                }
                LoadLabels();
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка при добавлении метки:\n{ex.Message}", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void DeleteFilter_Clik(object sender, RoutedEventArgs e)
        {
            if (!(sender is Button btn) || !(btn.CommandParameter is int filterId)) return;

            try
            {
                using (var ctx = new TaskManagementEntities1())
                {
                    var filter = ctx.Filters.FirstOrDefault(f => f.Id == filterId && f.UserId == UserSession.IdUser);
                    if (filter != null)
                    {
                        ctx.Filters.Remove(filter);
                        ctx.SaveChanges();
                    }
                }
                LoadFilters();
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка при удалении фильтра:\n{ex.Message}", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void DeleteLabel_Clik(object sender, RoutedEventArgs e)
        {
            if (!(sender is Button btn) || !(btn.CommandParameter is int labelId)) return;

            try
            {
                using (var ctx = new TaskManagementEntities1())
                {
                    var label = ctx.Labels.FirstOrDefault(l => l.Id == labelId && l.UserId == UserSession.IdUser);
                    if (label != null)
                    {
                        ctx.Labels.Remove(label);
                        ctx.SaveChanges();
                    }
                }
                LoadLabels();
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка при удалении метки:\n{ex.Message}", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void EditFilter_Click(object sender, RoutedEventArgs e)
        {
            if (!(sender is Button btn) || !(btn.CommandParameter is int filterId)) return;

            try
            {
                using (var context = new TaskManagementEntities1())
                {
                    var filter = context.Filters.FirstOrDefault(f => f.Id == filterId && f.UserId == UserSession.IdUser);
                    if (filter == null) return;

                    var window = new LabelOrFilterWindow(
                        isFilter: true,
                        initialName: filter.Name,
                        initialValue: filter.Query,
                        initialColor: filter.Color);
                    if (window.ShowDialog() != true) return;

                    filter.Name = window.InputName;
                    filter.Query = window.InputQuery;
                    filter.Color = window.InputColor;
                    context.SaveChanges();
                }
                LoadFilters();
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка при редактировании фильтра:\n{ex.Message}", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void EditLabel_Click(object sender, RoutedEventArgs e)
        {
            if (!(sender is Button btn) || !(btn.CommandParameter is int labelId)) return;

            try
            {
                using (var context = new TaskManagementEntities1())
                {
                    var label = context.Labels.FirstOrDefault(l => l.Id == labelId && l.UserId == UserSession.IdUser);
                    if (label == null) return;

                    var window = new LabelOrFilterWindow(
                        isFilter: false,
                        initialName: label.Name,
                        initialValue: "",
                        initialColor: label.Color);
                    if (window.ShowDialog() != true) return;

                    label.Name = window.InputName;
                    label.Color = window.InputColor;
                    context.SaveChanges();
                }
                LoadLabels();
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка при редактировании метки:\n{ex.Message}", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }
    }
}