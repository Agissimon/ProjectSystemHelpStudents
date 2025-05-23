﻿using ProjectSystemHelpStudents.Models;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;

namespace ProjectSystemHelpStudents.Views.TaskPages
{
    /// <summary>
    /// Логика взаимодействия для LabelOrFilterWindow.xaml
    /// </summary>
    public partial class LabelOrFilterWindow : Window
    {
        public string InputName { get; private set; }
        public string InputQuery { get; private set; }
        public string InputColor { get; private set; }
        public bool IsFavorite { get; private set; }

        private bool _isFilterMode;

        public ObservableCollection<ColorOption> ColorOptions { get; set; }
        public ColorOption SelectedColor { get; set; }

        public LabelOrFilterWindow(bool isFilter = false, string initialName = "", string initialValue = "", string initialColor = "")
        {
            InitializeComponent();
            _isFilterMode = isFilter;
            DataContext = this;

            ColorOptions = new ObservableCollection<ColorOption>
            {
                new ColorOption { Name = "Синий", Hex = "#0000FF" },
                new ColorOption { Name = "Виноградный", Hex = "#6B3FA0" },
                new ColorOption { Name = "Фиолетовый", Hex = "#800080" },
                new ColorOption { Name = "Лавандовый", Hex = "#E6E6FA" },
                new ColorOption { Name = "Ярко-розовый", Hex = "#FF69B4" },
                new ColorOption { Name = "Розовый", Hex = "#FFC0CB" },
                new ColorOption { Name = "Аспидно-серый", Hex = "#708090" },
                new ColorOption { Name = "Серый", Hex = "#808080" },
                new ColorOption { Name = "Тауп", Hex = "#483C32" }
            };

            NameTextBox.Text = initialName;

            SelectedColor = ColorOptions.FirstOrDefault(c => c.Hex.Equals(initialColor, StringComparison.OrdinalIgnoreCase));
            ColorComboBox.SelectedItem = SelectedColor;

            if (isFilter)
            {
                this.Title = "Редактировать фильтр";
                QueryPanel.Visibility = Visibility.Visible;
                QueryTextBox.Text = initialValue;
            }
            else
            {
                this.Title = "Редактировать метку";
                QueryPanel.Visibility = Visibility.Collapsed;
            }
        }

        private void Add_Click(object sender, RoutedEventArgs e)
        {
            var name = NameTextBox.Text?.Trim();
            if (string.IsNullOrEmpty(name))
            {
                MessageBox.Show("Имя не может быть пустым.", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }
            if (name.Contains(" "))
            {
                MessageBox.Show("Имя не должно содержать пробелов.", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            if (SelectedColor == null)
            {
                MessageBox.Show("Выберите цвет.", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            if (_isFilterMode)
            {
                var query = QueryTextBox.Text?.Trim();
                if (string.IsNullOrWhiteSpace(query))
                {
                    MessageBox.Show("Запрос не может быть пустым.", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Warning);
                    return;
                }
                InputQuery = query;
            }
            else
            {
                InputQuery = null;
            }

            InputName = name;
            InputColor = SelectedColor.Hex;

            DialogResult = true;
        }


        private void Cancel_Click(object sender, RoutedEventArgs e)
        {
            DialogResult = false;
            Close();
        }
    }
}