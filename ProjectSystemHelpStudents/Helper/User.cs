﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ProjectSystemHelpStudents.Helper
{
    public class User
    {
        public int IdUser { get; set; }
        public string Name { get; set; }
        public string Surname { get; set; }
        public string Patronymic { get; set; }
        public string Login { get; set; }
        public string Password { get; set; }
        public string Mail { get; set; }

        public string UserFIO => $"{Surname} {Name} {Patronymic}";
    }

}
