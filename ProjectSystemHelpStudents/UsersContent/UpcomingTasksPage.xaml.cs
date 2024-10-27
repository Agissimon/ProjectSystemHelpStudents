using ProjectSystemHelpStudents.Helper;
using System;
using System.Collections.Generic;
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
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace ProjectSystemHelpStudents.UsersContent
{
    public partial class UpcomingTasksPage : Page
    {
        public UpcomingTasksPage()
        {
            InitializeComponent();
            LoadTasks();
        }

        private void LoadTasks()
        {
            try
            {
                // Получаем данные задач из базы данных и конвертируем их в LINQ to Objects
                var tasks = DBClass.entities.Task
                    .Where(t => t.EndDate >= DateTime.Now)
                    .AsEnumerable()
                    .Select(t => new
                    {
                        t.Title,
                        t.Description,
                        t.StatusTask,
                        EndDate = t.EndDate != DateTime.MinValue ? t.EndDate.ToString("dd MMMM yyyy") : "Без срока"
                    })
                    .ToList();

                TasksListView.ItemsSource = tasks;
            }
            catch (Exception ex)
            {
                MessageBox.Show("Ошибка при загрузке задач: " + ex.Message);
            }
        }
    }
}