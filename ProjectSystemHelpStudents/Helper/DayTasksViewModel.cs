using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ProjectSystemHelpStudents.Helper
{
    public class DayTasksViewModel
    {
        public DateTime Date { get; set; }
        public string DayOfWeek { get; set; }
        public List<TaskViewModel> Tasks { get; set; }
    }

}
