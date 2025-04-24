using ProjectSystemHelpStudents.Helper;
using ProjectSystemHelpStudents.Views.TaskPages;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
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
            var window = new LabelOrFilterWindow(isFilter: true);
            if (window.ShowDialog() == true)
            {
                if (window.InputName.Contains(" "))
                {
                    MessageBox.Show("Название фильтра не может содержать пробел.");
                    return;
                }

                using (var context = new TaskManagementEntities1())
                {
                    var existing = context.Filters.FirstOrDefault(f => f.Name == window.InputName);
                    if (existing != null)
                    {
                        MessageBox.Show("Фильтр с таким названием уже существует.");
                        return;
                    }

                    var filter = new Filters
                    {
                        Name = window.InputName,
                        Query = window.InputQuery,
                        UserId = UserSession.IdUser
                    };

                    context.Filters.Add(filter);
                    context.SaveChanges();
                    LoadFilters();
                }
            }
        }


        private void AddLabel_Click(object sender, RoutedEventArgs e)
        {
            var window = new LabelOrFilterWindow(isFilter: false);
            if (window.ShowDialog() == true)
            {
                using (var context = new TaskManagementEntities1())
                {
                    var label = new Labels
                    {
                        Name = window.InputName,
                        Color = window.InputColor
                        //IsFavorite = window.IsFavorite
                    };
                    context.Labels.Add(label);
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

        private void EditLabel_Click(object sender, RoutedEventArgs e)
        {
            var button = sender as Button;
            if (button?.CommandParameter is int labelId)
            {
                using (var context = new TaskManagementEntities1())
                {
                    var label = context.Labels.FirstOrDefault(l => l.Id == labelId);
                    if (label != null)
                    {
                        var window = new LabelOrFilterWindow(isFilter: false, label.Name, label.Color);
                        if (window.ShowDialog() == true)
                        {
                            label.Name = window.InputName;
                            label.Color = window.InputColor;
                            context.SaveChanges();
                            LoadLabels();
                        }
                    }
                }
            }
        }

        private void EditFilter_Click(object sender, RoutedEventArgs e)
        {
            var button = sender as Button;
            if (button?.CommandParameter is int filterId)
            {
                using (var context = new TaskManagementEntities1())
                {
                    var filter = context.Filters.FirstOrDefault(f => f.Id == filterId);
                    if (filter != null)
                    {
                        var window = new LabelOrFilterWindow(
                            isFilter: true,
                            initialName: filter.Name,
                            initialValue: filter.Query,
                            initialColor: filter.Color
                        );

                        if (window.ShowDialog() == true)
                        {
                            filter.Name = window.InputName;
                            filter.Query = window.InputQuery;
                            filter.Color = window.InputColor;
                            context.SaveChanges();
                            LoadFilters();
                        }
                    }
                }
            }
        }
    }
}
