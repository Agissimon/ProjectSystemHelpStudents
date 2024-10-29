using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ProjectSystemHelpStudents.Helper
{
    public class TaskViewModel
    {
        public string Title { get; set; }
        public string Description { get; set; }
        public string StatusTask { get; set; }
        public string EndDate { get; set; }
        public bool IsCompleted { get; set; }
    }
}
