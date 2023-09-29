using Data.Profiles;
using System;
using System.CodeDom;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Animation;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;
using UntisDesktop.UserControls;
using UntisDesktop.ViewModels;
using Data.Timetable;
using UntisDesktop.Extensions;

namespace UntisDesktop.Views;

public partial class ProfileManageDialog : Window
{
    private ProfileManageDialogViewModel ViewModel { get => (ProfileManageDialogViewModel)DataContext; }

    public ProfileManageDialog()
    {
        InitializeComponent();

        foreach (ProfileFile profile in ProfileCollection.s_DefaultInstance)
        {
            ProfileControl control = new(profile);
            control.Switch += ProfileSwitchBtn_ClickAsync;
            control.Delete += ProfileDeleteBtn_Click;

            Profiles.Children.Add(control);
        }
    }

    private void ProfileAddBtn_Click(object sender, RoutedEventArgs e)
    {
        new LoginWindow().Show();
        DialogResult = true;
        e.Handled = true;
    }

    private async void ProfileSwitchBtn_ClickAsync(object sender, RoutedEventArgs e)
    {
        ProfileControl profileControl = (ProfileControl)sender;

        try
        {
            Mouse.OverrideCursor = Cursors.Wait;

            App.Client = await profileControl.ProfileFile.LoginAsync(CancellationToken.None);
            await ProfileCollection.SetActiveProfileAsync(profileControl.ProfileFile, App.Client);
            await (Application.Current.Windows
                .OfType<MainWindow>()
                .FirstOrDefault()?
                .UpdateTimetableAsync()
                ?? Task.CompletedTask);
        }
        catch (Exception ex)
        {
            ex.HandleWithDefaultHandler(ViewModel, "Change profile");
        }
        finally
        {
            Mouse.OverrideCursor = null;
        }

        DialogResult = true;
        e.Handled = true;
    }

    private void ProfileDeleteBtn_Click(object? sender, RoutedEventArgs e)
    {
        e.Handled = true;
    }

    private void ErrorBoxClose_Click(object sender, RoutedEventArgs e)
    {
        Storyboard popupAnimation = (Storyboard)ErrorBox.FindResource("PopUpAnimation");
        void OnCompletion(object? sender, EventArgs e)
        {
            Application.Current.Dispatcher.Invoke(() => ViewModel.ErrorBoxContent = string.Empty);
            popupAnimation.Completed -= OnCompletion;
        }

        popupAnimation.Completed += OnCompletion;
        popupAnimation.AutoReverse = true;
        popupAnimation.Begin();
        popupAnimation.Pause();
        popupAnimation.Seek(TimeSpan.FromSeconds(1));
        popupAnimation.Resume();
    }

    private void CloseBtn_Click(object sender, RoutedEventArgs e)
    {
        Close();
        e.Handled = true;
    }
}