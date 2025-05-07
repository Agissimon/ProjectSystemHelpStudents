using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Media;

namespace ProjectSystemHelpStudents.Helper
{
    public class LabelViewModel
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public bool IsSelected { get; set; }
        public string HexColor { get; set; }

        public Brush BackgroundBrush =>
        !string.IsNullOrWhiteSpace(HexColor)
            ? (Brush)new BrushConverter().ConvertFromString(HexColor)
            : Brushes.Gray;
    }
}
