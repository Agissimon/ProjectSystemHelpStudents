﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Remoting.Contexts;
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
using ProjectSystemHelpStudents.Helper;
using ProjectSystemHelpStudents.UsersContent;

namespace ProjectSystemHelpStudents
{
    /// <summary>
    /// Логика взаимодействия для AddTaskWindow.xaml
    /// </summary>
    public partial class AddTaskWindow : System.Windows.Window
    {
        public AddTaskWindow()
        {
            InitializeComponent();
        }

        private void SaveTask_Click(object sender, RoutedEventArgs e)
        {
            // Считываем данные из полей
            string title = txtTitle.Text;
            string description = txtDescription.Text;
            DateTime? endDate = dpEndDate.SelectedDate;

            string categoryName = (cmbCategory.SelectedItem as ComboBoxItem)?.Content.ToString();
            string priorityName = (cmbPriority.SelectedItem as ComboBoxItem)?.Content.ToString();

            // Проверка, что все обязательные поля заполнены
            if (string.IsNullOrWhiteSpace(title))
            {
                MessageBox.Show("Пожалуйста, введите название задачи.", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
                return;
            }

            if (endDate == null)
            {
                MessageBox.Show("Пожалуйста, выберите дату завершения.", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
                return;
            }
            try
            {
                // Находим категорию и приоритет по имени
                var category = DBClass.entities.Category.FirstOrDefault(c => c.Name == categoryName);
                var priority = DBClass.entities.Priority.FirstOrDefault(p => p.Name == priorityName);

                // Создаем новую задачу
                var newTask = new Task
                {
                    Title = title,
                    Description = description,
                    EndDate = endDate.Value,
                    StatusId = DBClass.entities.Status.FirstOrDefault(s => s.Name == "Не завершено")?.StatusId ?? 1, // Устанавливаем статус по умолчанию
                    IdUser = UserSession.IdUser,
                    CategoryId = (int)(category?.CategoryId),
                    PriorityId = (int)(priority?.PriorityId)
                };

                // Добавляем задачу в базу данных
                DBClass.entities.Task.Add(newTask);
                DBClass.entities.SaveChanges();
                MessageBox.Show("Задача успешно добавлена.", "Успех", MessageBoxButton.OK, MessageBoxImage.Information);

                // Закрываем окно после сохранения
                this.Close();
            }
            catch (Exception ex)
            {
                MessageBox.Show("Ошибка при добавлении задачи: " + ex.Message, "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }
    }
}
