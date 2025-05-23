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
                // 1) Вычитываем «сырые» данные по новым приглашениям
                var invitesRaw = (from ti in ctx.TeamInvitation
                                  where ti.InviteeId == userId
                                        && ti.Status == "В ожидании"
                                        && ti.IsNew == true
                                  join u in ctx.Users on ti.InviterId equals u.IdUser
                                  join t in ctx.Team on ti.TeamId equals t.TeamId
                                  orderby ti.CreatedAt descending
                                  select new
                                  {
                                      InvitationEntity = ti,
                                      ti.InvitationId,
                                      ti.CreatedAt,
                                      InviterName = u.Name,
                                      TeamName = t.Name
                                  })
                                 .ToList();

                // 2) Вычитываем «сырые» данные по новым назначениям
                var assignmentsRaw = (from ta in ctx.TaskAssignee
                                      where ta.UserId == userId
                                            && ta.IsNew == true
                                      join task in ctx.Task on ta.TaskId equals task.IdTask
                                      join creator in ctx.Users on task.CreatorId equals creator.IdUser
                                      orderby ta.TaskAssigneeId descending
                                      select new
                                      {
                                          AssigneeEntity = ta,
                                          ta.TaskAssigneeId,
                                          ta.AssignedAt,
                                          CreatorName = creator.Name,
                                          TaskTitle = task.Title
                                      })
                                     .ToList();

                // 3) Формируем VM в памяти (LINQ-to-Objects)
                var invites = invitesRaw
                    .Select(x => new NotificationViewModel
                    {
                        Id = x.InvitationId,
                        Type = NotificationType.TeamInvitation,
                        Title = "Приглашение в команду",
                        Message = $"{x.InviterName} пригласил вас в «{x.TeamName}»",
                        CreatedAt = x.CreatedAt ?? DateTime.Now,
                        Payload = x.InvitationEntity
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
                        Payload = x.AssigneeEntity
                    })
                    .ToList();

                // 4) Объединяем и сортируем все уведомления
                allNotifications = invites
                    .Concat(assignments)
                    .OrderByDescending(n => n.CreatedAt)
                    .ToList();

                // 5) Сбрасываем маркер IsNew = 0L для всех показанных уведомлений
                foreach (var vm in invites)
                {
                    var ti = vm.Payload as TeamInvitation;
                    if (ti != null) ti.IsNew = false;
                }

                foreach (var vm in assignments)
                {
                    var ta = vm.Payload as TaskAssignee;
                    if (ta != null) ta.IsNew = false;
                }

                ctx.SaveChanges();
            }

            // 6) Отображаем список и пересчитываем бейдж
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