using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using WebUntisAPI.Client.Models.Messages;
using Image = SixLabors.ImageSharp.Image;
using SixLabors.ImageSharp.Processing;
using System.Windows.Controls;
using SixLabors.ImageSharp.Formats;
using SixLabors.ImageSharp;
using System.Windows.Threading;
using System.Windows.Controls.Primitives;
using UntisDesktop.Localization;
using SixLabors.Fonts.Tables.AdvancedTypographic;

namespace UntisDesktop.UserControls;

public partial class Recipient : UserControl
{
    public MessagePerson MessagePerson { get; }

    public static readonly DependencyProperty IsSelectedProperty = DependencyProperty.Register("IsSelected", typeof(bool), typeof(Recipient), new(false));
    public bool IsSelected
    {
        get => (bool)GetValue(IsSelectedProperty);
        set
        {
            if (IsSelected != value)
            {
                SetValue(IsSelectedProperty, value);
                RaiseEvent(new UpdateEventArgs(MessagePerson.Id, ToggleSelectEvent));
            }
        }
    }

    public static readonly RoutedEvent ToggleSelectEvent = EventManager.RegisterRoutedEvent("OnToggleSelect", RoutingStrategy.Bubble, typeof(EventHandler<UpdateEventArgs>), typeof(Recipient));
    public event EventHandler<UpdateEventArgs> ToggleSelectEventHandler
    {
        add => AddHandler(ToggleSelectEvent, value);
        remove => RemoveHandler(ToggleSelectEvent, value);
    }

    public Recipient(MessagePerson recipient, bool isSelected)
    {
        MessagePerson = recipient;

        InitializeComponent();
        SetValue(IsSelectedProperty, isSelected);

        // Display profile image
        Dispatcher.Invoke(async () =>
        {
            using Image image = await App.Client!.GetMessagePersonProfileImageAsync(recipient).ConfigureAwait(true);
            MemoryStream imageStream = new();
            await image.SaveAsPngAsync(imageStream).ConfigureAwait(true);

            BitmapImage bmp = new();
            bmp.BeginInit();
            bmp.StreamSource = imageStream;
            bmp.EndInit();

            ProfileImage.Source = bmp;
        });

        // Display tags
        if (recipient.Role is not null)
            AddTag(LangHelper.GetString("RecipientDialog.RT." + recipient.Role));

        if (recipient.ClassName is not null)
            AddTag(recipient.ClassName);
        
        foreach (string tag in recipient.Tags)
            AddTag(tag);
    }

    private void AddTag(string tag)
    {
        Tags.Children.Add(new Label
        {
            Content = tag,
            Template = (ControlTemplate)Application.Current.FindResource("TagTemplate")
        });
    }

    private void UserControl_MouseDown(object sender, MouseButtonEventArgs e)
    {
        if (e.LeftButton != MouseButtonState.Pressed)
            return;

        IsSelected = !IsSelected;

        e.Handled = true;
    }
}