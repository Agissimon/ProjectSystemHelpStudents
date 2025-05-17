using System;
using System.Data.Entity;
using System.Linq;

namespace ProjectSystemHelpStudents.Helper
{
    public static class FilterParser
    {
        /// <summary>
        /// Применяет Todoist‑похожие токены к запросу задач.
        /// </summary>
        public static IQueryable<Task> Apply(this IQueryable<Task> source, string query)
        {
            if (string.IsNullOrWhiteSpace(query))
                return source;

            // Токены: lower-case, без пустых
            var tokens = query
                .Split(new[] { ' ' }, StringSplitOptions.RemoveEmptyEntries)
                .Select(t => t.Trim().ToLower())
                .ToArray();

            var today = DateTime.Today;

            foreach (var tok in tokens)
            {
                if (tok == "today")
                {
                    source = source.Where(t =>
                        DbFunctions.TruncateTime(t.EndDate) == DbFunctions.TruncateTime(today));
                }
                else if (tok == "overdue")
                {
                    source = source.Where(t =>
                        DbFunctions.TruncateTime(t.EndDate) < DbFunctions.TruncateTime(today));
                }
                else if (tok.StartsWith("priority"))
                {
                    // priority2 или priority 2
                    var numPart = tok.Substring("priority".Length);
                    if (int.TryParse(numPart, out int p))
                        source = source.Where(t => t.PriorityId == p);
                }
                else if (tok.StartsWith("@"))
                {
                    // @userId или @123
                    var idPart = tok.Substring(1);
                    if (int.TryParse(idPart, out int uid))
                    {
                        source = source.Where(t =>
                            t.TaskAssignee.Any(ta => ta.UserId == uid));
                    }
                }
                else if (tok.StartsWith("#"))
                {
                    // #labelId или #123
                    var idPart = tok.Substring(1);
                    if (int.TryParse(idPart, out int lid))
                    {
                        source = source.Where(t =>
                            t.TaskLabels.Any(tl => tl.LabelId == lid));
                    }
                    else
                    {
                        // или по названию метки
                        var name = idPart;
                        source = source.Where(t =>
                            t.TaskLabels.Any(tl => tl.Labels.Name.ToLower() == name));
                    }
                }
                else
                {
                    // общий поиск по Title
                    source = source.Where(t =>
                        t.Title.ToLower().Contains(tok));
                }
            }

            return source;
        }
    }
}