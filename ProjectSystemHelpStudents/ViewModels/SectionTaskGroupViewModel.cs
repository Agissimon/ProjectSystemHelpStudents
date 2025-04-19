using ProjectSystemHelpStudents.Helper;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ProjectSystemHelpStudents.ViewModels
{
    public class SectionTaskGroupViewModel
    {
        public int SectionId { get; set; }
        public string SectionName { get; set; }
        public List<TaskViewModel> Tasks { get; set; }
    }
}
