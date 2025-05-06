using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using ProjectSystemHelpStudents.Helper;
using ProjectSystemHelpStudents.ViewModels;

namespace ProjectSystemHelpStudents.Views.UserPages
{

    public partial class TeamManagementControl : UserControl
    {
        private readonly TaskManagementEntities1 _ctx = new TaskManagementEntities1();
        private TeamViewModel _selectedTeam;
        private class ParticipantItem
        {
            public int UserId { get; set; }
            public string Name { get; set; }
            public string Status { get; set; }
        }

        public TeamManagementControl()
        {
            InitializeComponent();
            LoadTeams();
        }

        private void LoadTeams()
        {
            int? selectedId = _selectedTeam?.TeamId;

            var teams = (from t in _ctx.Team
                         join u in _ctx.Users on t.LeaderId equals u.IdUser into lu
                         from leader in lu.DefaultIfEmpty()
                         select new TeamViewModel
                         {
                             TeamId = t.TeamId,
                             Name = t.Name,
                             LeaderName = leader != null ? leader.Name : "<нет лидера>"
                         })
                         .ToList();

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

            int teamId = _selectedTeam.TeamId;
            var participantList = new List<ParticipantItem>();

            // Лидер команды
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
                        Status = "Leader"
                    });
                }
            }

            // члены команды
            var members = (from tm in _ctx.TeamMember
                           join u in _ctx.Users on tm.UserId equals u.IdUser
                           where tm.TeamId == teamId
                           select new ParticipantItem
                           {
                               UserId = u.IdUser,
                               Name = u.Name,
                               Status = "Member"
                           })
                          .ToList();
            participantList.AddRange(members);

            // приглашения
            var allInvites = (from ti in _ctx.TeamInvitation
                              join u in _ctx.Users on ti.InviteeId equals u.IdUser
                              where ti.TeamId == teamId
                              select new ParticipantItem
                              {
                                  UserId = u.IdUser,
                                  Name = u.Name,
                                  Status = ti.Status
                              }).ToList();

            var invites = allInvites
                          .Where(inv => !participantList.Any(p => p.UserId == inv.UserId))
                          .ToList();

            participantList.AddRange(invites);

            MembersList.ItemsSource = participantList;
        }

        private void NewTeam_Click(object sender, RoutedEventArgs e)
        {
            var dlg = new InputDialog("Введите название новой команды:", "Новая команда");
            if (dlg.ShowDialog() == true)
            {
                var team = new Team { Name = dlg.InputText, LeaderId = UserSession.IdUser };
                _ctx.Team.Add(team);
                _ctx.SaveChanges();

                _ctx.TeamMember.Add(new TeamMember
                {
                    TeamId = team.TeamId,
                    UserId = UserSession.IdUser
                });
                _ctx.SaveChanges();

                _selectedTeam = new TeamViewModel { TeamId = team.TeamId };
                LoadTeams();
            }
        }

        private void EditTeam_Click(object sender, RoutedEventArgs e)
        {
            if (_selectedTeam == null) return;

            var dlg = new InputDialog("Введите новое название:", "Редактировать команду");
            dlg.InputText = _selectedTeam.Name;
            if (dlg.ShowDialog() == true)
            {
                var teamEntity = _ctx.Team.FirstOrDefault(t => t.TeamId == _selectedTeam.TeamId);
                if (teamEntity != null)
                {
                    teamEntity.Name = dlg.InputText;
                    _ctx.SaveChanges();
                    LoadTeams();
                }
            }
        }

        private void DeleteTeam_Click(object sender, RoutedEventArgs e)
        {
            if (_selectedTeam == null) return;

            if (MessageBox.Show($"Удалить команду «{_selectedTeam.Name}»?",
                                "Подтверждение", MessageBoxButton.YesNo,
                                MessageBoxImage.Warning) != MessageBoxResult.Yes)
                return;

            var teamEntity = _ctx.Team.FirstOrDefault(t => t.TeamId == _selectedTeam.TeamId);
            if (teamEntity == null)
            {
                MessageBox.Show("Команда не найдена в БД", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
                return;
            }

            var links = _ctx.TeamMember.Where(tm => tm.TeamId == teamEntity.TeamId).ToList();
            _ctx.TeamMember.RemoveRange(links);
            _ctx.Team.Remove(teamEntity);
            _ctx.SaveChanges();

            _selectedTeam = null;
            LoadTeams();
        }

        private void InviteMember_Click(object sender, RoutedEventArgs e)
        {
            if (_selectedTeam == null) return;

            var inviteControl = new InviteUserControl();
            var win = new Window
            {
                Title = "Пригласить в " + _selectedTeam.Name,
                Content = inviteControl,
                SizeToContent = SizeToContent.WidthAndHeight,
                WindowStartupLocation = WindowStartupLocation.CenterOwner,
                Owner = Window.GetWindow(this)
            };

            inviteControl.Confirmed += (s, user) =>
            {
                win.DialogResult = true;
                win.Close();
            };
            inviteControl.Cancelled += (s, _) =>
            {
                win.DialogResult = false;
                win.Close();
            };

            if (win.ShowDialog() != true) return;

            var targetUser = inviteControl.SelectedUser;
            if (targetUser == null) return;

            using (var ctx = new TaskManagementEntities1())
            {
                // проверим, нет ли уже приглашения
                bool exists = ctx.TeamInvitation.Any(ti =>
                    ti.TeamId == _selectedTeam.TeamId &&
                    ti.InviteeId == targetUser.IdUser &&
                    ti.Status == "Pending");

                if (exists)
                {
                    MessageBox.Show("Пользователь уже приглашён и ждет подтверждения.",
                                    "Информация", MessageBoxButton.OK, MessageBoxImage.Information);
                    return;
                }

                // создаём новое приглашение
                var invite = new TeamInvitation
                {
                    TeamId = _selectedTeam.TeamId,
                    InviterId = UserSession.IdUser,
                    InviteeId = targetUser.IdUser,
                    Status = "Pending",
                    CreatedAt = DateTime.Now
                };
                ctx.TeamInvitation.Add(invite);
                ctx.SaveChanges();
            }

            MessageBox.Show("Приглашение отправлено.", "Успех", MessageBoxButton.OK, MessageBoxImage.Information);
        }

        private void RemoveMember_Click(object sender, RoutedEventArgs e)
        {
            if (_selectedTeam == null) return;

            var btn = (Button)sender;
            if (btn.DataContext is Users user)
            {
                var link = _ctx.TeamMember.FirstOrDefault(tm => tm.TeamId == _selectedTeam.TeamId &&
                                                                 tm.UserId == user.IdUser);
                if (link != null)
                {
                    _ctx.TeamMember.Remove(link);
                    _ctx.SaveChanges();
                    LoadTeams();
                }
            }
        }
    }
}
