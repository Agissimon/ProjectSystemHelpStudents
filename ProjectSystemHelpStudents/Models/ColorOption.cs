using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Media;

namespace ProjectSystemHelpStudents.Models
{
    public class ColorOption
    {
        public string Name { get; set; }
        public string Hex { get; set; }
        public SolidColorBrush Brush => new SolidColorBrush((Color)ColorConverter.ConvertFromString(Hex));
    }
}
