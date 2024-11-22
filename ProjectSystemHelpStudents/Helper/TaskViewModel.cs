using ProjectSystemHelpStudents.Helper;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;

namespace ProjectSystemHelpStudents.Helper
{
    public class TaskViewModel : INotifyPropertyChanged
    {
        private int _priorityId;
        private int _projectId;
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
        public ObservableCollection<TaskViewModel> Tasks { get; set; }

        public TaskGroupViewModel()
        {
            Tasks = new ObservableCollection<TaskViewModel>();
        }
    }

}
