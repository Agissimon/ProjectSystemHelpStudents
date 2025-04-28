using ProjectSystemHelpStudents.Helper;
using ProjectSystemHelpStudents.Helpers;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.ComponentModel;
using System.Linq;
using System.Windows.Input;

namespace ProjectSystemHelpStudents.ViewModels
{
    public class SearchViewModel : INotifyPropertyChanged
    {
        public ObservableCollection<string> SearchHistory { get; } = new ObservableCollection<string>();
        public ObservableCollection<string> SearchResults { get; } = new ObservableCollection<string>();

        private string _query;
        public string Query
        {
            get => _query;
            set
            {
                if (_query != value)
                {
                    _query = value;
                    OnPropertyChanged(nameof(Query));
                }
            }
        }

        public ICommand SearchCommand { get; }
        public ICommand HistorySelectedCommand { get; }

        public SearchViewModel()
        {
            LoadHistory();
            SearchCommand = new RelayCommand(_ => ExecuteSearch(Query));
            HistorySelectedCommand = new RelayCommand(item => ExecuteSearch(item as string));
        }

        private void ExecuteSearch(string query)
        {
            if (string.IsNullOrWhiteSpace(query))
                return;

            Query = query;

            if (!SearchHistory.Contains(query))
            {
                SearchHistory.Insert(0, query);
                if (SearchHistory.Count > 20)
                    SearchHistory.RemoveAt(SearchHistory.Count - 1);
                SaveHistory();
            }

            SearchResults.Clear();

            var taskResults = DBClass.entities.Task
                .Where(t => t.Title.Contains(query) || t.Description.Contains(query))
                .Select(t => new { t.IdTask, t.Title })
                .AsEnumerable()
                .Select(x => $"Задача: {x.Title} (ID: {x.IdTask})");

            var commentResults = DBClass.entities.Comment
                .Where(c => c.Content.Contains(query))
                .Select(c => c.Content)
                .AsEnumerable()
                .Select(content => $"Комментарий: {content}");

            var fileResults = DBClass.entities.Files
                .Where(f => f.FilePath.Contains(query))
                .Select(f => f.FilePath)
                .AsEnumerable()
                .Select(path => $"Файл: {path}");

            foreach (var result in taskResults)
                SearchResults.Add(result);

            foreach (var result in commentResults)
                SearchResults.Add(result);

            foreach (var result in fileResults)
                SearchResults.Add(result);
        }

        private void LoadHistory()
        {
            var saved = Properties.Settings.Default.SearchHistory;
            if (saved != null)
                foreach (string q in saved)
                {
                    SearchHistory.Add(q);
                }
        }

        private void SaveHistory()
        {
            var sc = new StringCollection();
            foreach (var q in SearchHistory)
                sc.Add(q);
            Properties.Settings.Default.SearchHistory = sc;
            Properties.Settings.Default.Save();
        }

        public void RemoveHistoryItem(string item)
        {
            if (SearchHistory.Remove(item))
                SaveHistory();
        }

        public void ClearHistory()
        {
            SearchHistory.Clear();
            SaveHistory();
        }

        public event PropertyChangedEventHandler PropertyChanged;
        protected void OnPropertyChanged(string propName) =>
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propName));
    }
}