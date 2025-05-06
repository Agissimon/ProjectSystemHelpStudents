using System;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using ProjectSystemHelpStudents.Helper;
using ProjectSystemHelpStudents.ViewModels;

namespace ProjectSystemHelpStudents.Views.UserPages
{
    public partial class NotificationsPage : UserControl
    {
        public NotificationsPage()
        {
            InitializeComponent();
            LoadNotifications();
        }

        private void LoadNotifications()
        {
            int currentUserId = UserSession.IdUser;
            using (var ctx = new TaskManagementEntities1())
            {
                var invites = ctx.TeamInvitation
                                 .Where(ti =>
                                     ti.InviteeId == currentUserId &&
                                     ti.Status == "Pending")
                                 .OrderByDescending(ti => ti.CreatedAt)
                                 .ToList();

                var list = invites.Select(ti =>
                {
                    var inviter = ctx.Users.FirstOrDefault(u => u.IdUser == ti.InviterId);
                    var team = ctx.Team.FirstOrDefault(t => t.TeamId == ti.TeamId);

                    return new NotificationViewModel
                    {
                        Id = ti.InvitationId,
                        Type = NotificationType.TeamInvitation,
                        Title = "Приглашение в команду",
                        Message = $"{(inviter?.Name ?? "Неизвестный")} пригласил вас в «{team?.Name ?? "—"}»",
                        CreatedAt = ti.CreatedAt ?? DateTime.MinValue,
                        Payload = ti
                    };
                })
                .ToList();

                NotificationsList.ItemsSource = list;
            }
        }

        private void Accept_Click(object sender, RoutedEventArgs e)
        {
            var btn = (Button)sender;
            var ti = btn.Tag as TeamInvitation;
            if (ti == null) return;

            using (var ctx = new TaskManagementEntities1())
            {
                var invite = ctx.TeamInvitation.Find(ti.InvitationId);
                if (invite != null)
                {
                    invite.Status = "Accepted";

                    ctx.TeamMember.Add(new TeamMember
                    {
                        TeamId = invite.TeamId,
                        UserId = invite.InviteeId
                    });

                    ctx.SaveChanges();
                }
            }

            LoadNotifications();
        }

        private void Reject_Click(object sender, RoutedEventArgs e)
        {
            var btn = (Button)sender;
            var ti = btn.Tag as TeamInvitation;
            if (ti == null) return;

            using (var ctx = new TaskManagementEntities1())
            {
                var invite = ctx.TeamInvitation.Find(ti.InvitationId);
                if (invite != null)
                {
                    invite.Status = "Rejected";
                    ctx.SaveChanges();
                }
            }

            LoadNotifications();
        }
    }
}
