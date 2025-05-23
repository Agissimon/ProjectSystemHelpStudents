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

namespace ProjectSystemHelpStudents.Views.AdminPages
{
    /// <summary>
    /// Логика взаимодействия для PriorityEditControl.xaml
    /// </summary>
    public partial class PriorityEditControl : UserControl
    {
        private readonly Priority _priority;
        private readonly TaskManagementEntities1 _ctx;

        public PriorityEditControl(Priority priority, TaskManagementEntities1 ctx)
        {
            InitializeComponent();
            _priority = priority;
            _ctx = ctx;

            NameBox.Text = _priority.Name;
        }

        private void Save_Click(object sender, RoutedEventArgs e)
        {
            _priority.Name = NameBox.Text.Trim();
            _ctx.SaveChanges();
            Window.GetWindow(this)?.Close();
        }
    }
}
