using System;
using System.Collections.Generic;
using System.Linq;

namespace ProjectSystemHelpStudents.Helper
{
    public static class UserSettingsHelper
    {
        private const string Separator = ",";

        public static List<int> GetDetachedProjects()
        {
            var saved = Properties.Settings.Default.DetachedProjects;
            var projects = string.IsNullOrEmpty(saved)
                ? new List<int>()
                : saved.Split(new[] { Separator }, StringSplitOptions.RemoveEmptyEntries)
                       .Select(s => int.Parse(s.Trim()))
                       .ToList();
            Console.WriteLine($"Избранные проекты: {string.Join(", ", projects)}");
            return projects;
        }

        public static void SaveDetachedProjects(List<int> projectIds)
        {
            Properties.Settings.Default.DetachedProjects = string.Join(Separator, projectIds);
            Properties.Settings.Default.Save();
        }

        public static bool IsDetached(int projectId)
        {
            return GetDetachedProjects().Contains(projectId);
        }

        public static void AddDetachedProject(int projectId)
        {
            var ids = GetDetachedProjects();
            if (!ids.Contains(projectId))
            {
                ids.Add(projectId);
                SaveDetachedProjects(ids);
                Console.WriteLine($"Проект {projectId} добавлен в избранное.");
            }
            else
            {
                Console.WriteLine($"Проект {projectId} уже в избранном.");
            }
        }

        public static void RemoveDetachedProject(int projectId)
        {
            var ids = GetDetachedProjects();
            if (ids.Remove(projectId))
            {
                SaveDetachedProjects(ids);
                Console.WriteLine($"Проект {projectId} удален из избранного.");
            }
            else
            {
                Console.WriteLine($"Проект {projectId} не найден в избранном.");
            }
        }

    }
}
