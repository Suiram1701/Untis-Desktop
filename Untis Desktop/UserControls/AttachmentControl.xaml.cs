using System;
using System.Collections.Generic;
using System.IO;
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

namespace UntisDesktop.UserControls;

/// <summary>
/// A user added attachment
/// </summary>
public partial class AttachmentControl : UserControl
{
    public MemoryStream Stream = new();

    public string FileName { get; set; }

    public static readonly RoutedEvent DeleteEvent = EventManager.RegisterRoutedEvent("OnDeletion", RoutingStrategy.Bubble, typeof(RoutedEventHandler), typeof(AttachmentControl));
    public event RoutedEventHandler DeleteEventHandler
    {
        add => AddHandler(DeleteEvent, value);
        remove => RemoveHandler(DeleteEvent, value);
    }

    public AttachmentControl(string fileName, Stream content)
    {
        content.CopyTo(Stream);
        FileName = fileName;

        InitializeComponent();
    }

    private void Delete_Click(object sender, RoutedEventArgs e)
    {
        RaiseEvent(new(DeleteEvent));
    }
}