using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ProjectSystemHelpStudents.Helper
{
    public class TaskViewModel
    {
        private bool _isCompleted;

        public string Title { get; set; }
        public string Description { get; set; }
        public string Status { get; set; }
        public DateTime EndDate{ get; set; }
        public string EndDateFormatted { get; set; }
        public bool IsCompleted
        {
            get => _isCompleted;
            set
            {
                if (_isCompleted != value)
                {
                    _isCompleted = value;
                    OnPropertyChanged(nameof(IsCompleted));
                }
            }
        }

        public event PropertyChangedEventHandler PropertyChanged;

        protected void OnPropertyChanged(string propertyName)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }

    public class TaskGroupViewModel
    {
        public string DateHeader { get; set; }
        public List<TaskViewModel> Tasks { get; set; }
    }
}
