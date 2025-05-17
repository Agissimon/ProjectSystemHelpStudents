using System.ComponentModel;
using System.Windows.Media;

namespace ProjectSystemHelpStudents.Helper
{
    public class LabelViewModel : INotifyPropertyChanged
    {
        public int Id { get; set; }
        public string Name { get; set; }
        private bool _isSelected;
        public bool IsSelected
        {
            get => _isSelected;
            set
            {
                if (_isSelected == value) return;
                _isSelected = value;
                OnPropertyChanged(nameof(IsSelected));
                OnPropertyChanged(nameof(DisplayToken));
            }
        }
        public string HexColor { get; set; }

        public Brush BackgroundBrush =>
            !string.IsNullOrWhiteSpace(HexColor)
                ? (Brush)new BrushConverter().ConvertFromString(HexColor)
                : Brushes.Gray;

        // токен выводиться в лейбл в списке
        public string DisplayToken => Name;

        public event PropertyChangedEventHandler PropertyChanged;
        protected void OnPropertyChanged(string prop) =>
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(prop));
    }
}
