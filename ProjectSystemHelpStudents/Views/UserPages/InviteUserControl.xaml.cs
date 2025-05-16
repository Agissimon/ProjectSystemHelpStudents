using ProjectSystemHelpStudents.Helpers;
using System;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Data.Entity;
using System.Linq;
using System.Windows.Controls;
using System.Windows.Input;

namespace ProjectSystemHelpStudents.Views.UserPages
{
    /// <summary>
    /// Логика взаимодействия для InviteUserControl.xaml
    /// </summary>
    public partial class InviteUserControl : UserControl, INotifyPropertyChanged
    {
        public InviteUserControl()
        {
            InitializeComponent();
            DataContext = this;

            ConfirmCommand = new RelayCommand(_ => OnConfirmed(), _ => SelectedUser != null);
            CancelCommand = new RelayCommand(_ => OnCancelled());
        }

        private string _searchText;
        public string SearchText
        {
            get => _searchText;
            set
            {
                if (_searchText == value) return;
                _searchText = value;
                OnPropertyChanged(nameof(SearchText));
                RefreshSearchResults();
            }
        }

        public string TeamName
        {
            get => txtTeamName.Text;
            set => txtTeamName.Text = $"Пригласить в команду «{value}»";
        }

        // Список найденных пользователей
        public ObservableCollection<Users> Users { get; } = new ObservableCollection<Users>();

        private Users _selectedUser;
        public Users SelectedUser
        {
            get => _selectedUser;
            set
            {
                if (_selectedUser == value) return;
                _selectedUser = value;
                OnPropertyChanged(nameof(SelectedUser));
                OnPropertyChanged(nameof(IsUserSelected));
                OnPropertyChanged(nameof(AddButtonText));
            }
        }

        public bool IsUserSelected => SelectedUser != null;
        public string AddButtonText => SelectedUser != null
            ? $"Пригласить {SelectedUser.Login}"
            : "Пригласить";

        public ICommand ConfirmCommand { get; }
        public ICommand CancelCommand { get; }

        public event EventHandler<Users> Confirmed;
        public event EventHandler Cancelled;

        private void OnConfirmed() => Confirmed?.Invoke(this, SelectedUser);
        private void OnCancelled() => Cancelled?.Invoke(this, EventArgs.Empty);

        // Поиск пользователей через локальный контекст
        private void RefreshSearchResults()
        {
            Users.Clear();
            if (string.IsNullOrWhiteSpace(SearchText))
                return;

            var q = SearchText.Trim().ToLower();
            using (var ctx = new TaskManagementEntities1())
            {
                var found = ctx.Users
                    .Where(u =>
                        u.Login.ToLower().Contains(q) ||
                        u.Name.ToLower().Contains(q) ||
                        u.Mail.ToLower().Contains(q))
                    .OrderBy(u => u.Login)
                    .Take(20)
                    .ToList();

                foreach (var u in found)
                    Users.Add(u);
            }
        }

        public event PropertyChangedEventHandler PropertyChanged;
        protected void OnPropertyChanged(string prop) =>
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(prop));
    }
}
