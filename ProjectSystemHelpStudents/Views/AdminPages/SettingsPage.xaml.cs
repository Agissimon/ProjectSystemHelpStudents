using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Data.Entity;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;

namespace ProjectSystemHelpStudents.Views.AdminPages
{
    public partial class SettingsPage : Page
    {
        private readonly TaskManagementEntities1 _ctx = new TaskManagementEntities1();
        private ObservableCollection<Priority> _priorities;
        private ICollectionView _prioritiesView;

        public SettingsPage()
        {
            InitializeComponent();
            LoadPriorities();
        }

        private void LoadPriorities()
        {
            _priorities = new ObservableCollection<Priority>(_ctx.Priority.ToList());
            _prioritiesView = CollectionViewSource.GetDefaultView(_priorities);
            _prioritiesView.Filter = FilterBySearch;
            PriorityGrid.ItemsSource = _prioritiesView;
        }

        private bool FilterBySearch(object item)
        {
            if (item is Priority p)
            {
                var text = PrioritySearchBox.Text?.Trim().ToLower() ?? "";
                return string.IsNullOrEmpty(text) || p.Name.ToLower().Contains(text);
            }
            return false;
        }

        private void PrioritySearchBox_TextChanged(object sender, TextChangedEventArgs e)
        {
            _prioritiesView.Refresh();
        }

        private void AddPriorityButton_Click(object sender, RoutedEventArgs e)
        {
            var newPriority = new Priority { Name = "" };
            OpenEditDialog(newPriority, isNew: true);
        }

        private void EditPriorityButton_Click(object sender, RoutedEventArgs e)
        {
            if (sender is Button btn && btn.Tag is int id)
            {
                var priority = _ctx.Priority.Find(id);
                if (priority != null)
                    OpenEditDialog(priority, isNew: false);
            }
        }

        private void OpenEditDialog(Priority priority, bool isNew)
        {
            if (isNew)
                _ctx.Priority.Add(priority);

            var editControl = new PriorityEditControl(priority, _ctx);

            var dlg = new Window
            {
                Title = isNew ? "Новый приоритет" : "Редактирование приоритета",
                Content = editControl,
                SizeToContent = SizeToContent.WidthAndHeight,
                WindowStartupLocation = WindowStartupLocation.CenterOwner,
                Owner = Application.Current.MainWindow,
                Style = (Style)Application.Current.FindResource("SmallWindowStyle")
            };

            dlg.Closed += (s, args) =>
            {
                // Если пустое имя — не сохраняем новый
                if (isNew)
                {
                    if (!string.IsNullOrWhiteSpace(priority.Name))
                    {
                        _ctx.SaveChanges();
                        _priorities.Add(priority);
                    }
                    else
                    {
                        // Откат добавления
                        _ctx.Entry(priority).State = EntityState.Detached;
                    }
                }
                else
                {
                    _ctx.SaveChanges();
                    _prioritiesView.Refresh();
                }
            };

            dlg.ShowDialog();
        }

        private void DeletePriorityButton_Click(object sender, RoutedEventArgs e)
        {
            if (sender is Button btn && btn.Tag is int id)
            {
                var item = _ctx.Priority.Find(id);
                if (item != null)
                {
                    if (MessageBox.Show($"Удалить приоритет «{item.Name}»?",
                                        "Подтверждение",
                                        MessageBoxButton.YesNo,
                                        MessageBoxImage.Warning) == MessageBoxResult.Yes)
                    {
                        _ctx.Priority.Remove(item);
                        _ctx.SaveChanges();
                        _priorities.Remove(_priorities.First(p => p.PriorityId == id));
                    }
                }
            }
        }
    }
}
