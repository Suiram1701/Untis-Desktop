using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace UntisDesktop.ViewModels;

public interface IWindowViewModel
{
    public string ErrorBoxContent { get ; set; }

    public bool IsOffline { get; set; }
}