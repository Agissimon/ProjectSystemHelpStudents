using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ProjectSystemHelpStudents.Helper
{
    public class ProjectViewModel
    {
        public int ProjectId { get; set; }
        public string Name { get; set; }
        public string Icon { get; set; }
        public bool IsDetached { get; set; }
        public int? TeamId { get; set; }
        public virtual Team Team { get; set; }
        public object Description { get; set; }
        public DateTime? StartDate { get; set; }
        public DateTime? EndDate { get; set; }
        public string TeamName { get; set; }
        public bool IsCompleted { get; set; }
    }
}
