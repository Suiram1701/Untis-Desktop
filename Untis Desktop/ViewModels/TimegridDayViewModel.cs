using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UntisDesktop.Localization;
using UntisDesktop.Views;
using WebUntisAPI.Client;

namespace UntisDesktop.ViewModels;

internal class TimegridDayViewModel : ViewModelBase
{
    public string LocalizedDayName { get => LangHelper.GetString("MainWindow.Inf." + Date.DayOfWeek.ToString()); }

    public DateTime Date { get => MainWindow.SelectedWeek.AddDays((double)DisplayedDay - 1); }

    public Day DisplayedDay { get; set; }

    public void Update()
    {
        RaisePropertyChanged(nameof(Date));
        RaisePropertyChanged(nameof(LocalizedDayName));
    }
}
