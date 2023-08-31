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
using WebUntisAPI.Client.Models.Messages;

namespace UntisDesktop.UserControls;

public partial class RecipientControl : UserControl
{
    public MessagePerson MessagePerson { get; set; }

    public static readonly RoutedEvent DeleteEvent = EventManager.RegisterRoutedEvent("OnDeletion", RoutingStrategy.Bubble, typeof(EventHandler<UpdateEventArgs>), typeof(Recipient));
    public event EventHandler<UpdateEventArgs> DeleteEventHandler
    {
        add => AddHandler(DeleteEvent, value);
        remove => RemoveHandler(DeleteEvent, value);
    }

    public RecipientControl(MessagePerson messagePerson)
    {
        MessagePerson = messagePerson;
        InitializeComponent();
    }

    private void Delete_Click(object sender, RoutedEventArgs e)
    {
        RaiseEvent(new UpdateEventArgs(MessagePerson.Id, DeleteEvent));
    }
}
