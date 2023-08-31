using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace UntisDesktop.ViewModels;

internal interface IWindowViewModel
{
    public DelegateCommand ReloadOfflineCommand { get; }

    public string ErrorBoxContent { get ; set; }

    public bool IsOffline { get; set; }
}