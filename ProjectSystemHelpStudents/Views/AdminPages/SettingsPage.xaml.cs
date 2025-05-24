using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Data.Entity;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Media;
using ProjectSystemHelpStudents.Helper;

namespace ProjectSystemHelpStudents.Views.AdminPages
{
    public partial class SettingsPage : Page
    {
        private readonly TaskManagementEntities1 _ctx = new TaskManagementEntities1();

        private ObservableCollection<Priority> _priorities;
        private ICollectionView _prioritiesView;

        private ObservableCollection<Status> _statuses;
        private ICollectionView _statusesView;

        public SettingsPage()
        {
            InitializeComponent();
            LoadPriorities();
            LoadStatuses();
        }

        // ——— PRIORITIES ————————————————————————————————

        private void LoadPriorities()
        {
            _priorities = new ObservableCollection<Priority>(_ctx.Priority.ToList());
            _prioritiesView = CollectionViewSource.GetDefaultView(_priorities);
            _prioritiesView.Filter = FilterPriorities;
            PriorityGrid.ItemsSource = _prioritiesView;
        }

        private bool FilterPriorities(object obj)
        {
            if (obj is Priority p)
            {
                var txt = PrioritySearchBox.Text.Trim().ToLower();
                return string.IsNullOrEmpty(txt) || p.Name.ToLower().Contains(txt);
            }
            return false;
        }

        private void PrioritySearchBox_TextChanged(object sender, TextChangedEventArgs e)
            => _prioritiesView.Refresh();

        private void AddPriorityButton_Click(object sender, RoutedEventArgs e)
        {
            var newPriority = new Priority { Name = string.Empty };
            _ctx.Priority.Add(newPriority);

            OpenEditDialog(
                entity: newPriority,
                isNew: true,
                onSave: () =>
                {
                    _ctx.SaveChanges();
                    _priorities.Add(newPriority);
                },
                onCancel: () =>
                {
                    _ctx.Entry(newPriority).State = EntityState.Detached;
                });
        }

        private void EditPriorityButton_Click(object sender, RoutedEventArgs e)
        {
            if (!(sender is Button btn) || !(btn.Tag is int id)) return;
            var priority = _ctx.Priority.Find(id);
            if (priority == null) return;

            OpenEditDialog(
                entity: priority,
                isNew: false,
                onSave: () =>
                {
                    _ctx.SaveChanges();
                    _prioritiesView.Refresh();
                },
                onCancel: null);
        }

        private void DeletePriorityButton_Click(object sender, RoutedEventArgs e)
        {
            if (!(sender is Button btn) || !(btn.Tag is int id)) return;
            var item = _ctx.Priority.Find(id);
            if (item == null) return;

            if (MessageBox.Show($"Удалить приоритет «{item.Name}»?", "Подтверждение",
                                MessageBoxButton.YesNo, MessageBoxImage.Warning)
                == MessageBoxResult.Yes)
            {
                _ctx.Priority.Remove(item);
                _ctx.SaveChanges();
                _priorities.Remove(item);
            }
        }

        // ——— STATUSES ——————————————————————————————————

        private void LoadStatuses()
        {
            _statuses = new ObservableCollection<Status>(_ctx.Status.ToList());
            _statusesView = CollectionViewSource.GetDefaultView(_statuses);
            _statusesView.Filter = FilterStatuses;
            StatusGrid.ItemsSource = _statusesView;
        }

        private bool FilterStatuses(object obj)
        {
            if (obj is Status s)
            {
                var txt = StatusSearchBox.Text.Trim().ToLower();
                return string.IsNullOrEmpty(txt) || s.Name.ToLower().Contains(txt);
            }
            return false;
        }

        private void StatusSearchBox_TextChanged(object sender, TextChangedEventArgs e)
            => _statusesView.Refresh();

        private void AddStatusButton_Click(object sender, RoutedEventArgs e)
        {
            var newStatus = new Status { Name = string.Empty };
            _ctx.Status.Add(newStatus);

            OpenEditDialog(
                entity: newStatus,
                isNew: true,
                onSave: () =>
                {
                    _ctx.SaveChanges();
                    _statuses.Add(newStatus);
                },
                onCancel: () =>
                {
                    _ctx.Entry(newStatus).State = EntityState.Detached;
                });
        }

        private void EditStatusButton_Click(object sender, RoutedEventArgs e)
        {
            if (!(sender is Button btn) || !(btn.Tag is int id)) return;
            var status = _ctx.Status.Find(id);
            if (status == null) return;

            OpenEditDialog(
                entity: status,
                isNew: false,
                onSave: () =>
                {
                    _ctx.SaveChanges();
                    _statusesView.Refresh();
                },
                onCancel: null);
        }

        private void DeleteStatusButton_Click(object sender, RoutedEventArgs e)
        {
            if (!(sender is Button btn) || !(btn.Tag is int id)) return;
            var item = _ctx.Status.Find(id);
            if (item == null) return;

            if (MessageBox.Show($"Удалить статус «{item.Name}»?", "Подтверждение",
                                MessageBoxButton.YesNo, MessageBoxImage.Warning)
                == MessageBoxResult.Yes)
            {
                _ctx.Status.Remove(item);
                _ctx.SaveChanges();
                _statuses.Remove(item);
            }
        }

        // ——— Shared edit dialog logic ————————————————
        private void OpenEditDialog(
            object entity,
            bool isNew,
            System.Action onSave,
            System.Action onCancel)
        {
            // Получаем текущее имя
            var prop = entity.GetType().GetProperty("Name");
            var current = prop.GetValue(entity) as string ?? string.Empty;

            // Строим окно
            var dlg = new Window
            {
                Title = isNew ? "Добавление" : "Редактирование",
                SizeToContent = SizeToContent.WidthAndHeight,
                WindowStartupLocation = WindowStartupLocation.CenterOwner,
                Owner = Application.Current.MainWindow,
                Style = (Style)Application.Current.FindResource("SmallWindowStyle")
            };

            var tb = new TextBox
            {
                Width = 200,
                Margin = new Thickness(10),
                Text = current
            };
            tb.TextChanged += (_, __) => prop.SetValue(entity, tb.Text);

            var btnOk = new Button
            {
                Content = "OK",
                IsDefault = true,
                Margin = new Thickness(10),
                Padding = new Thickness(10, 4, 10, 4)
            };
            btnOk.Click += (_, __) =>
            {
                if (string.IsNullOrWhiteSpace(tb.Text))
                {
                    MessageBox.Show("Введите название.", "Ошибка",
                                    MessageBoxButton.OK, MessageBoxImage.Warning);
                    return;
                }
                onSave?.Invoke();
                dlg.Close();
            };

            var panel = new StackPanel();
            panel.Children.Add(new TextBlock
            {
                Text = "Название:",
                Margin = new Thickness(10, 10, 10, 0),
                Foreground = Brushes.White
            });
            panel.Children.Add(tb);
            panel.Children.Add(btnOk);

            dlg.Content = panel;
            dlg.Closed += (_, __) =>
            {
                if (string.IsNullOrWhiteSpace(prop.GetValue(entity) as string))
                    onCancel?.Invoke();
            };

            dlg.ShowDialog();
        }
    }
}
