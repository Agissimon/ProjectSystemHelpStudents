using System.Linq;
using System.Windows.Media;
using ProjectSystemHelpStudents.ViewModels;

namespace ProjectSystemHelpStudents.Helper
{
    public static class TaskMarkerHelper
    {
        /// <summary>
        /// Возвращает кисть для маркера задачи: сначала из первой выбранной метки, 
        /// иначе — по приоритету, иначе — серый.
        /// </summary>
        public static Brush GetMarkerBrush(TaskViewModel task)
        {
            var lbl = task.AvailableLabels?
                          .FirstOrDefault(l => l.IsSelected);
            if (lbl != null)
                return lbl.BackgroundBrush;

            switch (task.PriorityId)
            {
                case 3: return Brushes.Red;
                case 2: return Brushes.Orange;
                case 1: return Brushes.Green;
                default: return Brushes.Gray;
            }
        }
    }
}
