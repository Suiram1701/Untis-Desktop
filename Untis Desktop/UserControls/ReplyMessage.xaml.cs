using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
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
using WebUntisAPI.Client.Models.Messages;

namespace UntisDesktop.UserControls;

public partial class ReplyMessage : UserControl
{
    public Message Message { get; set; }

    public bool HasAttachments { get => Message.Attachments.Any(); }

    public static readonly DependencyProperty RecipientTypeProperty = DependencyProperty.Register("RecipientType", typeof(string), typeof(ReplyMessage), new(LangHelper.GetString("MessageWindow.R")));
    public string RecipientType
    {
        get => (string)GetValue(RecipientTypeProperty);
        set => SetValue(RecipientTypeProperty, value);
    }

    public ReplyMessage(Message message)
    {
        Message = message;
        InitializeComponent();

        RenderRecipients();
        RenderAttachments();
    }

    private void RenderRecipients()
    {
        // Display recipients or sender
        if (Message.Sender is null)
        {
            foreach (MessagePerson person in Message.Recipients)
                Recipients.Children.Add(new RecipientControl(person, false));
        }
        else
        {
            RecipientType = LangHelper.GetString("MessageWindow.S");
            Recipients.Children.Add(new RecipientControl(Message.Sender, false));
        }
    }

    private void RenderAttachments()
    {
        foreach (Attachment attachment in Message.Attachments)
            Attachments.Children.Add(new AttachmentControl(attachment));
    }
}
