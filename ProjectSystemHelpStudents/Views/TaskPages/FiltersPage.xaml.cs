using ProjectSystemHelpStudents.Helper;
using ProjectSystemHelpStudents.Views.TaskPages;
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
            int currentUser = UserSession.IdUser;
            using (var context = new TaskManagementEntities1())
            {
                // Только свои фильтры
                var filters = context.Filters
                                     .Where(f => f.UserId == currentUser)
                                     .ToList();
                FiltersList.ItemsSource = filters;
            }
        }

        private void LoadLabels()
        {
            int currentUser = UserSession.IdUser;
            using (var ctx = new TaskManagementEntities1())
            {
                var labels = ctx.Labels
                                .Where(l => l.UserId == currentUser)
                                .ToList();
                LabelsList.ItemsSource = labels;
            }
        }

        private void AddFilter_Click(object sender, RoutedEventArgs e)
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

        private void AddLabel_Click(object sender, RoutedEventArgs e)
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

        private void DeleteFilter_Clik(object sender, RoutedEventArgs e)
        {
            if (!(sender is Button btn) || !(btn.CommandParameter is int filterId))
                return;

            using (var context = new TaskManagementEntities1())
            {
                var filter = context.Filters
                                    .FirstOrDefault(f => f.Id == filterId && f.UserId == UserSession.IdUser);
                if (filter != null)
                {
                    context.Filters.Remove(filter);
                    context.SaveChanges();
                }
            }
            LoadFilters();
        }

        private void DeleteLabel_Clik(object sender, RoutedEventArgs e)
        {
            if (!(sender is Button btn) || !(btn.CommandParameter is int labelId))
                return;

            using (var context = new TaskManagementEntities1())
            {
                var label = context.Labels
                                   .FirstOrDefault(l => l.Id == labelId && l.UserId == UserSession.IdUser);
                if (label != null)
                {
                    context.Labels.Remove(label);
                    context.SaveChanges();
                }
            }
            LoadLabels();
        }

        private void EditFilter_Click(object sender, RoutedEventArgs e)
        {
            if (!(sender is Button btn) || !(btn.CommandParameter is int filterId))
                return;

            using (var context = new TaskManagementEntities1())
            {
                var filter = context.Filters
                                    .FirstOrDefault(f => f.Id == filterId && f.UserId == UserSession.IdUser);
                if (filter == null) return;

                var window = new LabelOrFilterWindow(
                    isFilter: true,
                    initialName: filter.Name,
                    initialValue: filter.Query,
                    initialColor: filter.Color
                );
                if (window.ShowDialog() != true) return;

                filter.Name = window.InputName;
                filter.Query = window.InputQuery;
                filter.Color = window.InputColor;
                context.SaveChanges();
            }

            LoadFilters();
        }

        private void EditLabel_Click(object sender, RoutedEventArgs e)
        {
            if (!(sender is Button btn) || !(btn.CommandParameter is int labelId))
                return;

            using (var context = new TaskManagementEntities1())
            {
                var label = context.Labels
                                   .FirstOrDefault(l => l.Id == labelId && l.UserId == UserSession.IdUser);
                if (label == null) return;

                var window = new LabelOrFilterWindow(
                    isFilter: false,
                    initialName: label.Name,
                    initialValue: "",
                    initialColor: label.Color
                );
                if (window.ShowDialog() != true) return;

                label.Name = window.InputName;
                label.Color = window.InputColor;
                context.SaveChanges();
            }

            LoadLabels();
        }
    }
}
