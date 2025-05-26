using System;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Data.Entity.Infrastructure;
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
            if (!(sender is Button btn) || !(btn.Tag is int id))
                return;

            var owned = _ctx.Project
                .Include("Task")
                .Include("Task.Files")
                .Include("Task.Comment")
                .Include("Section")
                .Where(p => p.OwnerId == id)
                .ToList();

            foreach (var pr in owned)
            {
                // удаляем все зависимости
                foreach (var t in pr.Task.ToList())
                {
                    _ctx.Files.RemoveRange(t.Files);
                    _ctx.Comment.RemoveRange(t.Comment);
                    _ctx.TaskAssignee.RemoveRange(t.TaskAssignee);
                    _ctx.TaskFilters.RemoveRange(t.TaskFilters);
                    _ctx.TaskLabels.RemoveRange(t.TaskLabels);
                    _ctx.Task.Remove(t);
                }

                _ctx.Section.RemoveRange(pr.Section);

                _ctx.Project.Remove(pr);
            }

            var user = _ctx.Users
                .Include("TaskAssignee")
                .Include("Comment")
                .Include("TeamMember")
                .Include("Filters")
                .Include("TeamInvitation")
                .Include("TeamInvitation1")
                .FirstOrDefault(u => u.IdUser == id);

            if (user == null)
                return;

            if (MessageBox.Show(
                    $"Удалить пользователя «{user.Name}»?",
                    "Подтверждение",
                    MessageBoxButton.YesNo,
                    MessageBoxImage.Warning
                ) != MessageBoxResult.Yes)
                return;

            _ctx.TaskAssignee.RemoveRange(user.TaskAssignee);
            _ctx.Comment.RemoveRange(user.Comment);
            _ctx.TeamMember.RemoveRange(user.TeamMember);
            _ctx.Filters.RemoveRange(user.Filters);
            _ctx.TeamInvitation.RemoveRange(user.TeamInvitation);
            _ctx.TeamInvitation.RemoveRange(user.TeamInvitation1);

            _ctx.Users.Remove(user);

            try
            {
                _ctx.SaveChanges();
                _users.Remove(user);
            }
            catch (DbUpdateException dbEx)
            {
                var msg = dbEx.InnerException?.InnerException?.Message
                          ?? dbEx.InnerException?.Message
                          ?? dbEx.Message;
                MessageBox.Show($"Ошибка при удалении:\n{msg}", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
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
