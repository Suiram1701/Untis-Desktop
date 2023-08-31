using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;

namespace UntisDesktop;

public sealed class UpdateEventArgs : RoutedEventArgs
{
    public int UpdateId { get; set; }

    public UpdateEventArgs(int updateId, RoutedEvent routedEvent) : base(routedEvent)
    {
        UpdateId = updateId;
    }
}
