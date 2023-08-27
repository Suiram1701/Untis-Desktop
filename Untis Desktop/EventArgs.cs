using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;

namespace UntisDesktop;

public sealed class DeletionEventArgs : RoutedEventArgs
{
    public int DeletedId { get; set; }

    public DeletionEventArgs(int deletedId, RoutedEvent routedEvent) : base(routedEvent)
    {
        DeletedId = deletedId;
    }
}
