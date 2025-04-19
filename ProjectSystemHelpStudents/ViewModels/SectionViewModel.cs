using ProjectSystemHelpStudents.Helper;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ProjectSystemHelpStudents.ViewModels
{
    public class SectionViewModel
    {
        public int IdSection { get; set; }
        public string Name { get; set; }
        public ObservableCollection<TaskViewModel> Tasks { get; set; }
    }

}
