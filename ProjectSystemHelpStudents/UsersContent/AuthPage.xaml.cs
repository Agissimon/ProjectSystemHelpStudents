﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.RightsManagement;
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
using ProjectSystemHelpStudents.Helper;

namespace ProjectSystemHelpStudents.UsersContent
{
    /// <summary>
    /// Логика взаимодействия для AuthPage.xaml
    /// </summary>
    public partial class AuthPage : Page
    {
        int logCount = 0;
        public static int idUser;
        public static string nameUser;
        public AuthPage()
        {
            InitializeComponent();
        }

        private void btnLogIn_Click(object sender, RoutedEventArgs e, User userLog)
        {


            if (string.IsNullOrWhiteSpace(txbLogin.Text))
            {
                MessageBox.Show("Вы не ввели логин пользователя");
                logCount++;
            }
            else if (string.IsNullOrWhiteSpace(psbPassword.Password))
            {
                MessageBox.Show("Вы не ввели пароль пользователя");
                logCount++;
            }
            else
            {
                var user = DBClass.entities.User.Where(i => i.Login == txbLogin.Text && i.Password == psbPassword.Password).FirstOrDefault();

                if (logCount == 4)
                {
                    MessageBox.Show("Вы превысили лимит попыток входа. Аккаунт заблокирован.");
                    // Добавить капчу что-ли
                }
                else
                {
                    if (user == null)
                    {
                        MessageBox.Show("Неверный логин или пароль");
                        logCount++;
                    }
                    else
                    {
                       user = userLog;
                        if (user.RoleUser == 1)
                        {
                            //MessageBox.Show("Здравствуйте, " + user.Name);
                            //AdminPage adminPage = new AdminPage();
                            //FrmClass.frmContentUser.Content = adminPage;

                        }
                        else if (user.RoleUser == 2)
                        {
                            MessageBox.Show("Здравствуйте, " + user.Name);
                            idUser = user.IdUser;
                            nameUser = user.Name;
                            UserPage content = new UserPage();
                            FrmClass.frmContentUser.Content = content;
                        }
                    }
                }
            }
        }

        private void btnReg_Click(object sender, RoutedEventArgs e)
        {
            RegPage reg = new RegPage();
            FrmClass.frmAuth.Content = reg;
        }

        private void chkShowPassword_Checked(object sender, RoutedEventArgs e)
        {
            if (txbPassword != null)
            {
                txbPassword.Text = psbPassword.Password;
                psbPassword.Visibility = Visibility.Collapsed;
                txbPassword.Visibility = Visibility.Visible;
            }
        }

        private void chkShowPassword_Unchecked(object sender, RoutedEventArgs e)
        {
            if (txbPassword != null)
            {
                psbPassword.Password = txbPassword.Text;
                psbPassword.Visibility = Visibility.Visible;
                txbPassword.Visibility = Visibility.Collapsed;
            }
        }

        private void btnFogot_Click(object sender, RoutedEventArgs e)
        {
            FogotPassWindow fog = new FogotPassWindow();
            fog.Show();
        }
    }
}
