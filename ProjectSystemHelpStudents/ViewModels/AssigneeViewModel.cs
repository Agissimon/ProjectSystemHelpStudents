using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;

namespace ProjectSystemHelpStudents.ViewModels
{
    public class AssigneeViewModel : INotifyPropertyChanged
    {
        public int UserId { get; set; }
        public string Name { get; set; }
        public bool IsCreator { get; set; }
        private bool _isAssigned;
        public bool IsAssigned
        {
            get => _isAssigned;
            set { _isAssigned = value; OnPropertyChanged(); }
        }
        public event PropertyChangedEventHandler PropertyChanged;
        protected void OnPropertyChanged([CallerMemberName] string n = null) =>
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(n));
    }
}
