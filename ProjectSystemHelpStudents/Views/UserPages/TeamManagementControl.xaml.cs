using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media.Imaging;
using ProjectSystemHelpStudents.Helper;
using ProjectSystemHelpStudents.ViewModels;

namespace ProjectSystemHelpStudents.Views.UserPages
{
    public partial class TeamManagementControl : UserControl
    {
        private readonly TaskManagementEntities1 _ctx = new TaskManagementEntities1();
        private TeamViewModel _selectedTeam;

        public TeamManagementControl()
        {
            InitializeComponent();
            LoadTeams();
        }

        private void LoadTeams()
        {
            int currentUserId = UserSession.IdUser;
            int? selectedId = _selectedTeam?.TeamId;

            var teams = (from t in _ctx.Team
                         where t.LeaderId == currentUserId
                               || _ctx.TeamMember.Any(tm => tm.TeamId == t.TeamId && tm.UserId == currentUserId)
                         join u in _ctx.Users on t.LeaderId equals u.IdUser into lu
                         from leader in lu.DefaultIfEmpty()
                         select new TeamViewModel
                         {
                             TeamId = t.TeamId,
                             Name = t.Name,
                             LeaderName = leader != null ? leader.Name : "<нет лидера>"
                         }).ToList();

            TeamsListView.ItemsSource = teams;

            if (selectedId.HasValue)
            {
                var restored = teams.FirstOrDefault(t => t.TeamId == selectedId.Value);
                TeamsListView.SelectedItem = restored;
            }
            else
            {
                MembersList.ItemsSource = null;
            }
        }

        private void TeamsListView_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            _selectedTeam = TeamsListView.SelectedItem as TeamViewModel;
            if (_selectedTeam == null)
            {
                MembersList.ItemsSource = null;
                return;
            }
            PopulateMembers(_selectedTeam.TeamId);
        }

        private void PopulateMembers(int teamId)
        {
            var participantList = new List<ParticipantItem>();

            var teamEntity = _ctx.Team.Find(teamId);
            if (teamEntity != null)
            {
                var leaderUser = _ctx.Users.Find(teamEntity.LeaderId);
                if (leaderUser != null)
                {
                    participantList.Add(new ParticipantItem
                    {
                        UserId = leaderUser.IdUser,
                        Name = leaderUser.Name,
                        Status = "Лидер",
                        IsInvitation = false
                    });
                }
            }

            var members = _ctx.TeamMember
                .Where(tm => tm.TeamId == teamId)
                .Include(tm => tm.Users)
                .Select(tm => new ParticipantItem
                {
                    UserId = tm.UserId,
                    Name = tm.Users.Name,
                    Status = "Участник",
                    IsInvitation = false
                }).ToList();
            participantList.AddRange(members);

            var invites = _ctx.TeamInvitation
                .Where(ti => ti.TeamId == teamId)
                .Include(ti => ti.Users)
                .Select(ti => new ParticipantItem
                {
                    UserId = ti.InviteeId,
                    Name = ti.Users.Name,
                    Status = ti.Status,
                    IsInvitation = true
                }).ToList();
            participantList.AddRange(invites.Where(i => !participantList.Any(p => p.UserId == i.UserId)));

            MembersList.ItemsSource = participantList;
        }

        private void NewTeam_Click(object sender, RoutedEventArgs e)
        {
            var dlg = new InputDialog("Введите название новой команды:", "MyTask");
            if (dlg.ShowDialog() != true) return;

            var team = new Team
            {
                Name = dlg.InputText,
                LeaderId = UserSession.IdUser
            };
            _ctx.Team.Add(team);
            _ctx.SaveChanges();

            _selectedTeam = new TeamViewModel { TeamId = team.TeamId };
            LoadTeams();
        }

        private void EditTeam_Click(object sender, RoutedEventArgs e)
        {
            if (_selectedTeam == null) return;

            var dlg = new InputDialog("Введите новое название:", "MyTask");
            dlg.InputText = _selectedTeam.Name;
            if (dlg.ShowDialog() != true) return;

            var teamEntity = _ctx.Team.FirstOrDefault(t => t.TeamId == _selectedTeam.TeamId);
            if (teamEntity == null) return;

            teamEntity.Name = dlg.InputText;
            _ctx.SaveChanges();
            LoadTeams();
        }

        private void DeleteTeam_Click(object sender, RoutedEventArgs e)
        {
            if (_selectedTeam == null) return;

            if (MessageBox.Show($"Удалить команду '{_selectedTeam.Name}' со всеми участниками и приглашениями?",
                                "Подтверждение",
                                MessageBoxButton.YesNo,
                                MessageBoxImage.Warning) != MessageBoxResult.Yes)
            {
                return;
            }

            var teamEntity = _ctx.Team.Find(_selectedTeam.TeamId);
            if (teamEntity == null)
            {
                MessageBox.Show("Команда не найдена в БД", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
                return;
            }

            // Удаляем приглашения
            var invites = _ctx.TeamInvitation.Where(ti => ti.TeamId == teamEntity.TeamId).ToList();
            _ctx.TeamInvitation.RemoveRange(invites);

            var members = _ctx.TeamMember.Where(tm => tm.TeamId == teamEntity.TeamId).ToList();
            _ctx.TeamMember.RemoveRange(members);

            _ctx.Team.Remove(teamEntity);

            _ctx.SaveChanges();

            _selectedTeam = null;
            LoadTeams();
        }

        private void InviteMember_Click(object sender, RoutedEventArgs e)
        {
            if (_selectedTeam == null) return;

            var inviteControl = new InviteUserControl { TeamName = _selectedTeam.Name };
            var win = new Window
            {
                MaxHeight = 300,
                Title = "MyTask",
                Content = inviteControl,
                SizeToContent = SizeToContent.WidthAndHeight,
                Icon = new BitmapImage(new Uri(
                    "pack://application:,,,/ProjectSystemHelpStudents;component/Resources/Icon/logo001.ico",
                    UriKind.Absolute)),
                WindowStartupLocation = WindowStartupLocation.CenterOwner,
                Owner = Window.GetWindow(this)
            };

            inviteControl.Confirmed += (s, user) => { win.DialogResult = true; win.Close(); };
            inviteControl.Cancelled += (s, _) => { win.DialogResult = false; win.Close(); };

            if (win.ShowDialog() != true) return;
            var targetUser = inviteControl.SelectedUser;
            if (targetUser == null) return;

            // Создаём приглашение
            if (_ctx.TeamInvitation.Any(ti => ti.TeamId == _selectedTeam.TeamId && ti.InviteeId == targetUser.IdUser && ti.Status == "В ожидании"))
            {
                MessageBox.Show("Пользователь уже приглашён.", "Информация", MessageBoxButton.OK, MessageBoxImage.Information);
                return;
            }

            var invite = new TeamInvitation
            {
                TeamId = _selectedTeam.TeamId,
                InviterId = UserSession.IdUser,
                InviteeId = targetUser.IdUser,
                Status = "В ожидании",
                CreatedAt = DateTime.Now
            };
            _ctx.TeamInvitation.Add(invite);
            _ctx.SaveChanges();

            PopulateMembers(_selectedTeam.TeamId);
        }

        private void RemoveMember_Click(object sender, RoutedEventArgs e)
        {
            if (_selectedTeam == null) return;

            if (!(sender is Button btn) || !(btn.DataContext is ParticipantItem item))
                return;

            if (item.Status == "Лидер")
            {
                MessageBox.Show("Нельзя удалить лидера.", "Предупреждение", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            var invites = _ctx.TeamInvitation
                .Where(ti => ti.TeamId == _selectedTeam.TeamId && ti.InviteeId == item.UserId)
                .ToList();

            if (invites.Any())
            {
                _ctx.TeamInvitation.RemoveRange(invites);
            }

            var member = _ctx.TeamMember
                .FirstOrDefault(tm => tm.TeamId == _selectedTeam.TeamId && tm.UserId == item.UserId);

            if (member != null)
            {
                _ctx.TeamMember.Remove(member);
            }

            _ctx.SaveChanges();

            PopulateMembers(_selectedTeam.TeamId);
        }
    }
}
