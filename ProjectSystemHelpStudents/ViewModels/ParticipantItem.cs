using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ProjectSystemHelpStudents.ViewModels
{
    public class ParticipantItem
    {
        public int UserId { get; set; }
        public string Name { get; set; }
        public string Status { get; set; }
        public bool IsInvitation { get; set; }
    }
}
