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
    /// Логика взаимодействия для UserEditControl.xaml
    /// </summary>
    public partial class UserEditControl : UserControl
    {
        private Users _user;
        private TaskManagementEntities1 _ctx;

        public UserEditControl(Users user, TaskManagementEntities1 ctx)
        {
            InitializeComponent();
            _user = user;
            _ctx = ctx;

            NameBox.Text = user.Name;
            SurnameBox.Text = user.Surname;
            LoginBox.Text = user.Login;
            EmailBox.Text = user.Mail;
            IsAdminBox.IsChecked = user.RoleUser == 1;
        }

        private void Save_Click(object sender, RoutedEventArgs e)
        {
            _user.Name = NameBox.Text;
            _user.Surname = SurnameBox.Text;
            _user.Login = LoginBox.Text;
            _user.Mail = EmailBox.Text;
            _user.RoleUser = IsAdminBox.IsChecked == true ? 1 : 2;

            _ctx.SaveChanges();
            MessageBox.Show("Пользователь обновлён", "Успех", MessageBoxButton.OK, MessageBoxImage.Information);

            Window.GetWindow(this)?.Close();
        }
    }
}
