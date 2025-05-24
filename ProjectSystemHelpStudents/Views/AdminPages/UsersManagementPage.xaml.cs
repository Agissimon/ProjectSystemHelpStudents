using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;

namespace ProjectSystemHelpStudents.Views.AdminPages
{
    public partial class UsersManagementPage : Page
    {
        private readonly TaskManagementEntities1 _ctx = new TaskManagementEntities1();
        private ObservableCollection<Users> _users;
        private ICollectionView _usersView;

        public UsersManagementPage()
        {
            InitializeComponent();
            LoadUsers();
        }

        private void LoadUsers()
        {
            _users = new ObservableCollection<Users>(_ctx.Users.ToList());
            _usersView = CollectionViewSource.GetDefaultView(_users);
            UsersGrid.ItemsSource = _usersView;
        }

        private void SearchBox_TextChanged(object sender, TextChangedEventArgs e)
        {
            var filter = SearchBox.Text.Trim().ToLower();
            _usersView.Filter = item =>
            {
                if (item is Users u)
                {
                    return string.IsNullOrEmpty(filter)
                        || (u.Name?.ToLower().Contains(filter) ?? false)
                        || (u.Surname?.ToLower().Contains(filter) ?? false)
                        || (u.Login?.ToLower().Contains(filter) ?? false)
                        || (u.Mail?.ToLower().Contains(filter) ?? false);
                }
                return false;
            };
        }

        private void DeleteButton_Click(object sender, RoutedEventArgs e)
        {
            if (sender is Button btn && btn.Tag is int id)
            {
                var user = _ctx.Users.Find(id);
                if (user != null &&
                    MessageBox.Show($"Удалить пользователя «{user.Name}»?", "Подтверждение",
                                    MessageBoxButton.YesNo, MessageBoxImage.Warning) == MessageBoxResult.Yes)
                {
                    _ctx.Users.Remove(user);
                    _ctx.SaveChanges();
                    _users.Remove(user);
                }
            }
        }

        private void EditButton_Click(object sender, RoutedEventArgs e)
        {
            if (sender is Button btn && btn.Tag is int id)
            {
                var user = _ctx.Users.Find(id);
                if (user != null)
                {
                    var wnd = new Window
                    {
                        Title = "Редактирование пользователя",
                        Content = new UserEditControl(user, _ctx),
                        Width = 320,
                        Height = 420,
                        ResizeMode = ResizeMode.NoResize,
                        WindowStartupLocation = WindowStartupLocation.CenterScreen,
                        Owner = Application.Current.MainWindow,
                        Style = (Style)Application.Current.FindResource("SmallWindowStyle")
                    };
                    wnd.ShowDialog();
                    LoadUsers();
                }
            }
        }
    }
}
