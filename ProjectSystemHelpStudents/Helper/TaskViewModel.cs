using ProjectSystemHelpStudents.Helper;
using System;
using System.Collections.Generic;
using System.ComponentModel;

namespace ProjectSystemHelpStudents.Helper
{
    public class TaskViewModel : INotifyPropertyChanged
    {
        private int _priorityId;
        private string _priorityName;
        private int _projectId;
        private string _projectName;
        private string _title;
        private string _description;
        private DateTime _endDate;
        private bool _isCompleted;
        private string _Status;
        public string EndDateFormatted { get; set; }

        public int IdTask { get; set; }
        public string Status
        {
            get => _Status;
            set
            {
                if (_Status != value)
                {
                    _Status = value;
                    OnPropertyChanged(nameof(Status));
                }
            }
        }
        public int PriorityId
        {
            get => _priorityId;
            set
            {
                if (_priorityId != value)
                {
                    _priorityId = value;
                    OnPropertyChanged(nameof(PriorityId));
                }
            }
        }

        public string PriorityName
        {
            get => _priorityName;
            set
            {
                if (_priorityName != value)
                {
                    _priorityName = value;
                    OnPropertyChanged(nameof(PriorityName));
                }
            }
        }

        public int ProjectId
        {
            get => _projectId;
            set
            {
                if (_projectId != value)
                {
                    _projectId = value;
                    OnPropertyChanged(nameof(ProjectId));
                }
            }
        }

        public string ProjectName
        {
            get => _projectName;
            set
            {
                if (_projectName != value)
                {
                    _projectName = value;
                    OnPropertyChanged(nameof(ProjectName));
                }
            }
        }

        public string Title
        {
            get => _title;
            set
            {
                if (_title != value)
                {
                    _title = value;
                    OnPropertyChanged(nameof(Title));
                }
            }
        }

        public string Description
        {
            get => _description;
            set
            {
                if (_description != value)
                {
                    _description = value;
                    OnPropertyChanged(nameof(Description));
                }
            }
        }

        public DateTime EndDate
        {
            get => _endDate;
            set
            {
                if (_endDate != value)
                {
                    _endDate = value;
                    OnPropertyChanged(nameof(EndDate));
                }
            }
        }

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
