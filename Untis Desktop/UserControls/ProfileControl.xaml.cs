using Data.Profiles;
using SixLabors.ImageSharp;
using System;
using System.Collections.Generic;
using System.IO;
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
using System.Windows.Threading;
using UntisDesktop.Extensions;
using UntisDesktop.ViewModels;
using UntisDesktop.Views;
using Image = SixLabors.ImageSharp.Image;

namespace UntisDesktop.UserControls;

public partial class ProfileControl : UserControl
{
    public bool IsDeleteAble { get => !ProfileFile.IsActive; }

    public string ProfileName { get => ProfileFile.User.ForeName + ' ' + ProfileFile.User.LongName; }

    public ProfileFile ProfileFile { get; }

    public static readonly DependencyProperty ProfileImageProperty = DependencyProperty.Register(
        name: "ProfileImage",
        propertyType: typeof(BitmapImage),
        ownerType: typeof(ProfileControl),
        typeMetadata: new(new BitmapImage(new($"pack://application:,,,/{typeof(MainWindow).Assembly.GetName().Name};component/Assets/person.png"))));
    public BitmapImage ProfileImage
    {
        get => (BitmapImage)GetValue(ProfileImageProperty);
        set => SetValue(ProfileImageProperty, value);
    }

    public static readonly RoutedEvent SwitchEvent = EventManager.RegisterRoutedEvent(
        name: "Switch",
        routingStrategy: RoutingStrategy.Bubble,
        handlerType: typeof(RoutedEventHandler),
        ownerType: typeof(ProfileControl));
    public event RoutedEventHandler Switch
    {
        add => AddHandler(SwitchEvent, value);
        remove => RemoveHandler(SwitchEvent, value);
    }

    public static readonly RoutedEvent DeleteEvent = EventManager.RegisterRoutedEvent(
        name: "Delete",
        routingStrategy: RoutingStrategy.Bubble,
        handlerType: typeof(RoutedEventHandler),
        ownerType: typeof(ProfileControl));
    public event RoutedEventHandler Delete
    {
        add => AddHandler(DeleteEvent, value);
        remove => RemoveHandler(DeleteEvent, value);
    }

    public ProfileControl(ProfileFile profile)
    {
        ProfileFile = profile;
        Initialized += Control_InitializedAsync;
        InitializeComponent();
    }

    private async void Control_InitializedAsync(object? sender, EventArgs e)
    {
        WindowViewModelBase windowViewModel = (WindowViewModelBase)Application.Current.Windows.OfType<MainWindow>().First().DataContext;
        ProfileFile CurrentProfile = ProfileCollection.GetActiveProfile();

        try
        {
            BitmapImage bitmapImage = new();
            bitmapImage.BeginInit();

            Image? image = await CurrentProfile.GetProfileImageAsync().ConfigureAwait(true);
            if (image is not null)
            {
                CurrentProfile.ProfileImage = image;
                CurrentProfile.Update();

                bitmapImage.StreamSource = new MemoryStream();
                await image.SaveAsPngAsync(bitmapImage.StreamSource).ConfigureAwait(true);
            }
            else if (windowViewModel.IsOffline || App.Client is null)     // Load saved
            {
                Image? savedImage = CurrentProfile.ProfileImage;
                if (savedImage is not null)
                {
                    bitmapImage.StreamSource = new MemoryStream();
                    await savedImage.SaveAsPngAsync(bitmapImage.StreamSource).ConfigureAwait(true);
                }
            }

            if (image is null && CurrentProfile.ShouldSerialize_ProfileImageEncoded())
            {
                CurrentProfile.ProfileImage = null;
                CurrentProfile.Update();
            }

            // Load default
            if (bitmapImage.StreamSource is null)
                return;

            bitmapImage.EndInit();
            ProfileImage = bitmapImage;
        }
        catch (Exception ex)
        {
            ex.HandleWithDefaultHandler(windowViewModel, "Load profile image");
        }
    }

    private void SwitchBtn_Click(object sender, RoutedEventArgs e) => RaiseEvent(new(SwitchEvent));

    private void DeleteBtn_Click(object sender, RoutedEventArgs e) => RaiseEvent(new(DeleteEvent));
}
