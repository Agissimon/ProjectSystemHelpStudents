using System.Windows.Controls;
using ProjectSystemHelpStudents.UsersContent;
using ProjectSystemHelpStudents.Views.AdminPages;

namespace ProjectSystemHelpStudents.Helper
{
    internal static class FrmClass
    {
        public static Frame frmAuth;
        public static Frame frmContentUser;
        public static Frame frmContentAdmin;
        public static Frame frmStackPanelButton;
        public static Frame frmReg;

        public static void NavigateTo(Page contentPage)
        {
            if (frmContentUser != null)
            {
                frmContentUser.Content = contentPage;
            }

            if (frmStackPanelButton != null && !(frmStackPanelButton.Content is StackPanelButtonPage))
            {
                frmStackPanelButton.Content = new StackPanelButtonPage();
            }
        }

        public static void NavigateAdmin(Page page)
        {
            if (frmContentAdmin != null)
            {
                frmContentAdmin.Content = page;
            }

            if (frmStackPanelButton != null
                && !(frmStackPanelButton.Content is AdminNavigationPage))
            {
                frmStackPanelButton.Content = new AdminNavigationPage();
            }
        }
    }
}
