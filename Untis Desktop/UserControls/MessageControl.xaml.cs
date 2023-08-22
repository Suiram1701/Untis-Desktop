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

public partial class MessageControl : UserControl
{
    public static DateTime PlaceholderTime { get => new(2023, 12, 14); }

    public MessagePreview Message { get; set; }

    public string DisplayedName { get => Message.Sender?.DisplayName ?? Message.Recipients.Select(r => r.DisplayName).FirstOrDefault("Err"); }

    public string DisplayedProfileImg
    {
        get
        {
            StringBuilder sb = new();
            foreach (char c in DisplayedName.Split(' ').Select(s => s[0]).Take(2))
                sb.Append(c);
            return sb.ToString();
        }
    }

    public MessageControl(MessagePreview message)
    {
        Message = message;
        InitializeComponent();
    }

    private void Delete_Click(object sender, RoutedEventArgs e)
    {
    }

    private void Reply_Click(object sender, RoutedEventArgs e)
    {

    }
}