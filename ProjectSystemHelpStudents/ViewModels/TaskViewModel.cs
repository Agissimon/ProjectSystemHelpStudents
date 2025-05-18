using System;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Windows.Media;
using ProjectSystemHelpStudents.Helper;

namespace ProjectSystemHelpStudents.ViewModels
{
    public class TaskViewModel : INotifyPropertyChanged
    {
        private int _idTask;
        private string _title;
        private string _description;
        private DateTime _endDate;
        private bool _isCompleted;
        private string _status;
        private int _projectId;
        private int _priorityId;
        private DateTime? _reminderDate;
        private ObservableCollection<LabelViewModel> _availableLabels;

        public TaskViewModel()
        {
            Assignees = new ObservableCollection<AssigneeViewModel>();
            AvailableLabels = new ObservableCollection<LabelViewModel>();
        }

        public int IdTask
        {
            get => _idTask;
            set { _idTask = value; OnPropertyChanged(nameof(IdTask)); }
        }
        public string Title
        {
            get => _title;
            set { _title = value; OnPropertyChanged(nameof(Title)); }
        }
        public string Description
        {
            get => _description;
            set { _description = value; OnPropertyChanged(nameof(Description)); }
        }
        public DateTime EndDate
        {
            get => _endDate;
            set { _endDate = value; OnPropertyChanged(nameof(EndDate)); }
        }
        public string EndDateFormatted { get; set; }
        public bool IsCompleted
        {
            get => _isCompleted;
            set { _isCompleted = value; OnPropertyChanged(nameof(IsCompleted)); }
        }
        public string Status
        {
            get => _status;
            set { _status = value; OnPropertyChanged(nameof(Status)); }
        }
        public int ProjectId
        {
            get => _projectId;
            set { _projectId = value; OnPropertyChanged(nameof(ProjectId)); }
        }

        public int PriorityId
        {
            get => _priorityId;
            set
            {
                _priorityId = value;
                OnPropertyChanged(nameof(PriorityId));
                OnPropertyChanged(nameof(PriorityColor));
                OnPropertyChanged(nameof(MarkerBrush));
            }
        }

        public Brush PriorityColor
        {
            get
            {
                switch (PriorityId)
                {
                    case 3: return Brushes.Red;
                    case 2: return Brushes.Orange;
                    case 1: return Brushes.Green;
                    default: return Brushes.Gray;
                }
            }
        }

        public DateTime? ReminderDate
        {
            get => _reminderDate;
            set { _reminderDate = value; OnPropertyChanged(nameof(ReminderDate)); }
        }

        public ObservableCollection<AssigneeViewModel> Assignees { get; }

        public ObservableCollection<LabelViewModel> AvailableLabels
        {
            get => _availableLabels;
            set
            {
                if (_availableLabels != null)
                {
                    foreach (var lbl in _availableLabels)
                        lbl.PropertyChanged -= Label_PropertyChanged;
                    _availableLabels.CollectionChanged -= Labels_CollectionChanged;
                }
                _availableLabels = value;
                if (_availableLabels != null)
                {
                    foreach (var lbl in _availableLabels)
                        lbl.PropertyChanged += Label_PropertyChanged;
                    _availableLabels.CollectionChanged += Labels_CollectionChanged;
                }
                OnPropertyChanged(nameof(AvailableLabels));
                OnPropertyChanged(nameof(LabelsFormatted));
                OnPropertyChanged(nameof(MarkerBrush));
            }
        }

        private void Labels_CollectionChanged(object s, System.Collections.Specialized.NotifyCollectionChangedEventArgs e)
        {
            OnPropertyChanged(nameof(LabelsFormatted));
            OnPropertyChanged(nameof(MarkerBrush));
        }
        private void Label_PropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            if (e.PropertyName == nameof(LabelViewModel.IsSelected))
            {
                OnPropertyChanged(nameof(LabelsFormatted));
                OnPropertyChanged(nameof(MarkerBrush));
            }
        }

        public string LabelsFormatted
            => _availableLabels == null
                ? string.Empty
                : string.Join(", ", _availableLabels.Where(l => l.IsSelected).Select(l => l.Name));

        public int IdUser { get; set; }
        public object Section { get; set; }
        public int CreatorId { get; set; }

        public Brush MarkerBrush
            => TaskMarkerHelper.GetMarkerBrush(this);

        public event PropertyChangedEventHandler PropertyChanged;
        protected void OnPropertyChanged(string name)
            => PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
    }
}
