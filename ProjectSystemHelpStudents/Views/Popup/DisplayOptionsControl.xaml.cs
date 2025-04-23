using System;
using System.Windows;
using System.Windows.Controls;

namespace ProjectSystemHelpStudents.Views.Popup
{
    public partial class DisplayOptionsControl : UserControl
    {
        public event Action<string> ViewModeChanged;

        public DisplayOptionsControl()
        {
            InitializeComponent();
        }

        private void ListTab_Click(object sender, RoutedEventArgs e) => ViewModeChanged?.Invoke("List");
        private void BoardTab_Click(object sender, RoutedEventArgs e) => ViewModeChanged?.Invoke("Board");
        private void CalendarTab_Click(object sender, RoutedEventArgs e) => ViewModeChanged?.Invoke("Calendar");

        public void ShowPopup(UIElement placementTarget)
        {
            DisplayOptionsPopup.PlacementTarget = placementTarget;
            DisplayOptionsPopup.IsOpen = true;
        }

        private void SortComboBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            // логика сортировки
        }

        private void ExecutorComboBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            // логика по исполнителю
        }

        private void PriorityComboBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            // логика по приоритету
        }

        private void LabelComboBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            // логика по метке
        }
    }
}
