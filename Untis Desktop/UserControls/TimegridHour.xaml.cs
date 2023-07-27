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
using WebUntisAPI.Client.Models;
using WebUntis = WebUntisAPI.Client.Models;

namespace UntisDesktop.UserControls;
/// <summary>
/// Interaktionslogik für TimegridHour.xaml
/// </summary>
public partial class TimegridHour : UserControl
{
    public string HourName { get; }

    public string Length { get; }

    public TimegridHour(WebUntis.SchoolHour hour)
    {
        HourName = hour.Name;
        Length = $"{hour.StartTime:t}-{hour.EndTime:t}";

        InitializeComponent();
    }
}
