using System.Linq;

namespace ProjectSystemHelpStudents.Helper
{
    public static class TaskExtensions
    {
        /// <summary>
        /// Возвращает только те задачи, где пользователь — создатель или один из исполнителей.
        /// </summary>
        public static IQueryable<Task> ForUser(this IQueryable<Task> tasks, int userId)
        {
            return tasks.Where(t =>
                t.CreatorId == userId
                || t.TaskAssignee.Any(ta => ta.UserId == userId)
            );
        }
    }
}
