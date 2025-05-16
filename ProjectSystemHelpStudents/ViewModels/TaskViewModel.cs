using ProjectSystemHelpStudents.Helper;
using ProjectSystemHelpStudents.ViewModels;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;

namespace ProjectSystemHelpStudents.Helper
{
    public class TaskViewModel : INotifyPropertyChanged
    {
        public ObservableCollection<AssigneeViewModel> Assignees { get; } =
                new ObservableCollection<AssigneeViewModel>();

        public ObservableCollection<LabelViewModel> AvailableLabels { get; set; }
        public string LabelsFormatted => AvailableLabels != null
        ? string.Join(", ", AvailableLabels.Select(l => l.Name))
        : string.Empty;
        private int _priorityId;
        private int _projectId;
        private string _title;
        private string _description;
        private DateTime _endDate;
        private bool _isCompleted;
        private string _Status;
        private int _IdLabel;

        public int IdUser { get; set; }
        public int CreatorId { get; set; }
        public bool IsPinned { get; set; }
        public int? SectionId { get; set; }
        public string Section { get; set; } 

        public string EndDateFormatted { get; set; }
        public int IdTask { get; set; }

        private DateTime? _reminderDate;
        public DateTime? ReminderDate
        {
            get => _reminderDate;
            set
            {
                if (_reminderDate != value)
                {
                    _reminderDate = value;
                    OnPropertyChanged(nameof(ReminderDate));
                }
            }
        }

        public int Id
        {
            get => _IdLabel;
            set
            {
                if (_IdLabel != value)
                {
                    _IdLabel = value;
                    OnPropertyChanged(nameof(Id));
                }
            }
        }

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
        protected void OnPropertyChanged(string propertyName) =>
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
    }
}
