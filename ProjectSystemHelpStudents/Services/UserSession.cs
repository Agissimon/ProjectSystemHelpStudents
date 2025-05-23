using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ProjectSystemHelpStudents.Helper
{
    public static class UserSession
    {
        public static string NameUser { get; set; }
        public static int IdUser { get; set; }

        public static event Action<string> UserNameUpdated;
        public static event Action NotificationsChanged;

        public static void RaiseNotificationsChanged()
        {
            NotificationsChanged?.Invoke();
        }

        public static void NotifyUserNameUpdated(string newName)
        {
            NameUser = string.Empty;
            NameUser = newName;
            UserNameUpdated?.Invoke(newName);
        }
    }
}
