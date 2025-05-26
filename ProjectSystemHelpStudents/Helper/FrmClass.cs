using System.Windows.Controls;
using ProjectSystemHelpStudents.UsersContent;

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
            // Если хотим навигацию внутри UserContent
            if (frmContentUser != null)
            {
                frmContentUser.Content = contentPage;
            }

            // Если нужно всегда показывать панель кнопок
            if (frmStackPanelButton != null && !(frmStackPanelButton.Content is StackPanelButtonPage))
            {
                frmStackPanelButton.Content = new StackPanelButtonPage();
            }
        }
    }
}
