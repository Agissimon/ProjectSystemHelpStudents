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
using ProjectSystemHelpStudents.UsersContent;
using ProjectSystemHelpStudents.Helper;

namespace ProjectSystemHelpStudents
{
    /// <summary>
    /// Логика взаимодействия для MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            InitializeComponent();
            AuthPage authPage = new AuthPage();
            frmAuth.Content = authPage;

            FrmClass.frmAuth = frmAuth;
            FrmClass.frmReg = frmReg;
            FrmClass.frmContentUser = frmContentUser;
            FrmClass.frmContentAdmin = frmContentAdmin;
        }
    }
}
