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
using UntisDesktop.Localization;
using UntisDesktop.ViewModels;
using UntisDesktop.Views;
using WebUntisAPI.Client;

namespace UntisDesktop.UserControls;
/// <summary>
/// Interaktionslogik für TimegridDay.xaml
/// </summary>
public partial class TimegridDay : UserControl
{
    internal TimegridDayViewModel ViewModel { get => (TimegridDayViewModel)DataContext; }

    public TimegridDay(Day day)
    {
        InitializeComponent();

        ViewModel.DisplayedDay = day;
        ViewModel.Update();
    }
}
