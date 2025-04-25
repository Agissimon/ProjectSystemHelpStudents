using ProjectSystemHelpStudents.Helper;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Controls;

namespace ProjectSystemHelpStudents.ViewModels
{
    public class TaskGroupViewModel
    {
        public string DateHeader { get; set; }
        public ObservableCollection<TaskViewModel> Tasks { get; set; }
        public Expander Expander { get; set; }
        public TaskGroupViewModel()
        {
            Tasks = new ObservableCollection<TaskViewModel>();
        }
    }
}
