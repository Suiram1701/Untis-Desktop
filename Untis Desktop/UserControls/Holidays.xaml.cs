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

namespace UntisDesktop.UserControls
{
    /// <summary>
    /// Interaktionslogik für Holidays.xaml
    /// </summary>
    public partial class Holidays : UserControl
    {
        public string HolidayName { get; }

        public Holidays(string name)
        {
            HolidayName = name;
            InitializeComponent();
        }
    }
}
