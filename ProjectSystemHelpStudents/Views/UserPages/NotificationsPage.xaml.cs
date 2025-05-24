using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using ProjectSystemHelpStudents.Helper;
using ProjectSystemHelpStudents.ViewModels;
using ProjectSystemHelpStudents.Views.TaskPages;

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
            int userId = UserSession.IdUser;
            List<NotificationViewModel> allNotifications;

            using (var ctx = new TaskManagementEntities1())
            {
                // Гасим маркеры новых уведомлений (только для бейджа)
                var newInvites = ctx.TeamInvitation
                                    .Where(ti => ti.InviteeId == userId
                                              && ti.Status == "В ожидании"
                                              && ti.IsNew == true)
                                    .ToList();
                foreach (var ti in newInvites)
                    ti.IsNew = false;

                var newAssignments = ctx.TaskAssignee
                                        .Where(ta => ta.UserId == userId
                                                  && ta.IsNew == true)
                                        .ToList();
                foreach (var ta in newAssignments)
                    ta.IsNew = false;

                ctx.SaveChanges();

                // Вычитываем все (ожидающие) приглашения независимо от IsNew
                var invitesRaw = (from ti in ctx.TeamInvitation
                                  where ti.InviteeId == userId
                                        && ti.Status == "В ожидании"
                                  join u in ctx.Users on ti.InviterId equals u.IdUser
                                  join t in ctx.Team on ti.TeamId equals t.TeamId
                                  orderby ti.CreatedAt descending
                                  select new
                                  {
                                      ti,
                                      ti.InvitationId,
                                      ti.CreatedAt,
                                      InviterName = u.Name,
                                      TeamName = t.Name
                                  })
                                 .ToList();

                // Вычитываем все (активные) назначения независимо от IsNew
                var assignmentsRaw = (from ta in ctx.TaskAssignee
                                      where ta.UserId == userId
                                      join task in ctx.Task on ta.TaskId equals task.IdTask
                                      join creator in ctx.Users on task.CreatorId equals creator.IdUser
                                      orderby ta.TaskAssigneeId descending
                                      select new
                                      {
                                          ta,
                                          ta.TaskAssigneeId,
                                          ta.AssignedAt,
                                          CreatorName = creator.Name,
                                          TaskTitle = task.Title
                                      })
                                     .ToList();

                var invites = invitesRaw
                    .Select(x => new NotificationViewModel
                    {
                        Id = x.InvitationId,
                        Type = NotificationType.TeamInvitation,
                        Title = "Приглашение в команду",
                        Message = $"{x.InviterName} пригласил вас в «{x.TeamName}»",
                        CreatedAt = x.CreatedAt ?? DateTime.Now,
                        Payload = x.ti
                    })
                    .ToList();

                var assignments = assignmentsRaw
                    .Select(x => new NotificationViewModel
                    {
                        Id = x.TaskAssigneeId,
                        Type = NotificationType.TaskAssignee,
                        Title = "Вас назначили на задачу",
                        Message = $"{x.CreatorName} назначил вас на «{x.TaskTitle}»",
                        CreatedAt = x.AssignedAt ?? DateTime.Now,
                        Payload = x.ta
                    })
                    .ToList();

                allNotifications = invites
                    .Concat(assignments)
                    .OrderByDescending(n => n.CreatedAt)
                    .ToList();
            }

            NotificationsList.ItemsSource = allNotifications;
            UserSession.RaiseNotificationsChanged();
        }

        private void Accept_Click(object sender, RoutedEventArgs e)
        {
            var vm = (NotificationViewModel)((Button)sender).CommandParameter;
            var ti = vm.Payload as TeamInvitation;
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
            var vm = (NotificationViewModel)((Button)sender).CommandParameter;
            var ti = vm.Payload as TeamInvitation;
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

        private void OpenTask_Click(object sender, RoutedEventArgs e)
        {
            var vm = (NotificationViewModel)((Button)sender).CommandParameter;
            if (vm.Type != NotificationType.TaskAssignee) return;

            var ta = vm.Payload as TaskAssignee;
            if (ta == null) return;

            // Создаём TaskViewModel только с ID задачи:
            var taskVm = new TaskViewModel { IdTask = ta.TaskId };

            var window = new TaskDetailsWindow(taskVm);
            window.TaskUpdated += () => LoadNotifications();
            window.Show();
        }

        private void IgnoreTask_Click(object sender, RoutedEventArgs e)
        {
            // Достаём модель уведомления
            var vm = (NotificationViewModel)((Button)sender).CommandParameter;
            if (vm.Type != NotificationType.TaskAssignee)
                return;

            // Получаем текущий список из UI
            var current = NotificationsList.ItemsSource
                             as List<NotificationViewModel>;
            if (current == null) return;

            var filtered = current
                .Where(n => !(n.Type == vm.Type && n.Id == vm.Id))
                .ToList();

            NotificationsList.ItemsSource = filtered;
        }
    }
}